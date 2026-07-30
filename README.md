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
| Observability | Optional OpenTelemetry traces and metrics through OTLP |
| Logging | Structured Serilog configuration |
| Operations | Liveness and readiness health endpoints |
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
  --Observability OpenTelemetry
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

These choices are also available in the Visual Studio and Rider new-project
experience.

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
- OTLP export configured through standard OpenTelemetry environment variables.

Set the collector endpoint before running:

```bash
OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:4317
```

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
├── Dockerfile
├── Directory.Packages.props
└── appsettings.json
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

The generated Dockerfile writes Wolverine handlers during the SDK stage and
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
