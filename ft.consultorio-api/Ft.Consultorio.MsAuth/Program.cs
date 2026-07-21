using Ft.Consultorio.BuildingBlocks.Persistence.Outbox;
using System.Threading.RateLimiting;
using Ft.Consultorio.ServiceDefaults.Application.Common;
using Ft.Consultorio.ServiceDefaults.Extensions;
using Ft.Consultorio.ServiceDefaults.Web.Api.RateLimiting;
using Ft.Consultorio.ServiceDefaults.Infrastructure.Data;
using Ft.Consultorio.MsAuth;

var builder = WebApplication.CreateBuilder(args);

// Agregamos los servicios por defecto.
builder.AddServiceDefaults();

// Agregamos Redis desde los defaults.
builder.AddRedis();

// Agrega servicios al contenedor.

builder.Services.AddPresentation(builder.Configuration);

// Integración EF de Aspire: tracing, métricas y health check del DbContext.
// DisableRetry=true porque el servicio ya configura EnableRetryOnFailure.
builder.EnrichNpgsqlDbContext<Ft.Consultorio.MsAuth.Infrastructure.Data.ApplicationDbContext>(settings => settings.DisableRetry = true);

// Barrido de respaldo del Transactional Outbox (los eventos se despachan tras el commit;
// esto recoge los pendientes por crash o fallo transitorio).
builder.Services.AddOutboxProcessor<Ft.Consultorio.MsAuth.Infrastructure.Data.ApplicationDbContext>();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = (context, token) =>
    {
        var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
            .CreateLogger("RateLimiting");
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter =
                ((int)retryAfter.TotalSeconds).ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        logger.LogWarning(
            "Rate limit exceeded. Path={Path} Method={Method} RemoteIp={RemoteIp}",
            context.HttpContext.Request.Path,
            context.HttpContext.Request.Method,
            context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");

        return ValueTask.CompletedTask;
    };

    options.AddPolicy(RateLimitPolicyNames.AuthStrict, httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: RateLimitPartitionKeyFactory.ForIpAndPath(httpContext, RateLimitPolicyNames.AuthStrict),
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 8,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));

    options.AddPolicy(RateLimitPolicyNames.AuthModerate, httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: RateLimitPartitionKeyFactory.ForIpAndPath(httpContext, RateLimitPolicyNames.AuthModerate),
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 20,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
});

var app = builder.Build();

// Manejo global de excepciones (IExceptionHandler registrado en AddServiceDefaults).
app.UseExceptionHandler();

app.MapDefaultEndpoints();

// Configura el pipeline HTTP.
if (app.Environment.IsDevelopment())
{
    // OpenAPI nativo (/openapi/v1.json) + UI interactiva.
    app.UseDefaultOpenApiUi();

    // Aplicar las migraciones de la base de datos.
    app.ApplyMigrations();
}

app.UseForwardedHeaders();
app.UseHttpsRedirection();

app.UseRateLimiter();

// AuthN -> AuthZ.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
