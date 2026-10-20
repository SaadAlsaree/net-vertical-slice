# Net Vertical Slice Templates

Production-oriented .NET templates for building ASP.NET Core Web APIs with
Wolverine, Vertical Slice Architecture, Entity Framework Core, FluentValidation,
and Mapster.

The package provides two coordinated templates:

- `vsa-webapi`: creates a complete Web API with production infrastructure.
- `vsa-feature`: adds a full vertical slice to an existing project.

The generated application is ready to build immediately and keeps infrastructure,
authentication, observability, and feature code separated from the composition root.

## Why use this template?

| Area | Included capability |
|---|---|
| Architecture | Vertical Slice Architecture with command, query, handler, endpoint, and validator conventions |
| Messaging | Wolverine CQRS, HTTP endpoints, middleware, and generated handler code |
| Validation | FluentValidation executed before handler dispatch with ProblemDetails responses |
| Persistence | EF Core with PostgreSQL, SQL Server, or SQLite |
| Data integrity | Audit fields, soft delete, optimistic concurrency, and entity configurations |
| Authentication | Choose no authentication, Keycloak JWT, or ASP.NET Core Identity with local JWT |
| Security | Per-user and per-IP rate limiting with stricter authentication policies |
| API contracts | Standard `Response<T>`, typed `Error`, `Result<T>`, and paginated results |
| Mapping | Mapster registration and projection conventions |
| Documentation | Scalar or Swagger/OpenAPI |
| Caching | Optional ASP.NET Core output caching |
| Observability | Optional OpenTelemetry traces, metrics, and logs through OTLP |
| Logging | Structured Serilog configuration |
| Operations | Liveness and readiness health endpoints |
| Containers | Optional Dockerfile, Docker Compose with the selected database, and Aspire Dashboard |
| Production | Multi-stage Docker build with pre-generated Wolverine code |
| Developer experience | Central package management, Visual Studio options, sample feature, tests, and bundled AI guidance |

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

## Install

```bash
dotnet new install NetVerticalSlice.Templates
```

List the installed templates:

```bash
dotnet new list vsa
```

## Create a Web API

```bash
dotnet new vsa-webapi \
  -n Store.Api \
  -D PostgreSQL \
  -A Scalar \
  -C OutputCache \
  -Au Identity \
  --Observability OpenTelemetry \
  --Docker true
```

Then run:

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
| `--Framework` | `-F` | `net10.0` | `net10.0` |
| `--DbProvider` | `-D` | `PostgreSQL` | `PostgreSQL`, `SqlServer`, `Sqlite` |
| `--ApiDocsTool` | `-A` | `Scalar` | `Scalar`, `Swagger` |
| `--CacheTool` | `-C` | `OutputCache` | `OutputCache`, `None` |
| `--AuthProvider` | `-Au` | `None` | `None`, `Keycloak`, `Identity` |
| `--Observability` | — | `None` | `None`, `OpenTelemetry` |
| `--Docker` | — | `false` | `true`, `false` |

These choices are also available in the Visual Studio and Rider new-project
experience.

Every option is applied at generation time: files, packages, `DefineConstants`,
and configuration sections for options you did not select are not emitted. The
project ships a single `<Name>.slnx` solution file.

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

### Database choices

The selected database provider controls the EF Core package, compile symbol, and
generated connection strings. The application uses `DbContextPool`, configuration
classes, audit interception, and a global soft-delete filter.

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
│   ├── MessagingHostBuilderExtensions.cs
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

## Template verification

The repository includes automated tests that generate projects for every database
and authentication choice, build them, publish them with pre-generated Wolverine
code, and verify runtime validation and rate-limiting behavior.

```bash
dotnet test templates/TemplateTests/TemplateTests.csproj --configuration Release
```

Create a NuGet package:

```bash
dotnet pack templates/NetVerticalSlice.Templates.csproj --configuration Release
```

## Uninstall

```bash
dotnet new uninstall NetVerticalSlice.Templates
```

## Additional documentation

The repository contains:

- `docs/examples.md`: end-to-end usage examples.
- `docs/CONTRIBUTING.md`: contribution and verification workflow.
- `docs/IMPROVEMENTS.md`: planned improvements.
- `SKILL.md`: detailed architecture and coding conventions for AI-assisted development.

Generated projects also include the reusable skill under
`.agents/skills/net-vertical-slice/`.

## License

MIT
