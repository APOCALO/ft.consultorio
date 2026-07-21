using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Ft.Consultorio.ServiceDefaults.Extensions;

public static class ForwardedHeadersExtensions
{
    private const string SectionPath = "ServiceDefaults:ForwardedHeaders";

    // Redes confiables por defecto: loopback + rangos privados (RFC 1918 / ULA).
    // Cubre el gateway corriendo en localhost (dev) o en la red interna del clúster/Docker.
    private static readonly string[] DefaultKnownNetworks =
    [
        "127.0.0.0/8",
        "::1/128",
        "10.0.0.0/8",
        "172.16.0.0/12",
        "192.168.0.0/16",
        "fc00::/7"
    ];

    public static IHostApplicationBuilder AddForwardedHeaders(this IHostApplicationBuilder builder)
    {
        var section = builder.Configuration.GetSection(SectionPath);

        var configuredNetworks = section.GetSection("KnownNetworks").Get<string[]>();
        var networks = configuredNetworks is { Length: > 0 } ? configuredNetworks : DefaultKnownNetworks;

        // Un salto por defecto (el gateway). Configurable si hay más proxies (LB, CDN).
        var forwardLimit = section.GetValue<int?>("ForwardLimit") ?? 1;

        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.ForwardLimit = forwardLimit;
            options.KnownProxies.Clear();
            options.KnownIPNetworks.Clear();

            foreach (var network in networks)
            {
                options.KnownIPNetworks.Add(System.Net.IPNetwork.Parse(network));
            }
        });

        return builder;
    }
}
