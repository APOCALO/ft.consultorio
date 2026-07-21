using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Ft.Consultorio.ServiceDefaults.Application.Behaviors;

namespace Ft.Consultorio.ServiceDefaults.Extensions
{
    public static class ValidationBehaviorExtensions
    {
        /// <summary>
        /// Registers the validation pipeline behavior and the
        /// <see cref="IValidationErrorMapper"/> used to materialize validation errors
        /// into the handler's TResponse shape. The canonical
        /// <c>ErrorOr&lt;ApiResponse&lt;T&gt;&gt;</c> shape is auto-registered on first use;
        /// services with other response shapes can register additional factories by
        /// resolving <see cref="IValidationErrorMapper"/> at startup and calling
        /// <see cref="IValidationErrorMapper.Register"/>.
        /// </summary>
        public static IServiceCollection AddValidationBehaviorConfig(this IServiceCollection services)
        {
            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            // Singleton so the factory cache survives across requests.
            services.AddSingleton<IValidationErrorMapper, ValidationErrorMapper>();

            return services;
        }
    }
}
