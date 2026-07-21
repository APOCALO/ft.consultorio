# Stack Map - FtConsultorio Aspire

> Esta solución arrancó como una migración del microservicio de **auth** de Turho.
> Por ahora solo contiene ese microservicio; se irán agregando más con el mismo patrón.

## Component Dependency Diagram

```
              +----------------------+
              |        Client        |
              +----------+-----------+
                         |
              +----------v-----------+
              |       Gateway        |
              |      (YARP)          |
              |  HTTPS:8080 HTTP:8081|
              +----------+-----------+
                         |
              +------+-------+---------------+
              |              |               |
       +------v-----+  +-----v------------+  |
       |   MsAuth   |  | MsMedicalRecords |  |
       +------+-----+  +-----+------------+  |
              |              |               |
     +--------v----+   +-----v-----+   +-----v------+
     |  Postgres   |   | Postgres  |   |   Redis    |
     | (db-msauth) |   |(db-ms...) |   |  (cache)   |
     +-------------+   +-----------+   +------------+
     (cache/Redis lo usa solo MsAuth)
```

## Service Inventory

| Service | Aspire Name | Gateway Route | Postgres DB | Redis | Notes |
|---|---|---|---|---|---|
| MsAuth | `ftconsultorio-auth` | `/msauth/{**catch-all}` | `db-msauth` | Yes | Registro/login local, refresh tokens, OTP de email, reset de contraseña, login Google/Apple, Turnstile |
| MsMedicalRecords | `ftconsultorio-medicalrecords` | `/msmedicalrecords/{**catch-all}` | `db-msmedicalrecords` | No | Historias clínicas de fisioterapia (pacientes, historia, sesiones, pagos, dashboard). **Todos los endpoints solo rol `Admin`**. Sin caché. |

Todos los servicios usan la connection string `DatabaseConnection`.

### MsMedicalRecords (historias clínicas)

Basado en el patrón de MsCompanies (Clean Architecture + CQRS con MediatR), sin caché ni
mensajería. Agregados: `Patient` (raíz), `MedicalRecord` (uno por paciente, con `MedicalHistory`
como owned type), `Session`, `Payment`. Enums simples para `Gender`, `PatientStatus`,
`PaymentMethod` (los catálogos en tablas quedan como mejora futura).

Controllers (todos `[Authorize(Roles = "Admin")]`):
- `PatientsController` — CRUD de pacientes + `discharge` + `medical-record` (POST/GET) + `payments` (POST/GET) anidados.
- `MedicalRecordsController` — `PUT /{id}` + `sessions` (POST/GET) anidados.
- `SessionsController` — `PUT` / `DELETE` de sesiones.
- `DashboardController` — `GET` con conteos e ingresos (`Patients`, `ActivePatients`, `SessionsToday`, `IncomeThisMonth`, `PendingBalance`).

El servicio valida el JWT emitido por MsAuth (misma `ServiceDefaults:JwtSettings:PublicKeyPem`,
RS256); solo tiene la clave pública. El rol viaja como `ClaimTypes.Role` en el token.

Pendiente (siguiente iteración): `Evaluation` + `MuscleStrength`, `TreatmentPlan`,
`Appointment`, `Attachment` (fotos/PDF vía storage), y catálogos en tablas.

## Gateway Routing Model

Configurado en `Ft.Consultorio.Gateway/appsettings.json`:

- `/msauth/{**catch-all}` -> cluster `auth` (`https+http://ftconsultorio-auth`)

El transform quita el prefijo `/msauth` antes de reenviar. Las direcciones de destino
del cluster se resuelven en runtime con service discovery de Aspire
(`services__ftconsultorio-auth__https__0` / `__http__0`).

## AppHost Wiring

`Ft.Consultorio.AppHost/AppHost.cs` registra:

- Redis: `cache` (lo consume solo `MsAuth`)
- Postgres: `postgres` (puerto 5432, pgAdmin, volumen) con las bases `db-msauth` y `db-msmedicalrecords`
- Proyectos: `MsAuth`, `MsMedicalRecords`, `Gateway`
- El Gateway referencia a `MsAuth` y `MsMedicalRecords`.

El parámetro secreto `postgres-password` se toma de `Parameters:postgres-password`
(en `appsettings.Development.json` para dev; muévelo a user-secrets en otros entornos).

## Infrastructure Resources

### Redis (`cache`)
Caché de lecturas y limitadores de auth (`RefreshTokenCache`, `AuthAttemptLimiter`,
`EmailVerificationOtpResendLimiter`, `PasswordResetRequestLimiter`).

### PostgreSQL (`postgres`)
Base `db-msauth`. EF Core con Npgsql; migraciones aplicadas al arrancar en Development
(`ApplyMigrations()`). Tablas: `Users`, `Roles`, `UserRoles`, `UserSettings`,
`RefreshTokens`, `EmailVerificationOtps`, `PasswordResetTokens`, `OutboxMessages`.

## Auth surface (MsAuth)

Endpoints en `Controllers/V1/AuthController.cs`. Comandos en `Application/Auth/Commands`:

- `RegisterLocalUser`, `VerifyEmailOtp`, `ResendEmailOtp`
- `LoginLocalUser`, `RefreshToken`, `RevokeRefreshToken`
- `ForgotPassword`, `ResetPassword`
- `ExchangeGoogleToken`, `ExchangeAppleToken`

Servicios de infraestructura en `Infrastructure/Auth`: `JwtTokenService`,
`RefreshTokenService`, `EmailVerificationOtpService`, `PasswordResetTokenService`,
`GoogleIdTokenValidator`, `GoogleBirthDateResolver`, `AppleIdTokenValidator`,
`TurnstileVerifier`.

### Notificaciones (pendiente de microservicio)

Los flujos de OTP / reset / bienvenida usan el puerto `INotificationsApiClient`.
Mientras no exista MsNotifications, se registra `LoggingNotificationsApiClient`
(deja el contenido en el log). Al agregar MsNotifications, reemplazar el registro en
`DependencyInjection.AddNotificationsClient` por un `AddHttpClient` real con
`AddInternalApiKeyHandler()`.

## What `AddServiceDefaults()` Adds

Desde `Ft.Consultorio.ServiceDefaults/Extensions/Extensions.cs` (`ServiceDefaultsExtensions`):
Serilog, OpenTelemetry, health checks, API versioning + ApiExplorer, service discovery,
JWT auth, JSON defaults, ProblemDetails factory, forwarded headers, internal API key,
HttpClient resilience + service discovery, middleware global de excepciones y
ValidationBehavior.

## Guardrail Notes

- Endpoints internos protegidos con `[InternalOnly]` requieren `X-Internal-Api-Key`.
- El service discovery key de auth es `ftconsultorio-auth`.
- Persistencia es **PostgreSQL** (recurso `postgres`).
- Building blocks compartidos disponibles para futuros micros: `BuildingBlocks.Persistence`
  (Outbox + BaseRepository), `BuildingBlocks.Cqrs`, `BuildingBlocks.Caching`,
  `BuildingBlocks.Storage`, `SharedKernel`.
- Mantener este archivo actualizado cuando cambien servicios o rutas.
