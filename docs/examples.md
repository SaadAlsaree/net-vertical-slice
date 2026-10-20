# Examples & Walkthroughs

End-to-end examples for the `vsa-webapi` and `vsa-feature` templates. Every code snippet below follows the **exact patterns** the templates generate — adapted with realistic entity names. Use these as a reference for how to extend a generated feature.

---

## Table of Contents

1. [Example 1 — From zero to running API (PostgreSQL + Scalar)](#example-1--from-zero-to-running-api-postgresql--scalar)
2. [Example 2 — Scaffolding a feature and finishing it by hand](#example-2--scaffolding-a-feature-and-finishing-it-by-hand)
3. [Example 3 — The anatomy of a feature (annotated)](#example-3--the-anatomy-of-a-feature-annotated)
4. [Example 4 — Customizing the generated feature](#example-4--customizing-the-generated-feature)
5. [Example 5 — Calling the API (cURL / HTTP)](#example-5--calling-the-api-curl--http)
6. [Example 6 — Authentication with Keycloak](#example-6--authentication-with-keycloak)
7. [Example 7 — SQLite quick-prototype (no server)](#example-7--sqlite-quick-prototype-no-server)
8. [Example 8 — Docker Compose with Aspire Dashboard](#example-8--docker-compose-with-aspire-dashboard)

---

## Example 1 — From zero to running API (PostgreSQL + Scalar)

```bash
# 1. Install the templates (once)
dotnet new install NetVerticalSlice.Templates

# 2. Create the project
dotnet new vsa-webapi -n Shop.Api -D PostgreSQL -A Scalar -C OutputCache -Au None
cd Shop.Api
dotnet restore

# 3. Adjust the connection string in appsettings.Development.json if needed
#    (a working PostgreSQL string is already generated)

# 4. Create + apply the initial migration
dotnet ef migrations add InitialCreate -o Data/Migrations
dotnet ef database update

# 5. Run
dotnet run
```

Open **http://localhost:5000/scalar/v1**. You'll see a working `Samples` group with five endpoints — that's the `SampleFeature` shipped with the template.

---

## Example 2 — Scaffolding a feature and finishing it by hand

Goal: add a `Product` resource at `/api/products` to `Shop.Api`.

```bash
# From the project root
dotnet new vsa-feature -n Product \
  --ResourceName products \
  --Category Catalog \
  --RootNamespace Shop.Api \
  --DbContext AppDbContext \
  --output .
```

This generates 22 files. The generated `ProductHandler`s call `_db.Set<Product>()`, so the project **compiles immediately**. To persist data:

**1. Register the DbSet** in `Data/AppDbContext.cs`:

```csharp
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Sample> Samples => Set<Sample>();
    public DbSet<Product> Products => Set<Product>();   // ← add this

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ... existing code
    }
}
```

**2. Migrate:**

```bash
dotnet ef migrations add AddProduct -o Data/Migrations
dotnet ef database update
```

**3. Run and try it** — `POST /api/products`, then `GET /api/products`.

> **Irregular plurals:** for `Category` use `--ResourceName categories` and a `public DbSet<Category> Categories` property. The template tests cover exactly this case.

---

## Example 3 — The anatomy of a feature (annotated)

Below is what `vsa-feature` generates for `-n Product --ResourceName products --Category Catalog`, with annotations explaining each piece.

### 3.1 Entity — `Data/Entities/Catalog/Product.cs`

```csharp
using Shop.Api.Data.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shop.Api.Data.Entities.Catalog;

[Table("products")]            // ← maps to the kebab-case resource name
public class Product : BaseEntity   // ← inherits Id, audit fields, soft-delete, RowVersion
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }
}
```

> **Soft-delete is automatic.** `AppDbContext` applies a global query filter so soft-deleted rows are never returned, and `AuditSaveChangesInterceptor` converts `Remove()` into a soft-delete on `SaveChanges`.

### 3.2 EF Configuration — `Data/Configuration/ProductConfiguration.cs`

Auto-discovered via `modelBuilder.ApplyConfigurationsFromAssembly(...)` in `AppDbContext`.

### 3.3 Create slice

**Command** (a record):

```csharp
namespace Shop.Api.Features.ProductFeature.CreateProduct;

public record CreateProductCommand(
    string Name,
    string? Description
);
```

**Handler** (sealed class, primary-constructor DI, returns `Response<T>`):

```csharp
public sealed class CreateProductHandler(AppDbContext _db, IMapper _mapper)
{
    public async Task<Response<Guid>> Handle(
        CreateProductCommand request,
        CancellationToken cancellationToken)
    {
        var entity = _mapper.Map<Product>(request);

        _db.Set<Product>().Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return Response<Guid>.Success(entity.Id);
    }
}
```

**Validator** (FluentValidation; Wolverine runs it automatically):

```csharp
public class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    private readonly AppDbContext _context;

    public CreateProductValidator(AppDbContext context)
    {
        _context = context;

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name must be less than 100 characters")
            .MustAsync(BeUniqueName).WithMessage("Name already exists");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must be less than 500 characters");
    }

    private async Task<bool> BeUniqueName(string name, CancellationToken ct)
        => !await _context.Set<Product>().AnyAsync(x => x.Name == name, ct);
}
```

**Endpoint** (static class, Wolverine HTTP attributes, dispatches via `IMessageBus`):

```csharp
public static class CreateProductEndpoint
{
    [WolverinePost("/api/products")]
    [Tags("Product")]
    [EndpointSummary("List Products")]
    [EndpointDescription("Create a new product in the system")]
    [ProducesResponseType(typeof(Response<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Response<Guid>), StatusCodes.Status400BadRequest)]
    public static async Task<IResult> Create(
        CreateProductCommand command,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        var result = await bus.InvokeAsync<Response<Guid>>(command, cancellationToken);

        return result.Succeeded
            ? Results.Created($"/api/products/{result.Data}", result)
            : Results.BadRequest(result);
    }
}
```

### 3.4 List slice (pagination + search + dynamic sort)

```csharp
public sealed class GetListProductHandler(AppDbContext _db, IMapper _mapper)
{
    public async Task<Response<PagedList<GetListProductViewModel>>> Handle(
        GetListProductQuery request,
        CancellationToken cancellationToken)
    {
        var query = _db.Set<Product>().AsNoTracking();   // read-only

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            query = query.Where(e =>
                e.Name.ToLower().Contains(term) ||
                (e.Description != null && e.Description.ToLower().Contains(term)));
        }

        // Dynamic sort via QueryableExtensions.ApplySort
        query = query.ApplySort(request.SortBy, request.OrderBy, defaultSortBy: "Name");

        // Project straight to SQL via Mapster
        var projected = query.ProjectToType<GetListProductViewModel>(_mapper.Config);

        var paged = await PagedList<GetListProductViewModel>.CreateAsync(
            projected, request.Page ?? 1, request.PageSize ?? 10, cancellationToken);

        return Response<PagedList<GetListProductViewModel>>.Success(paged);
    }
}
```

### 3.5 Mapster config — `Mappings/ProductMappingConfig.cs`

Implements `IRegister` → auto-discovered at startup.

```csharp
public class ProductMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateProductCommand, Product>();

        // Update must NOT overwrite immutable audit fields
        config.NewConfig<UpdateProductCommand, Product>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.CreatedAt);

        config.NewConfig<Product, GetByIdProductViewModel>();
        config.NewConfig<Product, GetListProductViewModel>();
    }
}
```

---

## Example 4 — Customizing the generated feature

The template is a starting point. Common customizations:

### Add a domain-specific field

1. Add the property to `Product.cs` and `ProductConfiguration.cs`.
2. Add it to `CreateProductCommand` / `UpdateProductCommand`.
3. Add it to the ViewModels you want it exposed in.
4. Add a validation rule in the validators.
5. `dotnet ef migrations add AddProductPrice -o Data/Migrations && dotnet ef database update`.

### Add a relationship (e.g., `Product.CategoryId`)

```csharp
public class Product : BaseEntity
{
    // ...
    public Guid CategoryId { get; set; }
    public virtual Category Category { get; set; } = null!;
}
```

In `GetByIdProductHandler`, eager-load with `.Include()` and add `.AsSplitQuery()` if you `Include` multiple collections:

```csharp
var entity = await _db.Set<Product>()
    .AsNoTracking()
    .AsSplitQuery()
    .Include(p => p.Category)
    .Where(p => p.Id == request.Id)
    .ProjectToType<GetByIdProductViewModel>(_mapper.Config)
    .FirstOrDefaultAsync(cancellationToken);
```

### Throw a domain error (handled by `GlobalExceptionHandler`)

```csharp
throw new NotFoundException($"Product with ID {request.Id} was not found");
// → 404 Not Found with a consistent Response<object> body + X-Correlation-ID header

throw new DomainException("Insufficient stock", ErrorType.Conflict, "Product.OutOfStock");
// → 409 Conflict
```

### Add a custom action endpoint (beyond CRUD)

Add a new slice folder, e.g. `Features/ProductFeature/ArchiveProduct/`, following the same `Command` → `Handler` → `Endpoint` shape, and route it at `/api/products/{id:guid}/archive`.

---

## Example 5 — Calling the API (cURL / HTTP)

Assume the API is running on `http://localhost:5000` and you've added a `Product` feature at `/api/products`.

```bash
# Create
curl -X POST http://localhost:5000/api/products \
  -H "Content-Type: application/json" \
  -d '{"name":"MCP-1000","description":"Starter product"}'

# List (paginated, filtered, sorted)
curl "http://localhost:5000/api/products?page=1&pageSize=10&searchTerm=mcp&sortBy=Name&orderBy=asc"

# Get by id
curl http://localhost:5000/api/products/<id>

# Update
curl -X PUT http://localhost:5000/api/products/<id> \
  -H "Content-Type: application/json" \
  -d '{"name":"MCP-1000-V2","description":"Updated"}'

# Soft-delete
curl -X DELETE http://localhost:5000/api/products/<id>
```

**Success response** (`POST`):

```json
{
  "succeeded": true,
  "data": "f3c1...guid...",
  "message": "Operation succeeded",
  "code": "",
  "errors": []
}
```

**Validation error** (`400`, from `GlobalExceptionHandler`):

```json
{
  "succeeded": false,
  "data": null,
  "message": "Validation failed",
  "code": "Validation.Error",
  "errors": [
    { "property": "Name", "message": "Name already exists" }
  ]
}
```

Every error response also carries an `X-Correlation-ID` response header.

---

## Example 6 — Authentication with Keycloak

```bash
dotnet new vsa-webapi -n Secure.Api -Au Keycloak
```

This adds `Microsoft.AspNetCore.Authentication.JwtBearer`, generates `Extensions/AuthExtensions.cs`, `Models/KeycloakSettings.cs`, and wires `app.UseAuthentication()` / `app.UseAuthorization()` into the pipeline. The OpenAPI document is also patched with a `Bearer` security scheme.

Configure Keycloak in `appsettings.json` under the `Keycloak` section (issuer, realm, audience, client IDs). Then protect an endpoint with `[Authorize]`:

```csharp
[WolverineGet("/api/products/{id:guid}")]
[Authorize]                       // ← require auth
public static async Task<IResult> GetById(...) { ... }
```

> For local dev with self-signed HTTPS certs, the generated `AuthExtensions` relaxes certificate validation only in the `Development` environment.

---

## Example 7 — SQLite quick-prototype (no server)

The fastest way to try the templates with zero infrastructure:

```bash
dotnet new vsa-webapi -n Sandbox -D Sqlite -Au None
cd Sandbox
dotnet restore

# SQLite needs the DB file created + schema applied
dotnet ef migrations add InitialCreate -o Data/Migrations
dotnet ef database update

dotnet run
```

The generated connection string for SQLite is simply `Data Source=vsatemplate.db` (or `vsatemplate_dev.db` in Development). The DB file appears in the project root after `database update`.

---

## Example 8 — Docker Compose with Aspire Dashboard

Generate the API with containers and OpenTelemetry enabled:

```bash
dotnet new vsa-webapi -n Shop.Api -D PostgreSQL -A Scalar -Au None \
  --Observability OpenTelemetry --Docker true
cd Shop.Api
```

The project now contains `Dockerfile`, `docker-compose.yml`,
`docker-compose.override.yml`, and `.env`. Compose declares exactly three
services: `api`, `postgres`, and `aspire-dashboard`. Picking `-D SqlServer` swaps
`postgres` for `sqlserver`; `-D Sqlite` drops the database service and mounts a
volume for the database file instead; leaving `--Observability None` drops
`aspire-dashboard`.

```bash
# Every setting (passwords, connection string, JWT, OTEL) lives in .env
docker compose up -d --build
docker compose ps             # postgres reports "healthy" before api starts
```

Create the migration on the host; the API applies it to the `shop_api` database
when it starts, because the generated `.env` sets `Database__MigrateOnStartup=true`:

```bash
dotnet ef migrations add InitialCreate -o Data/Migrations
docker compose up -d --build
```

Then:

- API: `http://localhost:8080/scalar/v1` and `http://localhost:8080/health/ready`
- Aspire Dashboard: `http://localhost:18888` — traces, metrics, and structured
  logs from the API, exported over OTLP to `aspire-dashboard:18889`

Stop everything with `docker compose down` (add `-v` to delete the database volume).

---

## Where to go next

- **Full pattern reference** (naming conventions, every CQRS pattern, best-practices checklist) → [`../SKILL.md`](../SKILL.md)
- **Packaging & publishing** the templates → [`../templates/README.md`](../templates/README.md)
- **Contributing** → [`CONTRIBUTING.md`](./CONTRIBUTING.md)
