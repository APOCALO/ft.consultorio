using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ft.Consultorio.ServiceDefaults.Infrastructure.Http;
using Ft.Consultorio.ServiceDefaults.Infrastructure.Settings;

namespace Ft.Consultorio.ServiceDefaults.Extensions
{
    public static class InternalHttpExtensions
    {
        /// <summary>
        /// Registra InternalApiKeyOptions desde configuracion y el DelegatingHandler en DI.
        /// </summary>
        public static TBuilder AddInternalApiKey<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
        {
            builder.Services.Configure<InternalApiKeyOptions>(
                builder.Configuration.GetSection(InternalApiKeyOptions.SectionName));

            builder.Services.AddTransient<InternalApiKeyHandler>();

            return builder;
        }

        /// <summary>
        /// Agrega el InternalApiKeyHandler a un HttpClient especifico.
        /// </summary>
        public static IHttpClientBuilder AddInternalApiKeyHandler(this IHttpClientBuilder httpClientBuilder)
        {
            return httpClientBuilder.AddHttpMessageHandler<InternalApiKeyHandler>();
        }
    }
}
