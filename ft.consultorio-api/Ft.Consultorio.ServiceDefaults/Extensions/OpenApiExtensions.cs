using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Ft.Consultorio.ServiceDefaults.Extensions
{
    public static class OpenApiExtensions
    {
        /// <summary>
        /// Genera documentos OpenAPI con el soporte nativo de .NET (Microsoft.AspNetCore.OpenApi),
        /// uno por versión de API. El documento "v1" incluye los endpoints del grupo "v1"
        /// que produce Asp.Versioning (GroupNameFormat 'v'VVV).
        /// </summary>
        public static IServiceCollection AddDefaultOpenApi(this IServiceCollection services, params string[] documentNames)
        {
            var documents = documentNames is { Length: > 0 } ? documentNames : ["v1"];

            foreach (var documentName in documents)
            {
                services.AddOpenApi(documentName);
            }

            return services;
        }

        /// <summary>
        /// Expone /openapi/{version}.json y la UI interactiva apuntando a esos documentos.
        /// Pensado para usarse solo en Development.
        /// </summary>
        public static WebApplication UseDefaultOpenApiUi(this WebApplication app)
        {
            app.MapOpenApi();

            var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
            app.UseSwaggerUI(options =>
            {
                foreach (var description in provider.ApiVersionDescriptions)
                {
                    options.SwaggerEndpoint($"/openapi/{description.GroupName}.json", description.GroupName.ToUpperInvariant());
                }
            });

            return app;
        }
    }
}
