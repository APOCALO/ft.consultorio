using ErrorOr;
using FluentValidation;
using MediatR;

namespace Ft.Consultorio.ServiceDefaults.Application.Behaviors
{
    /// <summary>
    /// MediatR pipeline behavior that runs FluentValidation against the request
    /// and short-circuits the pipeline with a materialized error response on failure.
    /// </summary>
    /// <remarks>
    /// Replaces the previous implementation that used a <c>(dynamic)</c> cast to
    /// convert a <see cref="List{Error}"/> into <typeparamref name="TResponse"/>. The
    /// DLR dispatch could fail at runtime with <c>RuntimeBinderException</c> for any
    /// TResponse that did not match the implicit conversion contract. The current
    /// implementation uses an injected <see cref="IValidationErrorMapper"/> which
    /// caches a typed delegate per TResponse and never touches the DLR.
    /// </remarks>
    public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
        where TResponse : IErrorOr
    {
        private readonly IValidator<TRequest>? _validator;
        private readonly IValidationErrorMapper _mapper;

        public ValidationBehavior(IValidator<TRequest>? validator = null, IValidationErrorMapper? mapper = null)
        {
            _validator = validator;
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            if (_validator is null)
            {
                return await next();
            }

            var validatorResult = await _validator.ValidateAsync(request, cancellationToken);

            if (validatorResult.IsValid)
            {
                return await next();
            }

            var errors = validatorResult.Errors.ConvertAll(vf =>
                Error.Validation(vf.PropertyName, vf.ErrorMessage));

            // Typed materialization (reflection-free after the first call per TResponse).
            // Throws InvalidOperationException with a clear message if TResponse is not registered.
            return (TResponse)_mapper.Materialize(typeof(TResponse), errors);
        }
    }
}
