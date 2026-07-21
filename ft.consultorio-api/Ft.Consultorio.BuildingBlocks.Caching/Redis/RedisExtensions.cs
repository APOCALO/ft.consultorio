using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ft.Consultorio.ServiceDefaults.Application.Interfaces;
using Ft.Consultorio.ServiceDefaults.Infrastructure.Caching;

namespace Ft.Consultorio.ServiceDefaults.Extensions;

public static class RedisExtensions
{
    public static IHostApplicationBuilder AddRedis(
        this IHostApplicationBuilder builder,
        string connectionName = "cache")
    {
        // Aspire configura automáticamente IConnectionMultiplexer.
        builder.AddRedisClient(connectionName);

        // Registra tus servicios personalizados.
        builder.Services.AddSingleton<IRedisCacheService, RedisCacheService>();
        builder.Services.AddSingleton<IDistributedLock, RedisDistributedLock>();

        return builder;
    }
}
