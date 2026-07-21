---
name: project-context
description: Onboard and work effectively in the FtConsultorio Aspire repository. Use when adding or changing any microservice, wiring AppHost and Gateway routes, implementing CQRS handlers/controllers, configuring ServiceDefaults, adding inter-service HTTP clients, or validating FtConsultorio architecture and conventions.
---

# FtConsultorio Aspire - Project Context

Use this skill as the first context pass before editing any project in this repository.

## Read Order

1. Read `stack-map.md` first for topology, service dependencies, and infra resources.
   - If `stack-map.md` is missing, use `Ft.Consultorio.AppHost/AppHost.cs` and `Ft.Consultorio.Gateway/appsettings.json` as topology source of truth.
2. Open only the entry files needed for the current task.
3. Expand into service-specific folders after identifying the exact service boundary.

## Repository Topology

`Ft.Consultorio.slnx` uses virtual folders (`/aspire`, `/src`, `/test`), but projects are physically in repository root.

- `Ft.Consultorio.AppHost/` - Aspire orchestrator (`AppHost.cs`)
- `Ft.Consultorio.Gateway/` - YARP reverse proxy
- `Ft.Consultorio.ServiceDefaults/` - shared architecture and infra extensions
- `Ft.Consultorio.MsAuth/` — microservicio de autenticación (auth)
- `Ft.Consultorio.MsMedicalRecords/` — historias clínicas (solo rol Admin; sin caché)
- Futuros microservicios se agregan con el mismo patrón (Clean Architecture + CQRS).
- Shared building blocks: `Ft.Consultorio.BuildingBlocks.Persistence` (EventPublishingDbContext + Outbox + BaseRepository), `Ft.Consultorio.BuildingBlocks.Cqrs`, `Ft.Consultorio.BuildingBlocks.Caching` (Redis cache + distributed lock), `Ft.Consultorio.BuildingBlocks.Storage` (S3/R2), `Ft.Consultorio.SharedKernel`

Do not assume there is a physical `/src` or `/test` folder in disk layout.

## Core Architecture

- Runtime: `.NET 10` (`net10.0`)
- Orchestration: `Aspire.AppHost.Sdk` + `Aspire.Hosting.*`
- Gateway: `Yarp.ReverseProxy`
- Service style: Clean Architecture + CQRS (`MediatR`) + `FluentValidation`
- Error flow: `ErrorOr<ApiResponse<T>>`
- Shared HTTP defaults: resilience + service discovery from `ServiceDefaults`

### Shared Building Blocks (ServiceDefaults)

Use these as repository-wide contracts:

- `Application/Common/BaseResponse.cs` — namespace `Application.Common`
- `Application/Common/ApiBaseHandler.cs` — namespace `Application.Common`
- `Application/Common/ApiResponse.cs` — namespace `Application.Common`
- `Application/Behaviors/ValidationBehavior.cs` — namespace `Application.Behaviors`
- `Application/Validators/FileValidator.cs` — namespace `Application.Validators`
- `Web.Api/Controllers/ApiBaseController.cs`
- `Web.Api/Attributes/InternalOnlyAttribute.cs` — namespace `Web.Api.Attributes`
- `Web.Api/Attributes/IdempotentAttribute.cs` — namespace `Web.Api.Attributes`
- `Web.Api/Constants/HttpContextItemKeys.cs` — namespace `Web.Api.Constants`
- `Web.Api/ProblemDetails/ApiBaseProblemDetailsFactory.cs` — namespace `Web.Api.ProblemDetails`
- `Web.Api/RateLimiting/RateLimitPolicyNames.cs` — namespace `Web.Api.RateLimiting`
- `Web.Api/RateLimiting/OutputCachePolicyNames.cs` — namespace `Web.Api.RateLimiting`
- `Infrastructure/Caching/RedisCacheService.cs` — namespace `Infrastructure.Caching`
- `Infrastructure/Storage/S3FileStorageService.cs` — namespace `Infrastructure.Storage`
- `Infrastructure/Storage/StorageExtensions.cs` — `AddS3Storage()` — namespace `Infrastructure.Storage`
- `Infrastructure/Data/MigrationExtensions.cs` — `ApplyMigrations()` — namespace `Infrastructure.Data`
- `Infrastructure/Http/InternalApiKeyHandler.cs` — namespace `Infrastructure.Auth`
- `Extensions/Extensions.cs` — class `ServiceDefaultsExtensions`, `AddServiceDefaults()`
- `Extensions/InternalHttpExtensions.cs` — `AddInternalApiKeyHandler()`

## Primary Entry Points By Task

Use this map to minimize file discovery time:

- App orchestration
  - `Ft.Consultorio.AppHost/AppHost.cs`
- Gateway routing and cluster address rewrites
  - `Ft.Consultorio.Gateway/appsettings.json`
  - `Ft.Consultorio.Gateway/Program.cs`
- Shared defaults and cross-cutting concerns
  - `Ft.Consultorio.ServiceDefaults/Extensions/Extensions.cs`
- Per-service startup and DI
  - `Ft.Consultorio.Ms*/Program.cs`
  - `Ft.Consultorio.Ms*/DependencyInjection.cs`
- Cross-service clients
  - `Ft.Consultorio.Ms*/Infrastructure/Clients/*.cs`

## Standard Microservice Pattern

Cada microservicio (`MsAuth`, `MsMedicalRecords`, …) sigue esta estructura:

- `Domain/`
- `Application/` (commands, queries, DTOs, mappings, interfaces)
- `Infrastructure/` (persistence, repositories, clients, settings)
- `Controllers/V1/`
- `DependencyInjection.cs`
- `Program.cs`

## Operational Rules

### Always

- Inherit commands/queries from `BaseResponse<T>`.
- Inherit handlers from `ApiBaseHandler<TRequest, TResponse>`.
- Inherit controllers from `ApiBaseController`.
- Return `ErrorOr<ApiResponse<T>>` from handlers.
- Keep handlers `internal sealed`.
- Register service dependencies in `DependencyInjection.cs`.
- Call `builder.AddServiceDefaults()` in each `Program.cs`.
- Pass `CancellationToken` through async flows.
- Use `DateTime.UtcNow` for audit/timestamps.

### Never

- Put business logic in controllers.
- Use `new HttpClient()` directly.
- Skip validators for request models.
- Return domain entities from API endpoints.
- Scatter DI registrations across arbitrary files.

## Inter-Service HTTP Pattern

Use two auth models depending on call type:

1. User-context forwarding
- Example: `MsBookings -> MsCompanies /api/v1/companies/{id}/is-owner`
- Forward `Authorization` header from incoming request.

2. Internal service-to-service auth
- Use `[InternalOnly]` on internal endpoints.
- Attach `X-Internal-Api-Key` via `.AddInternalApiKeyHandler()` on HttpClient.
- Current examples: `MsRecommendations -> MsBookings internal endpoints`, `MsCompanies -> MsSquoosh`.

## Workflow: Add Or Change A Service Endpoint

1. Implement request and handler in `Application/.../Commands` or `Queries`.
2. Add validator in the same feature folder.
3. Update Mapperly mapper(s) in `Application/Mapping` or `Application/Mappings` if DTO transforms change (do not create `AutoMapper` profiles).
4. Expose endpoint in `Controllers/V1` and keep versioned route format.
5. Register dependencies in `DependencyInjection.cs`.
6. Validate startup pipeline order in `Program.cs` (auth and middleware order).

## Mapping Conventions

- Use `Riok.Mapperly` for object mapping in microservices.
- Define mapper interfaces and implementations per module/feature (for example `ICompaniesMapper`, `CompaniesMapper`).
- Register mappers explicitly in `DependencyInjection.cs` (for example `AddSingleton<IModuleMapper, ModuleMapper>()`).
- Prefer `RequiredMappingStrategy = RequiredMappingStrategy.Target` and explicit ignores for computed/enriched fields.
- For mappings with custom fallback/default logic, wrap generated partial methods with small manual methods to preserve behavior.

## Workflow: Add A New Cross-Service Call

1. Define typed client interface + implementation in caller `Infrastructure/Clients`.
2. Register `AddHttpClient` in caller `DependencyInjection.cs`.
3. Add `.AddInternalApiKeyHandler()` when target endpoint uses `[InternalOnly]`.
4. Configure base URL through service discovery names and options.
5. Handle failures with graceful fallback where business-safe.

## Workflow: Add A New Microservice

1. Create project with target `net10.0`.
2. Reference `Ft.Consultorio.ServiceDefaults`.
3. Add `ApplicationAssemblyReference.cs`, `DependencyInjection.cs`, `Program.cs`.
4. Register project in `Ft.Consultorio.slnx`.
5. Add project reference + service registration in `Ft.Consultorio.AppHost/AppHost.cs` and `.csproj`.
6. Add Gateway route/cluster in `Ft.Consultorio.Gateway/appsettings.json` if externally exposed.
7. Add cluster address override in `Ft.Consultorio.Gateway/Program.cs`.
8. Add migrations and service-specific settings.
9. Update `stack-map.md` with dependencies and infra usage.

## High-Signal Repository Exceptions

- `MsAuth` Aspire service name is `ftconsultorio-auth` (not `ftconsultorio-msauth`).
- Persistence is **PostgreSQL** (`postgres` resource in AppHost, one DB per service).
- El Transactional Outbox (`OutboxMessages`) ya está cableado en el DbContext, aunque
  todavía no hay broker de mensajería en la solución (se agregará con futuros micros).
- Las notificaciones usan un adaptador de logging (`LoggingNotificationsApiClient`)
  hasta que exista un microservicio de notificaciones.

## Quick Verification Commands

Use targeted builds/tests for changed projects:

```bash
# Build one service
dotnet build Ft.Consultorio.MsAuth/Ft.Consultorio.MsAuth.csproj -v minimal

# Build the whole solution
dotnet build Ft.Consultorio.slnx -v minimal
```

Prefer project-level validation over full solution runs unless the change spans multiple services.
