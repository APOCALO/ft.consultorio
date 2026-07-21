using Ft.Consultorio.MsMedicalRecords;
using Ft.Consultorio.MsMedicalRecords.Infrastructure.Data;
using Ft.Consultorio.ServiceDefaults.Extensions;
using Ft.Consultorio.ServiceDefaults.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Servicios por defecto (Serilog, OTel, health checks, versioning, JWT, ProblemDetails, etc.).
builder.AddServiceDefaults();

// Servicios de la aplicación (controllers, MediatR, validación, persistencia).
builder.Services.AddPresentation(builder.Configuration);

// Integración EF de Aspire: tracing, métricas y health check del DbContext.
// DisableRetry=true porque el servicio ya configura EnableRetryOnFailure.
builder.EnrichNpgsqlDbContext<ApplicationDbContext>(settings => settings.DisableRetry = true);

var app = builder.Build();

// Manejo global de excepciones (IExceptionHandler registrado en AddServiceDefaults).
app.UseExceptionHandler();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseDefaultOpenApiUi();
    app.ApplyMigrations();
}

app.UseForwardedHeaders();
app.UseHttpsRedirection();

// AuthN -> AuthZ.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Expuesto para pruebas.
public partial class Program { }
