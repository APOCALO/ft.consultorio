using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ft.Consultorio.ServiceDefaults.Infrastructure.Auth;
using Ft.Consultorio.ServiceDefaults.Web.Api.ProblemDetails;
using Ft.Consultorio.ServiceDefaults.Infrastructure.Http;
using Ft.Consultorio.ServiceDefaults.Web.Api.ExceptionHandling;

namespace Ft.Consultorio.ServiceDefaults.Extensions;

// Agrega servicios comunes de Aspire: service discovery, resiliencia, health checks y OpenTelemetry.
// Este proyecto debe ser referenciado por cada servicio en tu solución.
// Más información en https://aka.ms/dotnet/aspire/service-defaults
public static class ServiceDefaultsExtensions
{
    public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.AddSerilog();

        builder.ConfigureOpenTelemetry();

        builder.AddDefaultHealthChecks();

        // API Versioning (segmento de URL + header + query) + ApiExplorer para Swagger.
        builder.Services.AddApiVersioningConfig();

        builder.Services.AddServiceDiscovery();

        builder.Services.AddJwtAuthentication(builder.Configuration);
        builder.AddDefaultJsonSerialization();

        builder.Services.AddProblemDetails();
        builder.Services.AddOptions<ApiBehaviorOptions>();
        builder.Services.AddSingleton<ProblemDetailsFactory, ApiBaseProblemDetailsFactory>();

        builder.AddForwardedHeaders();

        builder.AddInternalApiKey();

        builder.Services.AddSingleton<IHttpApiClient, HttpApiClient>();

        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            // Activa resiliencia por defecto.
            http.AddStandardResilienceHandler();

            // Activa service discovery por defecto.
            http.AddServiceDiscovery();
        });

        // Manejo global de excepciones (patrón nativo IExceptionHandler + ProblemDetails).
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

        // Esquemas permitidos en service discovery, configurables por entorno
        // (p. ej. ["https"] cuando todo el tráfico interno va con TLS).
        var allowedSchemes = builder.Configuration
            .GetSection("ServiceDefaults:ServiceDiscovery:AllowedSchemes")
            .Get<string[]>();
        if (allowedSchemes is { Length: > 0 })
        {
            builder.Services.Configure<Microsoft.Extensions.ServiceDiscovery.ServiceDiscoveryOptions>(
                options => options.AllowedSchemes = allowedSchemes);
        }

        return builder;
    }
}
