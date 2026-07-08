---
name: net-vertical-slice
description: Reusable skill for building .NET web applications using Wolverine CQRS, Vertical Slice Architecture, EF Core, FluentValidation, and Mapster. Adapts to any project's domain and folder structure.
---

# .NET Wolverine Vertical Slice Architecture Skill

> A reusable, project-agnostic skill for building professional .NET web applications following Vertical Slice Architecture with Wolverine CQRS, Entity Framework Core, and modern .NET best practices.

---

## How to Use This Skill

When applying this skill to a project, **first analyze the existing codebase** to discover:

1. **Project root namespace** — check `*.csproj` for `<RootNamespace>` or infer from folder structure
2. **Existing folder structure** — scan top-level directories to match conventions
3. **Existing entity categories** — check `Data/Entities/` subfolders for domain grouping
4. **Infrastructure patterns** — look at `Infrastructure/Common/` for `Response<T>`, `Result<T>`, `Error`, `PagedList<T>`
5. **Existing features** — scan `Features/` to understand naming and slice patterns already in use

Then adapt all templates using the discovered project namespace and conventions.

---

## dotnet new Templates

Two templates are bundled with this skill:

| Template | Short Name | Type | Purpose |
|----------|-----------|------|---------|
| **Wolverine VSA Web API** | `vsa-webapi` | Project | Create a new VSA project from scratch |
| **Wolverine VSA Feature** | `vsa-feature` | Item | Scaffold a new feature in an existing project |

### Install Both Templates

```bash
dotnet new install ./templates/vsa-webapi
dotnet new install ./templates/vsa-feature
```

### Uninstall

```bash
dotnet new uninstall ./templates/vsa-webapi
dotnet new uninstall ./templates/vsa-feature
```

---

### Project Template: `vsa-webapi`

Creates a complete ASP.NET Core Web API with Wolverine CQRS, EF Core, FluentValidation, Mapster, all infrastructure boilerplate, and a sample feature.

#### Usage

```bash
dotnet new vsa-webapi -n MyApp
```

#### Parameters

| Parameter      | Default       | Description                                   |
| -------------- | ------------- | --------------------------------------------- |
| `-n`           | directory name | Project name / namespace                       |
| `-F` / `--Framework` | `net10.0` | `net10.0` or `net9.0`                    |
| `-D` / `--DbProvider` | `PostgreSQL` | `PostgreSQL`, `SqlServer`, `Sqlite`   |
| `-A` / `--ApiDocsTool` | `Scalar` | `Scalar` or `Swagger`                       |
| `-C` / `--CacheTool` | `OutputCache` | `OutputCache` or `None`                  |
| `-Au` / `--AuthProvider` | `None` | `None`, `Keycloak`, or `Identity`               |

#### Generated Structure

```
MyApp/
├── Program.cs                    # Wolverine + EF Core + Mapster + FV setup
├── MyApp.csproj                  # NuGet packages pre-configured
├── appsettings.json / .Development.json
├── Data/
│   ├── Common/BaseEntity.cs      # Audit fields + soft delete + row version
│   ├── AppDbContext.cs           # Auto soft-delete filter, audit on SaveChanges
│   ├── Entities/Sample.cs        # Sample entity
│   └── Configuration/SampleConfiguration.cs
├── Features/SampleFeature/       # Full CRUD sample (5 slices)
├── Infrastructure/Common/
│   ├── Response.cs               # API response envelope
│   ├── Result.cs                 # Domain result pattern
│   ├── Error.cs                  # Error types & factory
│   ├── PagedList.cs              # Pagination wrapper
│   └── Exceptions/               # Domain, NotFound, Forbidden
├── Extensions/
│   ├── GlobalExceptionHandler.cs # Centralized HTTP error mapping
│   ├── QueryableExtensions.cs    # Dynamic LINQ sorting
│   └── OpenApiExtensions.cs      # Swagger helpers
└── Mappings/SampleMappingConfig.cs
```

#### In Visual Studio

1. Install the template via CLI: `dotnet new install ./templates/vsa-webapi`
2. Open Visual Studio → **Create a new project**
3. Search for **"Wolverine VSA"**
4. Select **Wolverine VSA Web API** → configure name and location → Create

#### After Generation

1. Update `appsettings.json` with your database connection string
2. Run `dotnet restore`
3. Run `dotnet ef migrations add InitialCreate`
4. Run `dotnet ef database update`
5. Run `dotnet run` and open Swagger at `http://localhost:5000/swagger`

---

### Feature Template: `vsa-feature`

Scaffolds a complete CRUD feature (Entity + 5 slices + Mapping) inside an existing VSA project.

#### Usage

```bash
dotnet new vsa-feature -n Product \
  --ResourceName products \
  --Category Catalog \
  --RootNamespace MyApp \
  --DbContext AppDbContext \
  --output .
```

#### Parameters

| Parameter          | Required | Default         | Description                                              |
| ------------------ | -------- | --------------- | -------------------------------------------------------- |
| `-n` / `--name`    | Yes      | —               | Entity name in PascalCase (e.g., `Product`, `Order`)     |
| `--ResourceName`   | Yes      | —               | API resource in kebab-case (e.g., `products`, `orders`)  |
| `--Category`       | No       | `Core`          | Entity category for namespaces (e.g., `Catalog`)         |
| `--RootNamespace`  | No       | `MyApp`         | Project root namespace                                   |
| `--DbContext`      | No       | `AppDbContext`  | DbContext class name                                     |

#### Generated Files (22 files)

```
Data/Entities/{Entity}.cs
Data/Configuration/{Entity}Configuration.cs
Features/{Entity}Feature/
├── Create{Entity}/       (Command, Handler, Endpoint, Validator)
├── GetById{Entity}/      (Query, Handler, Endpoint, ViewModel)
├── GetList{Entity}/      (Query, Handler, Endpoint, ViewModel)
├── Update{Entity}/       (Command, Handler, Endpoint, Validator)
└── SoftDelete{Entity}/   (Command, Handler, Endpoint)
Mappings/{Entity}MappingConfig.cs
```

#### After Generation

1. Move `Data/Entities/{Entity}.cs` to the appropriate category folder (e.g., `Data/Entities/{Category}/{Entity}.cs`)
2. Add `public DbSet<{Entity}> {Entities} { get; set; }` to your `AppDbContext`
3. Adjust the DbSet property name if the plural is irregular (e.g., `Categories` not `Categorys`)
4. Run `dotnet ef migrations add Add{Entity}`
5. Run `dotnet ef database update`

---

## Table of Contents

1. [Architecture Overview](#architecture-overview)
2. [Project Structure](#project-structure)
3. [CQRS Feature Patterns](#cqrs-feature-patterns)
4. [Infrastructure Patterns](#infrastructure-patterns)
5. [Data Layer Patterns](#data-layer-patterns)
6. [Mapping Patterns](#mapping-patterns)
7. [Validation Patterns](#validation-patterns)
8. [Error Handling](#error-handling)
9. [Service Layer Patterns](#service-layer-patterns)
10. [Common Development Tasks](#common-development-tasks)
11. [Naming Conventions](#naming-conventions)
12. [Code Style Rules](#code-style-rules)
13. [Best Practices Checklist](#best-practices-checklist)

---

## Architecture Overview

This architecture follows **Vertical Slice Architecture (VSA)** where code is organized by **features/use-cases** rather than technical layers. Each feature is a self-contained vertical slice containing its own Command/Query, Handler, Endpoint, Validator, and ViewModel.

### Core Technology Stack

| Technology                 | Purpose                       |
| -------------------------- | ----------------------------- |
| **.NET 10 / ASP.NET Core** | Runtime & Web Framework       |
| **Wolverine**              | CQRS & Mediator (Message Bus) |
| **Wolverine.Http**         | HTTP Endpoint Routing         |
| **Entity Framework Core**  | ORM (any supported database)  |
| **FluentValidation**       | Input Validation              |
| **Mapster**                | Object Mapping                |

> **Note:** Additional technologies (Serilog, JWT Bearer, SignalR, Minio, etc.) may be present in specific projects. Adapt the stack section to match the target project's actual dependencies.

### Architecture Diagram

```
┌─────────────────────────────────────────────────────┐
│                   HTTP Request                       │
├─────────────────────────────────────────────────────┤
│  Endpoint (Wolverine.Http)                          │
│    → Receives HTTP, dispatches Command/Query        │
├─────────────────────────────────────────────────────┤
│  FluentValidation Middleware (auto via Wolverine)   │
│    → Validates Command/Query before Handler         │
├─────────────────────────────────────────────────────┤
│  Handler                                            │
│    → Business logic, uses DbContext + Mapper        │
├─────────────────────────────────────────────────────┤
│  Data Layer (EF Core)                               │
│    → Entities, Configurations, Migrations           │
├─────────────────────────────────────────────────────┤
│  Response<T> / Result<T>                            │
│    → Standardized API response envelope             │
└─────────────────────────────────────────────────────┘
```

---

## Project Structure

Below is the canonical folder structure. **Adapt subfolder names under `Data/Entities/` and `Features/`** to match the project's domain.

```
{ProjectRoot}/
├── Contract/                     # Interfaces (Abstractions)
│   └── {Domain}/                 # Domain-specific contracts
│
├── Data/                         # Data Layer
│   ├── Common/
│   │   └── BaseEntity.cs         # Base entity with audit fields
│   ├── Entities/                 # Domain Entities (group by domain area)
│   │   └── {Category}/           # Category subfolders (e.g., Core/, Structure/)
│   ├── Configuration/            # EF Core IEntityTypeConfiguration<T>
│   ├── Enums/                    # Domain enumerations
│   ├── Seeding/                  # Data seeding classes
│   ├── Migrations/               # EF Core migrations
│   └── AppDbContext.cs           # Database context
│
├── Dtos/                         # Shared DTOs (cross-feature)
│
├── Extensions/                   # Extension methods & middleware
│   ├── GlobalExceptionHandler.cs # Centralized error handling
│   ├── QueryableExtensions.cs    # IQueryable sorting helpers
│   └── ...                       # Other extension classes
│
├── Features/                     # Feature Slices (Core of VSA)
│   ├── {FeatureName}Feature/     # Feature module
│   │   ├── Create{Entity}/       # Create operation slice
│   │   ├── GetById{Entity}/      # Get by ID operation slice
│   │   ├── GetList{Entity}/      # List operation slice
│   │   ├── Update{Entity}/       # Update operation slice
│   │   ├── SoftDelete{Entity}/   # Soft delete operation slice
│   │   └── {CustomAction}/       # Custom domain action
│   └── {AuthFeature}/            # Authentication/Security features
│
├── Infrastructure/               # Cross-cutting infrastructure
│   ├── Common/
│   │   ├── Response.cs           # API response envelope
│   │   ├── Result.cs             # Domain result pattern
│   │   ├── Error.cs              # Error types & factory
│   │   ├── ValidationError.cs    # Validation error aggregation
│   │   ├── PagedList.cs          # Pagination wrapper
│   │   └── Exceptions/
│   │       ├── DomainException.cs
│   │       ├── NotFoundException.cs
│   │       └── ForbiddenException.cs
│   └── Middlewares/              # Custom middleware
│
├── Mappings/                     # Mapster mapping configs
│   └── {Entity}MappingConfig.cs  # Implements IRegister
│
├── Models/                       # Configuration models (appsettings)
│
├── Services/                     # Service implementations
│   └── {Domain}/                 # Domain-specific services
│
├── Program.cs                    # Application entry point
├── appsettings.json              # Configuration
└── appsettings.Development.json  # Dev configuration
```

---

## CQRS Feature Patterns

### Pattern 1: Create (Command → Handler → Endpoint → Validator)

Every **write operation** follows this exact 4-file pattern:

#### 1.1 Command (Record)

```csharp
namespace {RootNamespace}.Features.{Feature}.Create{Entity};

public record Create{Entity}Command(
    string Property1,
    string? Property2,
    int Property3
);
```

**Rules:**

- Always use `record` type (immutable by design)
- Properties use PascalCase
- Nullable types for optional fields
- No default values — let the Validator enforce requirements
- Namespace matches the folder path exactly

#### 1.2 Handler (Sealed Class with Primary Constructor)

```csharp
using {RootNamespace}.Data;
using {RootNamespace}.Data.Entities.{Category};
using {RootNamespace}.Infrastructure.Common;
using MapsterMapper;

namespace {RootNamespace}.Features.{Feature}.Create{Entity};

public sealed class Create{Entity}Handler(AppDbContext _db, IMapper _mapper)
{
    public async Task<Response<Guid>> Handle(
        Create{Entity}Command request,
        CancellationToken cancellationToken)
    {
        var entity = _mapper.Map<{Entity}>(request);

        _db.{DbSetName}.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return Response<Guid>.Success(entity.Id);
    }
}
```

**Rules:**

- `sealed class` — no inheritance needed
- **Primary constructor** for DI (`AppDbContext _db, IMapper _mapper`)
- DI parameters use `_` prefix (e.g., `_db`, `_mapper`)
- Method signature: `Task<Response<T>> Handle({Command} request, CancellationToken cancellationToken)`
- Always return `Response<T>` wrapper
- Use `_mapper.Map<T>()` for object mapping
- Always pass `cancellationToken` to async operations

#### 1.3 Endpoint (Static Class with Wolverine HTTP)

```csharp
using {RootNamespace}.Infrastructure.Common;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using Wolverine.Http;

namespace {RootNamespace}.Features.{Feature}.Create{Entity};

public static class Create{Entity}Endpoint
{
    [WolverinePost("/api/{resource-name}")]
    [Tags("{TagGroup}")]
    [EndpointSummary("Create New {Entity}")]
    [EndpointDescription("Create a new {entity description} in the system")]
    [ProducesResponseType(typeof(Response<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Response<Guid>), StatusCodes.Status400BadRequest)]
    public static async Task<IResult> Create(
        Create{Entity}Command command,
        IMessageBus bus,
        CancellationToken cancellationToken)
    {
        var result = await bus.InvokeAsync<Response<Guid>>(command, cancellationToken);

        return result.Succeeded
            ? Results.Created($"/api/{resource-name}/{result.Data}", result)
            : Results.BadRequest(result);
    }
}
```

**Rules:**

- `static class` — stateless endpoint
- Use Wolverine HTTP attributes: `[WolverinePost]`, `[WolverineGet]`, `[WolverinePut]`, `[WolverineDelete]`
- Always include `[Tags]`, `[EndpointSummary]`, `[EndpointDescription]`
- Always include `[ProducesResponseType]` for OpenAPI documentation
- Use `IMessageBus` to dispatch commands (never call handlers directly)
- URL pattern: `/api/{kebab-case-resource-name}`
- Return `Results.Created()` for POST, `Results.Ok()` for GET/PUT, `Results.NoContent()` for DELETE

#### 1.4 Validator (FluentValidation)

```csharp
using {RootNamespace}.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace {RootNamespace}.Features.{Feature}.Create{Entity};

public class Create{Entity}Validator : AbstractValidator<Create{Entity}Command>
{
    private readonly AppDbContext _context;

    public Create{Entity}Validator(AppDbContext context)
    {
        _context = context;

        RuleFor(x => x.Property1)
            .NotEmpty().WithMessage("{Property1} is required")
            .MaximumLength(100).WithMessage("{Property1} must be less than 100 characters")
            .MustAsync(IsUniqueName).WithMessage("{Property1} already exists");

        RuleFor(x => x.Property2)
            .MaximumLength(500).WithMessage("{Property2} must be less than 500 characters");
    }

    private async Task<bool> IsUniqueName(string name, CancellationToken cancellationToken)
    {
        return !await _context.{DbSetName}.AnyAsync(
            x => x.Name == name, cancellationToken);
    }
}
```

**Rules:**

- Inherits `AbstractValidator<TCommand>`
- Inject `AppDbContext` for database validation (uniqueness checks)
- Standard constructor (not primary constructor) — FluentValidation requires this
- Async validation methods for DB queries: `MustAsync()`
- Always include `WithMessage()` for user-friendly messages
- Wolverine auto-runs validators before handlers (no manual invocation needed)

---

### Pattern 2: List/Query (Query → Handler → Endpoint → ViewModel)

#### 2.1 Query (Record with Defaults)

```csharp
namespace {RootNamespace}.Features.{Feature}.GetList{Entity};

public record GetList{Entity}Query(
    int? Page = 1,
    int? PageSize = 10,
    string? SearchTerm = null,
    string? SortBy = "{DefaultSortProperty}",
    string? OrderBy = "asc",
    // Feature-specific filters (add as needed):
    Guid? ParentId = null,
    bool? IsActive = null
);
```

**Rules:**

- Use nullable types with defaults for pagination params
- Standard pagination: `Page`, `PageSize`, `SearchTerm`, `SortBy`, `OrderBy`
- Feature-specific filters as additional nullable parameters

#### 2.2 List Handler

```csharp
using {RootNamespace}.Data;
using {RootNamespace}.Extensions;
using {RootNamespace}.Infrastructure.Common;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace {RootNamespace}.Features.{Feature}.GetList{Entity};

public sealed class GetList{Entity}Handler(AppDbContext _db, IMapper _mapper)
{
    public async Task<Response<PagedList<GetList{Entity}ViewModel>>> Handle(
        GetList{Entity}Query request,
        CancellationToken cancellationToken)
    {
        var query = _db.{DbSetName}.AsNoTracking();
        // Note: add .AsSplitQuery() ONLY when .Include()-ing multiple collections

        // Search filter
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            query = query.Where(e =>
                e.Name.ToLower().Contains(term) ||
                (e.Description != null && e.Description.ToLower().Contains(term)));
        }

        // Feature-specific filters (add as needed)
        if (request.ParentId.HasValue)
        {
            query = query.Where(e => e.ParentId == request.ParentId.Value);
        }

        // Apply sorting (uses QueryableExtensions.ApplySort)
        query = query.ApplySort(request.SortBy, request.OrderBy,
            defaultSortBy: "{DefaultSortProperty}");

        // Project and paginate (soft-delete is filtered globally by AppDbContext)
        var projectedQuery = query
            .ProjectToType<GetList{Entity}ViewModel>(_mapper.Config);

        var pagedList = await PagedList<GetList{Entity}ViewModel>.CreateAsync(
            projectedQuery,
            request.Page ?? 1,
            request.PageSize ?? 10,
            cancellationToken);

        return Response<PagedList<GetList{Entity}ViewModel>>.Success(pagedList);
    }
}
```

**Rules:**

- Always use `.AsNoTracking()` for read queries
- Use `.AsSplitQuery()` only when including multiple related collections
- Search uses `.ToLower().Contains()` for case-insensitive matching
- Use `ApplySort()` extension for dynamic sorting
- Soft-deleted records are excluded automatically by `AppDbContext`'s global query filter (no manual filtering needed in handlers)
- Use `ProjectToType<TViewModel>()` for efficient SQL projection
- Always use `PagedList<T>.CreateAsync()` for pagination

#### 2.3 List Endpoint

```csharp
using {RootNamespace}.Infrastructure.Common;
using Wolverine;
using Wolverine.Http;

namespace {RootNamespace}.Features.{Feature}.GetList{Entity};

public static class GetList{Entity}Endpoint
{
    [WolverineGet("/api/{resource-name}")]
    [Tags("{TagGroup}")]
    [EndpointSummary("List {Entities}")]
    [EndpointDescription("Get a paginated list of {entities} with filtering and sorting")]
    [ProducesResponseType(typeof(Response<PagedList<GetList{Entity}ViewModel>>), StatusCodes.Status200OK)]
    public static async Task<IResult> List(
        IMessageBus bus,
        CancellationToken cancellationToken,
        [FromQuery] int? page = 1,
        [FromQuery] int? pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortBy = "{DefaultSortProperty}",
        [FromQuery] string? orderBy = "asc",
        [FromQuery] bool? isActive = null)
    {
        var query = new GetList{Entity}Query(page, pageSize, searchTerm, sortBy, orderBy, isActive);
        var result = await bus.InvokeAsync<Response<PagedList<GetList{Entity}ViewModel>>>(
            query, cancellationToken);

        return Results.Ok(result);
    }
}
```

**Rules:**

- Use individual query parameters with `[FromQuery]` attribute and default values, then instantiate the Query record inside the method
- Return `Results.Ok()` for list endpoints

#### 2.4 ViewModel (Record)

```csharp
namespace {RootNamespace}.Features.{Feature}.GetList{Entity};

public record GetList{Entity}ViewModel(
    Guid Id,
    string Name,
    string? Description,
    // Computed/aggregated fields:
    int RelatedCount,
    DateTime CreatedAt
);
```

**Rules:**

- Always use `record` type
- Include only the fields needed for the UI/consumer
- Aggregated/computed fields (e.g., counts) are calculated via Mapster config

---

### Pattern 3: GetById

```csharp
// Query
public record GetById{Entity}Query(Guid Id);

// Handler — includes related data via .Include()
public sealed class GetById{Entity}Handler(AppDbContext _db, IMapper _mapper)
{
    public async Task<Response<GetById{Entity}ViewModel>> Handle(
        GetById{Entity}Query request,
        CancellationToken cancellationToken)
    {
        var entity = await _db.{DbSetName}
            .AsNoTracking()
            .AsSplitQuery()
            .Include(e => e.RelatedCollection)
            .Where(e => e.Id == request.Id)
            .ProjectToType<GetById{Entity}ViewModel>(_mapper.Config)
            .FirstOrDefaultAsync(cancellationToken);

        return entity is null
            ? Response<GetById{Entity}ViewModel>.Failure(
                Error.NotFound("{Entity}.NotFound", "{Entity} not found"))
            : Response<GetById{Entity}ViewModel>.Success(entity);
    }
}

// Endpoint
[WolverineGet("/api/{resource-name}/{id:guid}")]
public static async Task<IResult> GetById(
    Guid id,
    IMessageBus bus,
    CancellationToken cancellationToken)
{
    var result = await bus.InvokeAsync<Response<GetById{Entity}ViewModel>>(
        new GetById{Entity}Query(id), cancellationToken);

    return result.Succeeded
        ? Results.Ok(result)
        : Results.NotFound(result);
}
```

---

### Pattern 4: Update

```csharp
// Command — includes Id for entity identification
public record Update{Entity}Command(
    Guid Id,
    string Name,
    string? Description
);

// Handler
public sealed class Update{Entity}Handler(AppDbContext _db, IMapper _mapper)
{
    public async Task<Response<Guid>> Handle(
        Update{Entity}Command request,
        CancellationToken cancellationToken)
    {
        var entity = await _db.{DbSetName}
            .FirstOrDefaultAsync(e => e.Id == request.Id && !e.IsDeleted, cancellationToken);

        if (entity is null)
            return Response<Guid>.Failure(
                Error.NotFound("{Entity}.NotFound", "{Entity} not found"));

        _mapper.Map(request, entity);
        entity.LastUpdateAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        return Response<Guid>.Success(entity.Id);
    }
}

// Endpoint
[WolverinePut("/api/{resource-name}/{id:guid}")]
```

---

### Pattern 5: Soft Delete

```csharp
// Command
public record SoftDelete{Entity}Command(Guid Id);

// Handler
public sealed class SoftDelete{Entity}Handler(AppDbContext _db)
{
    public async Task<Response<Guid>> Handle(
        SoftDelete{Entity}Command request,
        CancellationToken cancellationToken)
    {
        var entity = await _db.{DbSetName}
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

        if (entity is null)
            return Response<Guid>.Failure(
                Error.NotFound("{Entity}.NotFound", "{Entity} not found"));

        // SaveChangesAsync intercepts EntityState.Deleted and converts it to a soft-delete
        _db.{DbSetName}.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return Response<Guid>.Success(entity.Id);
    }
}

// Endpoint
[WolverineDelete("/api/{resource-name}/{id:guid}")]
```

---

## Infrastructure Patterns

### Response Envelope (`Response<T>`)

The standard API response wrapper:

```csharp
public class Response<T>
{
    public bool Succeeded { get; set; }
    public T? Data { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public List<ValidationErrorDetail> Errors { get; set; } = new();

    // Factory methods:
    public static Response<T> Success(T data, string message = "Operation succeeded");
    public static Response<T> Success(string message = "Operation succeeded");
    public static Response<T> Failure(Error error);
}
```

**Usage:**

- `Response<Guid>.Success(entity.Id)` — Create/Update/Delete success
- `Response<T>.Success(data)` — Query success
- `Response<T>.Failure(Error.NotFound(...))` — Not found
- `Response<T>.Failure(error)` — Any error

### Result Pattern (`Result<T>`)

For internal domain logic (non-API):

```csharp
Result.Success()
Result.Success<TValue>(value)
Result.Failure(error)
Result<TValue>.Failure<TValue>(error)
Result<TValue>.ValidationFailure(error)
```

### Error Types

```csharp
public enum ErrorType { Failure, Validation, Problem, NotFound, Conflict }

// Factory methods:
Error.Failure(code, description)
Error.NotFound(code, description)
Error.Problem(code, description)
Error.Conflict(code, description)
Error.Validation(code, description)
Error.Forbidden(code, description)
```

**Error code convention:** `"{Domain}.{Action}"` — e.g., `"User.NotFound"`, `"Role.AlreadyExists"`

### PagedList

```csharp
var pagedList = await PagedList<T>.CreateAsync(query, pageNumber, pageSize, cancellationToken);
// Returns: Items, PageNumber, TotalPages, TotalCount, PageSize, HasPreviousPage, HasNextPage
```

### Domain Exceptions

Use these to throw from domain logic (caught by `GlobalExceptionHandler`):

```csharp
throw new NotFoundException($"{Entity} with ID {id} was not found");
throw new ForbiddenException("You do not have permission to perform this action");
throw new DomainException("Business rule violated", ErrorType.Conflict, "Domain.Conflict");
```

---

## Data Layer Patterns

### BaseEntity (Audit + Soft Delete)

All entities inherit from `BaseEntity`:

```csharp
public class BaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastUpdateAt { get; set; }
    public Guid? LastUpdateBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
    [Timestamp]
    public byte[]? RowVersion { get; set; }  // Optimistic concurrency
}
```

### Entity Definition

```csharp
using {RootNamespace}.Data.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace {RootNamespace}.Data.Entities.{Category};

[Table("{TableName}")]
public class {Entity} : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    // Navigation properties
    public virtual ICollection<RelatedEntity> RelatedEntities { get; set; } = new List<RelatedEntity>();
}
```

> **Tip:** Group entities under `Data/Entities/` in subfolders that reflect the project's domain. Use category names that make sense for the business (e.g., `Core/`, `Catalog/`, `Identity/`, `Workflow/`).

### EF Core Configuration

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using {RootNamespace}.Data.Entities.{Category};

namespace {RootNamespace}.Data.Configuration;

public class {Entity}Configuration : IEntityTypeConfiguration<{Entity}>
{
    public void Configure(EntityTypeBuilder<{Entity}> builder)
    {
        builder.ToTable("{TableName}");
        builder.HasKey(e => e.Id);

        builder.HasIndex(e => e.Name).IsUnique();

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Description)
            .HasMaxLength(500);

        // Relationships
        builder.HasMany(e => e.RelatedEntities)
            .WithOne(r => r.{Entity})
            .HasForeignKey(r => r.{Entity}Id)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

**Rules:**

- Configurations are auto-discovered via `modelBuilder.ApplyConfigurationsFromAssembly()`
- Always set `ToTable()` with explicit table name
- Add `HasIndex().IsUnique()` for unique business keys
- Set `HasMaxLength()` matching entity data annotations

### DbContext Registration

Add new entities to `AppDbContext`:

```csharp
public DbSet<NewEntity> NewEntities { get; set; }
```

---

## Mapping Patterns

### Mapster Configuration (IRegister)

```csharp
using {RootNamespace}.Data.Entities.{Category};
using {RootNamespace}.Features.{Feature}.Create{Entity};
using {RootNamespace}.Features.{Feature}.Update{Entity};
using {RootNamespace}.Features.{Feature}.GetById{Entity};
using {RootNamespace}.Features.{Feature}.GetList{Entity};
using Mapster;

namespace {RootNamespace}.Mappings;

public class {Entity}MappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // Command → Entity (Create)
        config.NewConfig<Create{Entity}Command, {Entity}>();

        // Command → Entity (Update) — ignore immutable fields
        config.NewConfig<Update{Entity}Command, {Entity}>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.CreatedAt);

        // Entity → ViewModel (GetById) — with related data
        config.NewConfig<{Entity}, GetById{Entity}ViewModel>()
            .Map(dest => dest.RelatedItems,
                 src => src.RelatedCollection.Select(r => r.RelatedEntity));

        // Entity → ViewModel (List) — with computed fields
        config.NewConfig<{Entity}, GetList{Entity}ViewModel>()
            .Map(dest => dest.RelatedCount,
                 src => src.RelatedCollection.Count);
    }
}
```

**Rules:**

- One mapping config class per entity/feature
- Implements `IRegister` (auto-discovered by Mapster)
- Use `.Ignore()` for fields that should NOT be overwritten in updates (like `Id`, `CreatedAt`)
- Map computed properties (counts, aggregations) in the config
- Use `.Map()` for custom projections

---

## Validation Patterns

### Standard Validators

```csharp
public class Create{Entity}Validator : AbstractValidator<Create{Entity}Command>
{
    private readonly AppDbContext _context;

    public Create{Entity}Validator(AppDbContext context)
    {
        _context = context;

        // Required field
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name must be less than 100 characters");

        // Async uniqueness check
        RuleFor(x => x.Name)
            .MustAsync(IsUniqueName).WithMessage("Name already exists");

        // Optional field with max length
        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must be less than 500 characters");

        // Enum validation
        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid status value");

        // Guid validation
        RuleFor(x => x.ParentId)
            .NotEmpty().WithMessage("Parent is required")
            .MustAsync(ParentExists).WithMessage("Parent not found");

        // Email validation
        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Invalid email format")
            .When(x => !string.IsNullOrEmpty(x.Email));
    }

    private async Task<bool> IsUniqueName(string name, CancellationToken ct)
        => !await _context.{DbSetName}.AnyAsync(x => x.Name == name, ct);

    private async Task<bool> ParentExists(Guid id, CancellationToken ct)
        => await _context.{ParentDbSet}.AnyAsync(x => x.Id == id && !x.IsDeleted, ct);
}
```

**Rules:**

- Wolverine **automatically** runs validators before handler execution
- No need to manually call `Validate()` — Wolverine + FluentValidation middleware handles it
- `ValidationException` is caught by `GlobalExceptionHandler` and returns `400 Bad Request`

---

## Error Handling

### GlobalExceptionHandler

The centralized exception handler maps exception types to HTTP status codes:

| Exception                     | HTTP Status                        | Error Response                    |
| ----------------------------- | ---------------------------------- | --------------------------------- |
| `ValidationException`         | `400 Bad Request`                  | Validation error details          |
| `DomainException`             | `400/404/409` (based on ErrorType) | Domain error message              |
| `NotFoundException`           | `404 Not Found`                    | Resource not found                |
| `ForbiddenException`          | `403 Forbidden`                    | Access denied                     |
| `UnauthorizedAccessException` | `401 Unauthorized`                 | Not authenticated                 |
| `Exception` (unhandled)       | `500 Internal Server Error`        | Generic error (no details leaked) |

**Key behavior:**

- All responses use `Response<object>.Failure(error)` for consistency
- Adds `X-Correlation-ID` header to every error response (reuses request's value if provided, otherwise generates a new GUID)
- `ValidationException` errors are logged at `Information` level (not errors)
- Unhandled exceptions are logged at `Error` level with full stack trace

---

## Service Layer Patterns

### Contract/Implementation Pattern

```csharp
// Contract (Interface) — in Contract/{Domain}/
public interface I{Service}Service
{
    Task<TResult> MethodAsync(TRequest request, CancellationToken ct = default);
}

// Implementation — in Services/{Domain}/
public class {Service}Service : I{Service}Service
{
    // implementation
}

// Registration in Program.cs
builder.Services.AddScoped<I{Service}Service, {Service}Service>();
```

---

## Common Development Tasks

### Running the Application

```bash
dotnet restore
dotnet run
dotnet run --launch-profile Development
```

### Database Migrations

```bash
# Add new migration
dotnet ef migrations add <MigrationName> --project {ProjectFile}.csproj --startup-project {ProjectFile}.csproj

# Update database
dotnet ef database update --project {ProjectFile}.csproj --startup-project {ProjectFile}.csproj

# Remove last migration
dotnet ef migrations remove --project {ProjectFile}.csproj --startup-project {ProjectFile}.csproj
```

### Adding a New Feature (Full Workflow)

1. **Create Entity** → `Data/Entities/{Category}/{Entity}.cs` (inherits `BaseEntity`)
2. **Create Configuration** → `Data/Configuration/{Entity}Configuration.cs`
3. **Register DbSet** → Add `public DbSet<{Entity}> {Entities} { get; set; }` to `AppDbContext.cs`
4. **Create Migration** → `dotnet ef migrations add Add{Entity}`
5. **Create Feature Folder** → `Features/{Entity}Feature/`
6. **Create Slices** → Create, GetById, GetList, Update, SoftDelete subfolders
7. **Create Mapping** → `Mappings/{Entity}MappingConfig.cs`
8. **Test** → Run application and test via Swagger/OpenAPI

### Program.cs Configuration Order

```csharp
// 1. Logging (e.g., Serilog)
// 2. Configuration binding (appsettings)
// 3. Authentication (e.g., JWT)
// 4. External services (project-specific)
// 5. Database (EF Core DbContextPool)
// 6. Wolverine configuration (Host.UseWolverine)
// 7. Framework services (Controllers, OpenApi, etc.)
// 8. Custom services (Scoped/Singleton/Transient)
// 9. Mapster configuration
// 10. CORS
// 11. Build & Middleware pipeline
// 12. Run
```

---

## Naming Conventions

### Files & Classes

| Item           | Convention                  | Example                     |
| -------------- | --------------------------- | --------------------------- |
| Feature folder | `{Entity}Feature`           | `AppRoleFeature`            |
| Slice folder   | `{Action}{Entity}`          | `CreateRole`, `GetListRole` |
| Command        | `{Action}{Entity}Command`   | `CreateRoleCommand`         |
| Query          | `{Action}{Entity}Query`     | `GetListRoleQuery`          |
| Handler        | `{Action}{Entity}Handler`   | `CreateRoleHandler`         |
| Endpoint       | `{Action}{Entity}Endpoint`  | `CreateRoleEndpoint`        |
| Validator      | `{Action}{Entity}Validator` | `CreateRoleValidator`       |
| ViewModel      | `{Action}{Entity}ViewModel` | `GetListRoleViewModel`      |
| Entity         | `{Entity}`                  | `AppRole`                   |
| Configuration  | `{Entity}Configuration`     | `AppRoleConfiguration`      |
| Mapping        | `{Entity}MappingConfig`     | `AppRoleMappingConfig`      |

### Namespace Convention

```
{RootNamespace}.Features.{Feature}.{Slice}
{RootNamespace}.Data.Entities.{Category}
{RootNamespace}.Data.Configuration
{RootNamespace}.Infrastructure.Common
{RootNamespace}.Extensions
{RootNamespace}.Mappings
{RootNamespace}.Contract.{Domain}
{RootNamespace}.Services.{Domain}
```

### API URL Convention

| Action  | HTTP Method | URL Pattern                               |
| ------- | ----------- | ----------------------------------------- |
| Create  | `POST`      | `/api/{resource-name}`                    |
| List    | `GET`       | `/api/{resource-name}`                    |
| GetById | `GET`       | `/api/{resource-name}/{id:guid}`          |
| Update  | `PUT`       | `/api/{resource-name}/{id:guid}`          |
| Delete  | `DELETE`    | `/api/{resource-name}/{id:guid}`          |
| Custom  | Varies      | `/api/{resource-name}/{id:guid}/{action}` |

URL uses **kebab-case** (e.g., `/api/app-roles`, `/api/org-units`)

---

## Code Style Rules

1. **Primary constructors** for Handlers with DI: `public sealed class Handler(AppDbContext _db, IMapper _mapper)`
2. **Standard constructors** for Validators (FluentValidation requirement)
3. **File-scoped namespaces**: `namespace X.Y.Z;` (not block syntax)
4. **Records** for Commands, Queries, ViewModels, and DTOs
5. **Sealed classes** for Handlers
6. **Static classes** for Endpoints
7. **Always pass `CancellationToken`** to async methods
8. **Async/await** — all database and I/O operations must be async
9. **No regions** — code should be short enough to not need them
10. **One class per file** — with the exception of small related records
11. **Implicit usings** enabled via .NET SDK
12. **Nullable reference types** enabled (`<Nullable>enable</Nullable>`)

---

## Best Practices Checklist

### When Creating a New Feature:

- [ ] Entity inherits from `BaseEntity`
- [ ] Entity has `[Table]` attribute with explicit table name
- [ ] Entity has proper data annotations (`[Required]`, `[MaxLength]`)
- [ ] Navigation properties use `virtual` and initialize with `new List<T>()`
- [ ] EF Configuration uses `IEntityTypeConfiguration<T>`
- [ ] DbSet registered in `AppDbContext`
- [ ] Migration created and applied
- [ ] Feature folder follows `{Entity}Feature/{Action}{Entity}/` structure
- [ ] Each slice has 3-4 files (Command/Query, Handler, Endpoint, Validator/ViewModel)
- [ ] Handler is `sealed class` with primary constructor DI
- [ ] Handler returns `Response<T>` wrapper
- [ ] Endpoint is `static class` with Wolverine HTTP attributes
- [ ] Endpoint uses `IMessageBus` for dispatching
- [ ] Endpoint has OpenAPI annotations (`[Tags]`, `[EndpointSummary]`, `[ProducesResponseType]`)
- [ ] Validator uses `AbstractValidator<T>` with async DB checks
- [ ] Mapping config implements `IRegister`
- [ ] Update mapping ignores `Id` and `CreatedAt`
- [ ] List queries use `.AsNoTracking()` and `ProjectToType<T>()`
- [ ] Soft-deleted records rely on AppDbContext global query filter (no manual `.Where(e => !e.IsDeleted)` in handlers)
- [ ] Pagination uses `PagedList<T>.CreateAsync()`
- [ ] Error codes follow `{Domain}.{Action}` convention
- [ ] URL follows kebab-case convention

### Performance:

- [ ] Use `.AsNoTracking()` for read-only queries
- [ ] Use `.AsSplitQuery()` when including multiple collections
- [ ] Use `ProjectToType<T>()` instead of loading full entities then mapping
- [ ] Use `DbContextPool` (not plain `AddDbContext`)
- [ ] Use `MemoryCache` for frequently accessed data
- [ ] Avoid N+1 queries — use `.Include()` or projection

### Security:

- [ ] Authentication settings validated at startup (`throw if null`)
- [ ] CORS configured with specific origins (not `AllowAnyOrigin`)
- [ ] Sensitive information never in error responses (500s show generic message)
- [ ] Use `ICurrentUserService` for user context in handlers
- [ ] Check permissions via `HasPermissionAsync()` for protected operations
