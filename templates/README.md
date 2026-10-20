# Net Vertical Slice — .NET Starter Platform

[![NuGet](https://img.shields.io/nuget/v/NetVerticalSlice.Templates.svg)](https://www.nuget.org/packages/NetVerticalSlice.Templates/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/NetVerticalSlice.Templates.svg)](https://www.nuget.org/packages/NetVerticalSlice.Templates/)
[![Template verification](https://github.com/SaadAlsaree/net-vertical-slice/actions/workflows/templates.yml/badge.svg)](https://github.com/SaadAlsaree/net-vertical-slice/actions/workflows/templates.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://github.com/SaadAlsaree/net-vertical-slice/blob/main/LICENSE)

Start your next .NET 10 API with architecture, infrastructure, and
production-minded defaults already working together.

**Net Vertical Slice is an opinionated starter platform for building and evolving
Wolverine-powered Vertical Slice APIs.** It is delivered through two coordinated
`dotnet new` generators: one creates the application foundation, and the other keeps
new business features aligned as the product grows.

Instead of spending the first stage of a project assembling messaging, validation,
persistence, authentication, observability, API contracts, and deployment plumbing,
you begin with a buildable foundation and own the generated source from day one.

> **Build the foundation once. Keep every feature aligned.**
>
> The platform is production-oriented, not production-complete. Every team must still
> review authorization, secrets, migrations, proxy configuration, domain validation,
> auditing, and operational policies for its deployment.

## One platform, two coordinated generators

| Generator | Role in the platform | Generates |
|---|---|---|
| `vsa-webapi` | Establish the foundation | A complete ASP.NET Core project with infrastructure, a sample feature, and selectable database, authentication, documentation, caching, and observability |
| `vsa-feature` | Grow the application consistently | Create, GetById, GetList, Update, and SoftDelete slices, validators, endpoints, handlers, and Mapster configuration |

Together, the generators provide a repeatable path from a new service to a growing
application without introducing a second architectural style or recreating the same
feature plumbing.

## Why start with this platform?

- **Move from setup to business code.** Start with Wolverine messaging, validation,
  persistence, API contracts, mapping, and operational plumbing already integrated.
- **Keep architecture consistent as the application grows.** Commands, queries,
  endpoints, handlers, and validators stay close to the use case they implement.
- **Own ordinary .NET source.** Generated applications remain readable and editable;
  the starter platform is not a proprietary runtime your application must depend on.
- **Choose only the infrastructure you need.** Database, authentication, API
  documentation, caching, and observability providers are isolated by template options.
- **Begin with an operational baseline.** Structured logging, health checks, rate
  limiting, ProblemDetails, Docker, and static Wolverine code generation are included.

## Platform capabilities

| Area | Included capability |
|---|---|
| Architecture | Vertical Slice conventions for commands, queries, handlers, endpoints, and validators |
| Messaging | Wolverine CQRS, HTTP endpoints, middleware, transactions, and generated handler code |
| Validation | FluentValidation before handler dispatch with ProblemDetails responses |
| Persistence | EF Core with PostgreSQL, SQL Server, or SQLite |
| Data model | Audit timestamps, soft-delete filtering, a concurrency token scaffold, and entity configurations |
| Authentication | No authentication, Keycloak JWT, or ASP.NET Core Identity with local JWT |
| Traffic protection | Global per-user/per-IP rate limiting and stricter Identity endpoint policies |
| API contracts | `Response<T>`, typed `Error`, `Result<T>`, and paginated results |
| Mapping | Mapster registration and query projection conventions |
| API documentation | Scalar or Swagger/OpenAPI |
| Caching | Optional ASP.NET Core output caching |
| Observability | Optional OpenTelemetry traces, metrics, and logs through OTLP |
| Logging | Structured Serilog configuration |
| Operations | Liveness and readiness health endpoints |
| Production packaging | Multi-stage Docker build with pre-generated Wolverine code |
| Developer experience | Central package management, Visual Studio/Rider options, sample feature, repository verification tests, and bundled AI guidance |
## Design principles

- Feature code is organized by business use case, not by technical layer.
- `Program.cs` remains a small composition root.
- Infrastructure registration is split into focused `DependencyInjection` extensions.
- Authentication providers are isolated: the generated project contains only the
  selected provider's files, packages, and configuration.
- Normal Wolverine endpoints receive global rate limiting automatically.
- Production builds use static Wolverine code instead of runtime compilation.
- Configuration values can be overridden with standard ASP.NET Core configuration
  and environment variables.

## Requirements

- .NET 10 SDK

## What's new in 2.0

Version 2.0 is the major .NET 10 and Wolverine 6 starter-platform release:

- A complete `vsa-webapi` project template and coordinated `vsa-feature` item template.
- Selectable PostgreSQL, SQL Server, or SQLite persistence.
- Isolated None, Keycloak, and Identity authentication outputs.
- Optional OpenTelemetry and output caching.
- Static Wolverine production code generation verified by the repository test harness.

Installing a newer template version affects newly generated output; it does not
rewrite existing applications. For an existing 1.x-generated project, compare the
new template output and adopt changes intentionally.

## Quick start

```bash
dotnet new install NetVerticalSlice.Templates
```

List the installed templates:

```bash
dotnet new list vsa
```

### Create a Web API

```bash
dotnet new vsa-webapi \
  -n Store.Api \
  -D Sqlite \
  -Au None
```

This fastest local path uses SQLite and no authentication. Then run:

```bash
cd Store.Api
dotnet restore
dotnet ef migrations add InitialCreate -o Data/Migrations
dotnet ef database update
dotnet run
```

For Scalar, open `/scalar/v1`. For Swagger, open `/swagger`.

## Project template options

| Option | Short name | Default | Choices |
|---|:---:|---|---|
| `--DbProvider` | `-D` | `PostgreSQL` | `PostgreSQL`, `SqlServer`, `Sqlite` |
| `--ApiDocsTool` | `-A` | `Scalar` | `Scalar`, `Swagger` |
| `--CacheTool` | `-C` | `OutputCache` | `OutputCache`, `None` |
| `--AuthProvider` | `-Au` | `None` | `None`, `Keycloak`, `Identity` |
| `--Observability` | — | `None` | `None`, `OpenTelemetry` |
| `--Docker` | — | `false` | `true`, `false` |

These choices are also available in the Visual Studio and Rider new-project
experience.

![Visual Studio project template wizard](https://raw.githubusercontent.com/SaadAlsaree/net-vertical-slice/main/image/vsa-wizard.png)

### Authentication choices

| Choice | Generated result |
|---|---|
| `None` | No authentication files, packages, claims, or provider settings |
| `Keycloak` | JWT Bearer authentication and Keycloak configuration only |
| `Identity` | ASP.NET Core Identity, JWT generation, refresh tokens, and authentication endpoints |

Identity projects include:

- `POST /api/auth/register`
- `POST /api/auth/login`
- `POST /api/auth/refresh`
- `POST /api/auth/logout`

> **Authentication is not authorization.** Selecting Identity or Keycloak configures
> token authentication, but generated business endpoints are not automatically
> protected. Add `[Authorize]`, fallback policies, roles, or claims according to
> your application requirements.

For Identity deployments, configure the JWT secret outside source control, decide
whether email confirmation is required, seed any roles your application expects
(such as `User`), and verify role-assignment failures instead of ignoring them.

### Database choices

The selected database provider controls the EF Core package, compile symbol, and
generated connection strings. The application uses `DbContextPool`, configuration
classes, audit interception, and a global soft-delete filter.

`BaseEntity.RowVersion` provides a concurrency-token scaffold. If your API requires
end-to-end optimistic concurrency, carry the token through update contracts and map
`DbUpdateConcurrencyException` to an intentional response such as HTTP 409.

### Rate limiting

Rate limiting is enabled in every generated project.

| Traffic | Default policy |
|---|---|
| Authenticated user | Token bucket: 60 tokens, replenished by 10 every 10 seconds |
| Anonymous client | Sliding window: 120 requests per minute, partitioned by IP |
| Identity login | 10 requests per 5 minutes per IP |
| Identity registration | 5 requests per 15 minutes per IP |
| Identity token refresh | 30 requests per minute per IP |

Rejected requests return HTTP `429`, a `Retry-After` header, and the standard
response envelope. Limits can be changed under `RateLimiting` in
`appsettings.json`.

### Observability

When `--Observability OpenTelemetry` is selected, the project includes:

- ASP.NET Core request traces and metrics.
- .NET runtime metrics.
- Wolverine activity and meter sources.
- ASP.NET Core rate-limiting metrics.
- Serilog log events forwarded to the OpenTelemetry logger provider, so logs are
  exported next to traces and metrics.
- OTLP export configured through standard OpenTelemetry environment variables.

Set the collector endpoint before running:

```bash
OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:4317
```

### Docker Compose

`--Docker true` generates `Dockerfile`, `.dockerignore`, `docker-compose.yml`,
`docker-compose.override.yml`, and `.env`. Compose only contains the services
you selected:

| Selection | Services |
|---|---|
| `-D PostgreSQL` | `api`, `postgres` (named volume, `pg_isready` health check) |
| `-D SqlServer` | `api`, `sqlserver` (named volume, `sqlcmd` health check) |
| `-D Sqlite` | `api` with a named volume for the database file |
| `--Observability OpenTelemetry` | adds `aspire-dashboard` |

The API waits for the database to report healthy and every service restarts with
`unless-stopped`.

`.env` is the single place for container configuration. Compose reads it to fill
the `${...}` values in the Compose files and passes every entry to the `api`
container, where `Section__Key` names override `appsettings*.json`: for example
`ConnectionStrings__DefaultConnection`, `Jwt__SecretKey`, `Keycloak__Realm`, and the
`OTEL_*` exporter settings. The Compose files contain no values or fallbacks of their
own; a missing entry stops Compose with a message naming it. The generated `.env`
holds development values only, lists only what your selections need, and is excluded
from git and from the Docker build context.

```bash
dotnet ef migrations add InitialCreate -o Data/Migrations
docker compose up -d
```

With `Database__MigrateOnStartup=true` (set in the generated `.env`), the API applies
pending EF Core migrations to the Compose database when it starts, so
`dotnet ef database update` is not needed. With `--Docker true`,
`appsettings.Development.json` also points at that database through the published
port, so EF tools and `dotnet run` on the host use the same database as the
container.

> [!IMPORTANT]
> - The database connection details live in two files: `.env` (used by the containers)
>   and `appsettings.Development.json` (used by `dotnet ef` and `dotnet run` on the host).
>   They match by default. If you change `POSTGRES_PASSWORD`/`MSSQL_SA_PASSWORD`,
>   `POSTGRES_PORT`/`MSSQL_PORT`, or the database name in `.env`, update
>   `appsettings.Development.json` too.
> - The database volume keeps the password it was first created with. After changing
>   the password in `.env`, recreate the volume. This deletes the database data:
>
>   ```bash
>   docker compose down -v
>   docker compose up -d
>   ```

`docker compose up` merges `docker-compose.override.yml`, which publishes the
database port to the host for EF Core migrations and database tools. For a
production-like run, use only the base file with a server-side `.env`:

```bash
docker compose -f docker-compose.yml up -d
```

In Visual Studio, the solution contains a separate `docker-compose` project
(`docker-compose/docker-compose.dcproj`) that groups `docker-compose.yml`, its
override, `.dockerignore`, `.env`, and the Compose `launchSettings.json`. It
lives in its own folder so it never shares `bin/` and `obj/` with the web project. Set it as
the startup project and press F5: the database and dashboard start first and the
debugger attaches to `api`, using the values in `.env`.

### Aspire Dashboard

With `--Docker true --Observability OpenTelemetry`, Compose adds the standalone
Aspire Dashboard (`mcr.microsoft.com/dotnet/aspire-dashboard`) and points the API at it:

```text
OTEL_EXPORTER_OTLP_ENDPOINT=http://aspire-dashboard:18889
OTEL_EXPORTER_OTLP_PROTOCOL=grpc
OTEL_SERVICE_NAME=<project name>
```

Open `http://localhost:18888` to browse traces, metrics, and structured logs.
In development the override file sets `DOTNET_DASHBOARD_UNSECURED_ALLOW_ANONYMOUS=true`.
The base file requires the browser token printed in the dashboard container log
and an OTLP API key (`ASPIRE_OTLP_API_KEY` in `.env`), which the API sends
through `OTEL_EXPORTER_OTLP_HEADERS`.

## Generated architecture

```text
Store.Api/
├── DependencyInjection/
│   ├── ApiServiceCollectionExtensions.cs
│   ├── ApplicationServiceCollectionExtensions.cs
│   ├── AuthenticationServiceCollectionExtensions.cs
│   ├── InfrastructureServiceCollectionExtensions.cs
│   ├── LoggingHostBuilderExtensions.cs
│   ├── ObservabilityServiceCollectionExtensions.cs
│   ├── RateLimitingServiceCollectionExtensions.cs
│   └── WebApplicationExtensions.cs
├── Data/
│   ├── AppDbContext.cs
│   ├── Common/
│   ├── Configuration/
│   └── Entities/
├── Features/
│   └── SampleFeature/
├── Infrastructure/
│   ├── Common/
│   └── RateLimiting/
├── Mappings/
├── Models/
├── Program.cs
├── Directory.Packages.props
├── appsettings.json
├── Store.Api.slnx
├── Dockerfile                      # --Docker true
├── docker-compose.yml              # --Docker true
├── docker-compose.override.yml     # --Docker true
└── .env                            # --Docker true (git-ignored)
```

The request flow is:

```text
HTTP request
  -> authentication
  -> rate limiting
  -> authorization
  -> Wolverine HTTP endpoint
  -> FluentValidation
  -> command/query handler
  -> EF Core
  -> standardized response
```

Authentication and authorization steps are omitted when `AuthProvider` is `None`.

## Add a vertical slice

Use `vsa-feature` inside an existing project:

```bash
dotnet new vsa-feature \
  -n Product \
  --ResourceName products \
  --Category Catalog \
  --RootNamespace Store.Api \
  --DbContext AppDbContext \
  --output .
```

The template generates Create, GetById, GetList, Update, and SoftDelete slices,
plus Mapster configuration. Each slice follows the same structure used by the
project template.

If the entity does not exist, include it and its EF configuration:

```bash
dotnet new vsa-feature \
  -n Product \
  --ResourceName products \
  --Category Catalog \
  --RootNamespace Store.Api \
  --DbContext AppDbContext \
  --GenerateEntity \
  --output .
```

### Feature template options

| Option | Required | Default | Purpose |
|---|:---:|---|---|
| `-n`, `--name` | Yes | — | Entity name in PascalCase |
| `--ResourceName` | Yes | — | Kebab-case API resource name |
| `--Category` | No | `Core` | Entity category and namespace |
| `--RootNamespace` | No | `MyApp` | Target project namespace |
| `--DbContext` | No | `AppDbContext` | EF Core context name |
| `--GenerateEntity` | No | `false` | Generate the entity and EF configuration |

Generated commands and view models are field-agnostic. Replace the marked
`TODO` fields with properties that match the target domain, register the `DbSet`,
and create a migration.

## Production checklist

Before deploying a generated application:

- Store database credentials, JWT secrets, and provider settings in environment
  variables or a secrets manager.
- Add authorization policies to protected business endpoints; authentication alone
  does not make them private.
- For Identity, seed required roles, decide on email-confirmation and lockout
  policies, and review refresh-token storage and retention.
- Create, review, and apply EF Core migrations through your deployment process.
- Configure trusted forwarded headers when running behind a reverse proxy or load
  balancer. IP-based rate limiting depends on the resolved remote address.
- Review CORS origins, rate limits, log retention, and health-check dependencies for
  the target environment.
- Configure an OTLP collector when OpenTelemetry is enabled.
- Complete user-aware audit fields and end-to-end concurrency handling if the domain
  requires them.
- Add application-specific unit and integration tests around authorization, data
  access, and critical workflows.

## Production build

The Dockerfile generated by `--Docker true` writes Wolverine handlers during the SDK stage and
publishes without runtime compilation, Roslyn workspaces, or EF design-time
packages.

```bash
docker build \
  --build-arg APP_ENVIRONMENT=Production \
  -t store-api:latest .
```

For a non-container production publish:

```bash
dotnet run \
  -c Release \
  -p:IncludeWolverineRuntimeCompilation=true \
  -- codegen write

dotnet publish \
  -c Release \
  -p:IncludeWolverineRuntimeCompilation=false \
  -p:IncludeEfDesignTimePackages=false
```

## Health endpoints

| Endpoint | Purpose |
|---|---|
| `/health/live` | Confirms that the process is running |
| `/health/ready` | Confirms that the application is ready to receive traffic |

Health endpoints are anonymous and excluded from rate limiting.

## Repository verification tests

The repository includes automated tests that generate projects across database and
authentication choices, build them, publish them with pre-generated Wolverine code,
and verify runtime validation and rate-limiting behavior.

These tests verify the templates themselves. A generated application does not
currently include a consumer test project; add tests that reflect your domain and
deployment requirements.

```bash
dotnet test templates/TemplateTests/TemplateTests.csproj --configuration Release
```

Create a NuGet package:

```bash
dotnet pack templates/NetVerticalSlice.Templates.csproj --configuration Release
```

## NuGet package semantics

NuGet reports no package dependencies because this is a content-only template
package. Applications generated by `vsa-webapi` do have framework and library
dependencies, pinned centrally in the generated `Directory.Packages.props`.

## Uninstall

```bash
dotnet new uninstall NetVerticalSlice.Templates
```

## Documentation and support

- [End-to-end usage examples](https://github.com/SaadAlsaree/net-vertical-slice/blob/main/docs/examples.md)
- [Contribution and verification workflow](https://github.com/SaadAlsaree/net-vertical-slice/blob/main/CONTRIBUTING.md)
- [Security policy and private reporting](https://github.com/SaadAlsaree/net-vertical-slice/blob/main/SECURITY.md)
- [Short project roadmap](https://github.com/SaadAlsaree/net-vertical-slice/blob/main/ROADMAP.md)
- [Architecture and AI-assisted coding conventions](https://github.com/SaadAlsaree/net-vertical-slice/blob/main/SKILL.md)
- [Report a bug or request a feature](https://github.com/SaadAlsaree/net-vertical-slice/issues)

Generated projects also include the reusable guidance under
`.agents/skills/net-vertical-slice/`.

## License

[MIT](https://github.com/SaadAlsaree/net-vertical-slice/blob/main/LICENSE)
