# FtConsultorio

Solución .NET Aspire para FtConsultorio. Arrancó como una **migración del microservicio
de autenticación** de Turho. Por ahora solo contiene ese microservicio (`MsAuth`); los
siguientes se irán agregando con el mismo patrón (Clean Architecture + CQRS).

## Estructura

```
Ft.Consultorio.AppHost/                 Orquestador Aspire (Postgres, Redis, MsAuth, Gateway)
Ft.Consultorio.Gateway/                 Reverse proxy YARP (ruta /msauth)
Ft.Consultorio.ServiceDefaults/         Cross-cutting compartido (JWT, OTel, Serilog, etc.)
Ft.Consultorio.MsAuth/                  Microservicio de autenticación
Ft.Consultorio.SharedKernel/            Primitivas de dominio compartidas
Ft.Consultorio.BuildingBlocks.Persistence/   EventPublishingDbContext + Outbox + BaseRepository
Ft.Consultorio.BuildingBlocks.Cqrs/     MediatR + FluentValidation (ValidationBehavior)
Ft.Consultorio.BuildingBlocks.Caching/  Redis cache + distributed lock
Ft.Consultorio.BuildingBlocks.Storage/  S3/R2 (disponible para futuros micros)
```

## Microservicio de Auth (`MsAuth`)

Ruta a través del Gateway: `/msauth/...` (el Gateway quita el prefijo antes de reenviar).

Flujos incluidos (`Controllers/V1/AuthController.cs`):

- **Registro local con verificación por OTP de email**: `RegisterLocalUser` → `VerifyEmailOtp` / `ResendEmailOtp`
- **Login local**: `LoginLocalUser`
- **Refresh tokens**: `RefreshToken`, `RevokeRefreshToken`
- **Restablecimiento de contraseña**: `ForgotPassword`, `ResetPassword`
- **Login social**: `ExchangeGoogleToken`, `ExchangeAppleToken`
- Protección con **Cloudflare Turnstile** y rate limiting por IP/ruta.

Persistencia: PostgreSQL (`db-msauth`) vía EF Core (Npgsql). Tablas: `Users`, `Roles`,
`UserRoles`, `UserSettings`, `RefreshTokens`, `EmailVerificationOtps`,
`PasswordResetTokens`, `OutboxMessages`.

### Notificaciones (pendiente)

Los correos (OTP, enlace de reset, bienvenida) se envían a través del puerto
`INotificationsApiClient`. Mientras no exista un microservicio de notificaciones, se
registra `LoggingNotificationsApiClient`, que **deja el contenido en el log** en lugar de
enviarlo. Al agregar `MsNotifications`, reemplazar el registro en
`DependencyInjection.AddNotificationsClient` por un cliente HTTP real.

## Requisitos

- .NET SDK 10
- Docker (Aspire levanta Postgres y Redis en contenedores)

## Cómo ejecutar

```bash
# Restaurar herramientas locales (dotnet-ef)
dotnet tool restore

# Levantar todo con Aspire (Postgres + Redis + MsAuth + Gateway)
dotnet run --project Ft.Consultorio.AppHost
```

El dashboard de Aspire muestra las URLs. El Gateway expone HTTPS en `:8080` y HTTP en `:8081`.
En Development las migraciones de EF se aplican automáticamente al arrancar.

### Migraciones EF

```bash
dotnet dotnet-ef migrations add <Nombre> \
  --project Ft.Consultorio.MsAuth/Ft.Consultorio.MsAuth.csproj \
  --startup-project Ft.Consultorio.MsAuth/Ft.Consultorio.MsAuth.csproj \
  -o Migrations
```

## Seguridad / configuración

> ⚠️ **Importante:** `appsettings*.json` de `MsAuth` y del `AppHost` contienen valores de
> ejemplo/secretos heredados de la migración (clave JWT, password de Postgres, Turnstile,
> Google Client ID). Antes de usar en cualquier entorno real, **rota estos secretos** y
> muévelos a *user-secrets* o variables de entorno:

```bash
dotnet user-secrets set "ServiceDefaults:JwtSettings:PrivateKeyPem" "<...>" \
  --project Ft.Consultorio.MsAuth
dotnet user-secrets set "Auth:Turnstile:SecretKey" "<...>" \
  --project Ft.Consultorio.MsAuth
```

## Convenciones

Ver el skill `.claude/skills/project-context` (`SKILL.md` + `stack-map.md`) para la
arquitectura, patrones CQRS y reglas del repositorio.
