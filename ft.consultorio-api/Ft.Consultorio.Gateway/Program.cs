using Ft.Consultorio.ServiceDefaults.Extensions;
using System.Net;
using System.Net.Security;
using System.Security.Authentication;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

// Observabilidad del punto de entrada: logs estructurados (Serilog→OTLP),
// trazas/métricas OpenTelemetry y health checks. No usamos AddServiceDefaults
// completo porque el gateway no necesita JWT, MVC, versionado ni OpenAPI.
builder.AddSerilog();
builder.ConfigureOpenTelemetry();
builder.AddDefaultHealthChecks();

builder.Services.AddHttpClient();
builder.Services.ConfigureHttpClientDefaults(http =>
{
    http.ConfigurePrimaryHttpMessageHandler(() => CreateGatewayHandler(builder.Environment.IsDevelopment()));
});

// Service discovery nativo: los destinos del proxy usan nombres lógicos
// (https+http://ftconsultorio-*) que Aspire resuelve vía variables services__*.
builder.Services.AddServiceDiscovery();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("GatewayCors", policy =>
    {
        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
        else
        {
            policy.SetIsOriginAllowed(_ => false);
        }
    });
});

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddServiceDiscoveryDestinationResolver()
    // Seguridad: elimina de TODO request entrante los headers de confianza interna.
    // Solo el InternalApiKeyHandler puede inyectar X-Internal-Api-Key en llamadas
    // servicio-a-servicio; si un cliente externo lo envía a través del gateway, se
    // descarta antes de reenviar, evitando alcanzar endpoints [InternalOnly].
    .AddTransforms(context => context.AddRequestHeaderRemove("X-Internal-Api-Key"));

builder.AddForwardedHeaders();

var app = builder.Build();

app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.Use(async (context, next) =>
{
    var headers = context.Response.Headers;
    headers["X-Content-Type-Options"] = "nosniff";
    headers["X-Frame-Options"] = "DENY";
    headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
    headers["X-XSS-Protection"] = "0";
    await next();
});

app.UseRouting();
app.UseCors("GatewayCors");

// Endpoints de health (/health, /alive, /health/live, /health/ready).
app.MapDefaultEndpoints();

app.MapReverseProxy().RequireCors("GatewayCors");

app.Run();

static SocketsHttpHandler CreateGatewayHandler(bool allowInsecureDevCertificates)
{
    var handler = new SocketsHttpHandler
    {
        PooledConnectionLifetime = TimeSpan.FromMinutes(5),
        PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),
        ConnectTimeout = TimeSpan.FromSeconds(10),
        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
        EnableMultipleHttp2Connections = true,
        UseCookies = false,
        SslOptions = new SslClientAuthenticationOptions
        {
            EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13
        }
    };

    if (allowInsecureDevCertificates)
    {
        handler.SslOptions.RemoteCertificateValidationCallback = (_, _, _, _) => true;
    }

    return handler;
}
