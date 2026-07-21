using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Sinks.OpenTelemetry;

namespace Ft.Consultorio.ServiceDefaults.Extensions
{
    public static class SerilogExtensions
    {
        private const string SectionPath = "ServiceDefaults:SerilogSettings";

        public static TBuilder AddSerilog<TBuilder>(this TBuilder builder)
            where TBuilder : IHostApplicationBuilder
        {
            ArgumentNullException.ThrowIfNull(builder);

            // 1) Sección requerida (falla rápido si falta).
            var section = builder.Configuration.GetRequiredSection(SectionPath);

            // 2) Validación ligera sin binder (evita errores con objetos complejos en config).
            ValidateSerilogSection(section);

            // 3) Quita providers por defecto (evita doble log).
            builder.Logging.ClearProviders();

            // 4) Conecta Serilog al pipeline de logging (DI-friendly).
            var otlpEndpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];

            builder.Services.AddSerilog((sp, loggerConfig) =>
            {
                loggerConfig
                    .ReadFrom.Configuration(section)   // Lee desde ServiceDefaults:SerilogSettings.
                    .ReadFrom.Services(sp)             // Sinks/enrichers vía DI.
                    .Enrich.FromLogContext();          // Recomendado (y no estorba si ya está en config).

                // Envía logs al dashboard de Aspire vía OTLP.
                if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                {
                    loggerConfig.WriteTo.OpenTelemetry(options =>
                    {
                        options.Endpoint = otlpEndpoint;
                        options.Protocol = OtlpProtocol.Grpc;

                        var resourceAttributes = new Dictionary<string, object>
                        {
                            ["service.name"] = builder.Environment.ApplicationName
                        };

                        var otelResourceAttributes = builder.Configuration["OTEL_RESOURCE_ATTRIBUTES"];
                        if (!string.IsNullOrWhiteSpace(otelResourceAttributes))
                        {
                            foreach (var pair in otelResourceAttributes.Split(',', StringSplitOptions.RemoveEmptyEntries))
                            {
                                var parts = pair.Split('=', 2);
                                if (parts.Length == 2)
                                {
                                    resourceAttributes[parts[0].Trim()] = parts[1].Trim();
                                }
                            }
                        }

                        options.ResourceAttributes = resourceAttributes;
                    });
                }
            }, writeToProviders: true);

            return builder;
        }

        private static void ValidateSerilogSection(IConfiguration section)
        {
            var minLevel = section["MinimumLevel:Default"];
            if (string.IsNullOrWhiteSpace(minLevel))
            {
                throw new InvalidOperationException($"Invalid '{SectionPath}' configuration: MinimumLevel:Default is required.");
            }

            var hasWriteTo = section.GetSection("WriteTo").GetChildren().Any();
            if (!hasWriteTo)
            {
                throw new InvalidOperationException($"Invalid '{SectionPath}' configuration: WriteTo must have at least one sink.");
            }
        }
    }
}
