using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using ErrorOr;
using Ft.Consultorio.ServiceDefaults.Application.Common;

namespace Ft.Consultorio.ServiceDefaults.Application.Behaviors
{
    /// <summary>
    /// Materializes a list of validation errors into the concrete TResponse shape used by a
    /// MediatR handler. The mapper is keyed by the runtime TResponse type and caches a
    /// compiled delegate per type, so the hot path is reflection-free.
    /// </summary>
    /// <remarks>
    /// Replaces the previous <c>(dynamic)errors</c> cast in <see cref="ValidationBehavior{TRequest,TResponse}"/>,
    /// which caused <c>Microsoft.CSharp.RuntimeBinder.RuntimeBinderException</c> for any TResponse
    /// that did not match the implicit DLR conversion contract.
    /// </remarks>
    public interface IValidationErrorMapper
    {
        /// <summary>
        /// Registers a factory for a specific TResponse shape at startup.
        /// Calling this more than once for the same <paramref name="responseType"/> throws.
        /// </summary>
        void Register(Type responseType, Func<IReadOnlyList<Error>, object> factory);

        /// <summary>
        /// Materializes <paramref name="errors"/> into the requested TResponse shape.
        /// Throws <see cref="InvalidOperationException"/> if no factory is registered and
        /// the type is not the canonical <c>ErrorOr&lt;ApiResponse&lt;T&gt;&gt;</c> shape.
        /// </summary>
        object Materialize(Type responseType, IReadOnlyList<Error> errors);
    }

    /// <summary>
    /// Default implementation. Caches one compiled delegate per response type so
    /// the per-request cost is a dictionary lookup plus a delegate invocation.
    /// Auto-registers the canonical <c>ErrorOr&lt;ApiResponse&lt;T&gt;&gt;</c> shape
    /// on first use (one-time Expression compilation per T).
    /// </summary>
    public sealed class ValidationErrorMapper : IValidationErrorMapper
    {
        private readonly ConcurrentDictionary<Type, Func<IReadOnlyList<Error>, object>> _factories = new();

        public void Register(Type responseType, Func<IReadOnlyList<Error>, object> factory)
        {
            ArgumentNullException.ThrowIfNull(responseType);
            ArgumentNullException.ThrowIfNull(factory);

            if (!_factories.TryAdd(responseType, factory))
            {
                throw new InvalidOperationException(
                    $"A validation error factory is already registered for response type '{responseType.FullName}'.");
            }
        }

        public object Materialize(Type responseType, IReadOnlyList<Error> errors)
        {
            ArgumentNullException.ThrowIfNull(responseType);
            ArgumentNullException.ThrowIfNull(errors);

            // Fast path: known factory.
            if (_factories.TryGetValue(responseType, out var factory))
            {
                return factory(errors);
            }

            // Auto-registration path: cualquier ErrorOr<T> (incluido ErrorOr<ApiResponse<T>>).
            // El operador implícito List<Error> -> ErrorOr<T> existe para todo T, así que
            // la fábrica compilada funciona con cualquier forma ErrorOr.
            if (IsErrorOrShape(responseType))
            {
                var innerType = responseType.GenericTypeArguments[0];
                var newFactory = ErrorOrMaterializer.BuildErrorOrFactory(innerType);
                if (_factories.TryAdd(responseType, newFactory))
                {
                    return newFactory(errors);
                }
                // Lost the race: another thread registered first. Use the winner's factory.
                return _factories[responseType](errors);
            }

            throw new InvalidOperationException(
                $"No IValidationErrorFactory registered for response type '{responseType.FullName}'. " +
                "Register one at startup via IValidationErrorMapper.Register(), or change the " +
                "handler to return an ErrorOr<T> shape which is auto-registered on first use.");
        }

        private static bool IsErrorOrShape(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ErrorOr<>);
        }
    }

    /// <summary>
    /// Helper that compiles a closed delegate of the form
    /// <c>Func&lt;IReadOnlyList&lt;Error&gt;, ErrorOr&lt;TInner&gt;&gt;</c> using Expression trees,
    /// then boxes the result as <see cref="object"/> so the open-generic registration works.
    /// </summary>
    internal static class ErrorOrMaterializer
    {
        /// <summary>
        /// Builds a factory for <c>ErrorOr&lt;TInner&gt;</c>. The returned delegate
        /// returns the value boxed as <see cref="object"/>.
        /// </summary>
        public static Func<IReadOnlyList<Error>, object> BuildErrorOrFactory(Type innerType)
        {
            // ErrorOr<T> exposes `op_Implicit(List<Error>) -> ErrorOr<T>`. We resolve the
            // closed operator and compile an expression that materializes the IReadOnlyList
            // into a List<Error> (which is what the operator requires), then boxes the result.
            var errorOrClosed = typeof(ErrorOr<>).MakeGenericType(innerType);
            var implicitMethod = errorOrClosed
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .FirstOrDefault(m => m.Name == "op_Implicit"
                                  && m.GetParameters().Length == 1
                                  && m.GetParameters()[0].ParameterType == typeof(List<Error>))
                ?? throw new InvalidOperationException(
                    $"ErrorOr<{innerType.FullName}>.op_Implicit(List<Error>) was not found.");

            // Expression: (IReadOnlyList<Error> errors) =>
            //     (object) ErrorOr<TInner>.op_Implicit(errors.ToList())
            var errorsParam = Expression.Parameter(typeof(IReadOnlyList<Error>), "errors");
            var toListCall = Expression.Call(
                typeof(Enumerable),
                nameof(Enumerable.ToList),
                new[] { typeof(Error) },
                Expression.Convert(errorsParam, typeof(IEnumerable<Error>)));
            var implicitCall = Expression.Call(implicitMethod, toListCall);
            var boxedExpr = Expression.Convert(implicitCall, typeof(object));
            var lambda = Expression.Lambda<Func<IReadOnlyList<Error>, object>>(boxedExpr, errorsParam);

            return lambda.Compile();
        }
    }
}
