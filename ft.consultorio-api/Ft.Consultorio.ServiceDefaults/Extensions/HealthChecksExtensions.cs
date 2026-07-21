using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ft.Consultorio.ServiceDefaults.Extensions
{
    public static class HealthChecksExtensions
    {
        private static readonly DateTimeOffset StartedAtUtc = DateTimeOffset.UtcNow;

        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
        {
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public static TBuilder AddDefaultHealthChecks<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
        {
            // Liveness: el proceso responde.
            // Las dependencias (SQL, Redis) registran sus health checks vía las
            // integraciones de Aspire (EnrichNpgsqlDbContext / AddRedisClient),
            // no manualmente aquí, para evitar pings duplicados por probe.
            builder.Services
                .AddHealthChecks()
                .AddCheck(
                    name: "self",
                    check: () => HealthCheckResult.Healthy("OK"),
                    tags: new[] { "live" });

            // Las integraciones de Aspire registran sus checks sin tags. Para que las
            // readiness probes (/health/ready) los incluyan, todo check que no sea de
            // liveness se etiqueta como "ready". PostConfigure garantiza que corre
            // después de todas las registraciones, incluidas las de Enrich/AddRedisClient.
            builder.Services.PostConfigure<HealthCheckServiceOptions>(options =>
            {
                foreach (var registration in options.Registrations)
                {
                    if (!registration.Tags.Contains("live") && !registration.Tags.Contains("ready"))
                    {
                        registration.Tags.Add("ready");
                    }
                }
            });

            return builder;
        }

        /// <summary>
        /// Mapea los endpoints de health como endpoints de routing (única fuente de verdad):
        ///  - /health        → todos los checks (debug/observabilidad).
        ///  - /alive y /health/live → solo checks con tag "live" (liveness probe).
        ///  - /health/ready  → solo checks con tag "ready" (readiness probe).
        /// </summary>
        public static WebApplication MapHealthCheckEndpoints(this WebApplication app)
        {
            app.MapHealthChecks("/health", BuildOptions(_ => true));
            app.MapHealthChecks("/alive", BuildOptions(r => r.Tags.Contains("live")));
            app.MapHealthChecks("/health/live", BuildOptions(r => r.Tags.Contains("live")));
            app.MapHealthChecks("/health/ready", BuildOptions(r => r.Tags.Contains("ready")));

            return app;
        }

        private static HealthCheckOptions BuildOptions(Func<HealthCheckRegistration, bool> predicate) => new()
        {
            Predicate = predicate,
            AllowCachingResponses = false,
            ResultStatusCodes =
            {
                [HealthStatus.Healthy] = StatusCodes.Status200OK,
                [HealthStatus.Degraded] = StatusCodes.Status200OK,
                [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable,
            },
            ResponseWriter = WriteResponseAsync
        };

        private static async Task WriteResponseAsync(HttpContext context, HealthReport report)
        {
            var env = context.RequestServices.GetService<IHostEnvironment>();
            var includeException = env?.IsDevelopment() == true;

            var payload = new
            {
                status = report.Status.ToString(),
                startedAtUtc = StartedAtUtc,
                timestampUtc = DateTimeOffset.UtcNow,
                totalDurationMs = report.TotalDuration.TotalMilliseconds,
                checks = report.Entries.Select(e => new
                {
                    component = e.Key,
                    status = e.Value.Status.ToString(),
                    description = e.Value.Description,
                    durationMs = e.Value.Duration.TotalMilliseconds,
                    tags = e.Value.Tags,
                    data = e.Value.Data is { Count: > 0 } ? e.Value.Data : null,
                    exception = includeException ? e.Value.Exception?.Message : null
                })
            };

            context.Response.ContentType = "application/json; charset=utf-8";
            await context.Response.WriteAsync(JsonSerializer.Serialize(payload, JsonOptions));
        }
    }
}
