# CLAUDE.md

Guía rápida para trabajar en este repositorio.

## Qué es

Solución .NET 10 + Aspire para FtConsultorio. Hoy contiene **un solo microservicio**:
`Ft.Consultorio.MsAuth` (autenticación), más el AppHost, el Gateway (YARP), los
ServiceDefaults y los building blocks compartidos. Se irán agregando más microservicios
con el mismo patrón.

## Antes de editar

Lee primero el skill del repo: `.claude/skills/project-context/` (`SKILL.md` y
`stack-map.md`). Contiene la topología, los contratos compartidos y las reglas de
arquitectura (Clean Architecture + CQRS con MediatR + FluentValidation, `ErrorOr<ApiResponse<T>>`).

## Reglas clave

- Handlers `internal sealed`, heredan de `ApiBaseHandler<TRequest, TResponse>`.
- Commands/queries heredan de `BaseResponse<T>`; controllers de `ApiBaseController`.
- Registrar DI en `DependencyInjection.cs`; `builder.AddServiceDefaults()` en cada `Program.cs`.
- Nunca `new HttpClient()`; usar `AddHttpClient` (+ `AddInternalApiKeyHandler()` para llamadas internas).
- Persistencia PostgreSQL (Npgsql). Migraciones con `dotnet dotnet-ef` (ver README).

## Build / verificación

```bash
dotnet build Ft.Consultorio.slnx -v minimal
dotnet run --project Ft.Consultorio.AppHost   # requiere Docker
```

## Notas

- Notificaciones: `LoggingNotificationsApiClient` (stub) hasta que exista MsNotifications.
- Secretos en `appsettings*.json` son heredados de la migración: rótalos y muévelos a
  user-secrets antes de cualquier entorno real.
