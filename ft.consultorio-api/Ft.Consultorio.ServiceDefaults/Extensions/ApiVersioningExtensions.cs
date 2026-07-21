using Asp.Versioning;
using Microsoft.Extensions.DependencyInjection;
namespace Ft.Consultorio.ServiceDefaults.Extensions
{
    public static class ApiVersioningExtensions
    {
        public static IServiceCollection AddApiVersioningConfig(this IServiceCollection services)
        {
            services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = ApiVersionReader.Combine(new UrlSegmentApiVersionReader(), new HeaderApiVersionReader("api-version"), new QueryStringApiVersionReader("api-version"));
            }).AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });
            services.AddDefaultOpenApi();
            return services;
        }
    }
}