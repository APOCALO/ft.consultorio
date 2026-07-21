using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Ft.Consultorio.ServiceDefaults.Extensions
{
    public static class OpenTelemetryExtensions
    {
        private const string HealthEndpointPath = "/health";
        private const string AlivenessEndpointPath = "/alive";

        public static TBuilder ConfigureOpenTelemetry<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
        {
            // Los logs se envían vía Serilog.Sinks.OpenTelemetry (ver SerilogExtensions).
            // No se agrega logging.AddOpenTelemetry() aquí para evitar duplicación.

            builder.Services.AddOpenTelemetry()
                .WithMetrics(metrics =>
                {
                    metrics.AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddRuntimeInstrumentation();
                })
                .WithTracing(tracing =>
                {
                    tracing.AddSource(builder.Environment.ApplicationName)
                        .AddAspNetCoreInstrumentation(tracing =>
                            // Excluye las peticiones de health checks del tracing.
                            tracing.Filter = context =>
                                !context.Request.Path.StartsWithSegments(HealthEndpointPath)
                                && !context.Request.Path.StartsWithSegments(AlivenessEndpointPath)
                        )
                        // Descomenta la siguiente línea para habilitar la instrumentación gRPC (requiere el paquete OpenTelemetry.Instrumentation.GrpcNetClient).
                        //.AddGrpcClientInstrumentation()
                        .AddHttpClientInstrumentation();
                });

            builder.AddOpenTelemetryExporters();

            return builder;
        }

        private static TBuilder AddOpenTelemetryExporters<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
        {
            var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

            if (useOtlpExporter)
            {
                builder.Services.AddOpenTelemetry().UseOtlpExporter();
            }

            var appInsightsConnectionString = builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];
            if (!string.IsNullOrWhiteSpace(appInsightsConnectionString))
            {
                builder.Services.AddOpenTelemetry().UseAzureMonitor();
            }

            return builder;
        }

        public static WebApplication MapDefaultEndpoints(this WebApplication app)
        {
            // Única fuente de verdad de los endpoints de health (ver HealthChecksExtensions).
            // En producción Azure necesita estos endpoints para health probes del load balancer / App Service.
            return app.MapHealthCheckEndpoints();
        }
    }
}
