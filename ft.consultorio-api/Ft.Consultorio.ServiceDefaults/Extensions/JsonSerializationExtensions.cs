using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using HttpJsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;
using MvcJsonOptions = Microsoft.AspNetCore.Mvc.JsonOptions;

namespace Ft.Consultorio.ServiceDefaults.Extensions;

public static class JsonSerializationExtensions
{
    public static IHostApplicationBuilder AddDefaultJsonSerialization(this IHostApplicationBuilder builder)
    {
        builder.Services.Configure<HttpJsonOptions>(options => Configure(options.SerializerOptions));
        builder.Services.Configure<MvcJsonOptions>(options => Configure(options.JsonSerializerOptions));

        return builder;
    }

    private static void Configure(JsonSerializerOptions options)
    {
        options.PropertyNameCaseInsensitive = true;
        options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    }
}
