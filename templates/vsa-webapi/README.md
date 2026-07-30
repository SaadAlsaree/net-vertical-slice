# Wolverine VSA Web API Template

This template creates a .NET 10 ASP.NET Core Web API using Wolverine 6 CQRS, Vertical Slice Architecture, EF Core, FluentValidation, and Mapster.

## 1) Create a project from the template

```bash
dotnet new vsa-webapi -n Microchip.Api -D SqlServer -A Swagger -C OutputCache -Au None
cd Microchip.Api
dotnet restore
```

Template options:

- `-D|--DbProvider`: `PostgreSQL`, `SqlServer`, `Sqlite`
- `-A|--ApiDocsTool`: `Scalar`, `Swagger`
- `-C|--CacheTool`: `OutputCache`, `None`
- `-Au|--AuthProvider`: `None`, `Identity`, `Keycloak`
- `--Observability`: `None`, `OpenTelemetry`

Authentication choices are isolated. Selecting `None` does not generate authentication
files, packages, or configuration; selecting `Identity` does not generate Keycloak
artifacts, and vice versa.

## 2) Configure connection string

Update `appsettings.json` and `appsettings.Development.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost,1433;Database=microchip_db;User Id=sa;Password=Your_password123;TrustServerCertificate=True"
}
```

## 3) Create and apply the initial migration

Dotnet CLI:

```bash
dotnet ef migrations add InitialCreate -o Data/Migrations
dotnet ef database update
```

Package Manager Console (Visual Studio):

```powershell
Add-Migration InitialCreate -OutputDir Data/Migrations
Update-Database
```

## 4) Create a Microchip feature

Install the item template (once, if not already installed):

```bash
dotnet new install <path-to-templates/vsa-feature>
```

Generate a feature:

```bash
dotnet new vsa-feature -n Microchip --ResourceName microchips --Category Core --RootNamespace Microchip.Api --DbContext AppDbContext --output .
```

Then add the DbSet in `Data/AppDbContext.cs`:

```csharp
public DbSet<Microchip> Microchips { get; set; }
```

Create and apply migration for the new feature:

```bash
dotnet ef migrations add AddMicrochip -o Data/Migrations
dotnet ef database update
```

## 5) Run the API

```bash
dotnet run
```

If you selected:

- `-A Swagger`: open `/swagger`
- `-A Scalar`: open `/scalar/v1`

Invalid FluentValidation requests are returned as HTTP 400 ProblemDetails before the
message is dispatched, so normal client validation failures are not logged as backend
errors.

## 6) Rate limiting

Rate limiting is enabled for every generated project. Anonymous traffic is partitioned
by client IP, authenticated traffic by the JWT `sub` claim, and Identity login,
registration, and refresh endpoints have stricter policies. Limits are configurable
under `RateLimiting` in `appsettings.json`; rejected requests return HTTP 429 with a
`Retry-After` header and the standard `Response<T>` envelope.

Normal Wolverine endpoints require no attributes because the global limiter applies
automatically. Use `[EnableRateLimiting]` only when an endpoint needs a named policy,
and `[DisableRateLimiting]` for operational endpoints such as health checks.

## 7) Production build

The included Dockerfile writes Wolverine's generated handlers during the SDK build,
then publishes without `WolverineFx.RuntimeCompilation`, Roslyn workspaces, or EF design
packages:

```bash
docker build --build-arg APP_ENVIRONMENT=Production -t microchip-api:latest .
docker run --rm -p 8080:8080 microchip-api:latest
```

For a non-container publish:

```bash
dotnet run -c Release -p:IncludeWolverineRuntimeCompilation=true -- codegen write
dotnet publish -c Release \
  -p:IncludeWolverineRuntimeCompilation=false \
  -p:IncludeEfDesignTimePackages=false
```

Local builds retain runtime compilation by default. The Production commands above
explicitly disable it after writing the generated Wolverine code.

## 8) Observability

Generate with `--Observability OpenTelemetry`, set `OTEL_EXPORTER_OTLP_ENDPOINT`, and
the service exports ASP.NET Core/runtime telemetry plus Wolverine's `Wolverine` activity
source and `Wolverine*` meters.

## 9) Example Microchip API calls

Create microchip:

```bash
curl -X POST "http://localhost:5000/api/microchips" \
  -H "Content-Type: application/json" \
  -d '{"name":"MCP-1000","description":"Starter microchip"}'
```

List microchips:

```bash
curl "http://localhost:5000/api/microchips?page=1&pageSize=10&sortBy=Name&orderBy=asc"
```

Get by id:

```bash
curl "http://localhost:5000/api/microchips/{id}"
```
