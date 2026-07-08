<div align="center">

# 🥷 Net Vertical Slice

### Production-ready `dotnet new` templates for ASP.NET Core Web APIs

**Wolverine CQRS · Vertical Slice Architecture · EF Core · FluentValidation · Mapster**

Ship a clean, opinionated .NET Web API in minutes — not days. This repository provides the `dotnet new` templates and an AI skill that scaffolds a complete Vertical Slice Architecture backend, with all the boilerplate handled for you.

<br />

[![GitHub stars](https://img.shields.io/github/stars/SaadAlsaree/net-vertical-slice?style=social&label=★%20Stars)](https://github.com/SaadAlsaree/net-vertical-slice/stargazers)
[![GitHub forks](https://img.shields.io/github/forks/SaadAlsaree/net-vertical-slice?style=social&label=Forks)](https://github.com/SaadAlsaree/net-vertical-slice/fork)
[![License](https://img.shields.io/github/license/SaadAlsaree/net-vertical-slice?color=blue)](https://github.com/SaadAlsaree/net-vertical-slice/blob/main/LICENSE)
[![Last Commit](https://img.shields.io/github/last-commit/SaadAlsaree/net-vertical-slice?color=brightgreen&label=last%20commit)](https://github.com/SaadAlsaree/net-vertical-slice/commits/main)
[![Issues](https://img.shields.io/github/issues/SaadAlsaree/net-vertical-slice?color=orange)](https://github.com/SaadAlsaree/net-vertical-slice/issues)
[![Pull Requests](https://img.shields.io/github/issues-pr/SaadAlsaree/net-vertical-slice?color=blueviolet)](https://github.com/SaadAlsaree/net-vertical-slice/pulls)

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Wolverine](https://img.shields.io/badge/Wolverine-5.1-512BD4)](https://wolverinefx.net/)
[![EF Core](https://img.shields.io/badge/EF%20Core-10.0-512BD4)](https://learn.microsoft.com/ef/)
[![C#](https://img.shields.io/badge/C%23-13-239120?logo=csharp&logoColor=white)](https://learn.microsoft.com/dotnet/csharp/)
[![Template Version](https://img.shields.io/badge/template%20version-1.1.0-brightgreen)](https://github.com/SaadAlsaree/net-vertical-slice/blob/main/templates/NetVerticalSlice.Templates.csproj)

<br />

[**🚀 Quick Start**](#-quick-start) ·
[**📦 Templates**](#-templates) ·
[**🏛️ Architecture**](#️-architecture) ·
[**📖 Docs**](https://github.com/SaadAlsaree/net-vertical-slice/blob/main/docs/examples.md) ·
[**🤝 Contributing**](#-contributing)

</div>

<br />

> **What is this?** This repository is **tooling**, not a runnable application. It ships three coordinated things you can use together or independently — a **project template**, a **feature item template**, and an **AI skill** with the full architecture reference.

---

## ✨ Features

### Generated Web API (`vsa-webapi`)

| | Feature | What it gives you |
|---|---------|-------------------|
| ⚡ | **Wolverine** CQRS + HTTP routing | Mediator pattern, automatic transactions, FluentValidation middleware |
| 🏛️ | **Vertical Slice Architecture** | Each feature is a self-contained slice: `Command` → `Handler` → `Endpoint` → `Validator` |
| 🗄️ | **EF Core** with provider choice | PostgreSQL · SQL Server · SQLite |
| 🗑️ | **Soft-delete by default** | Global query filter + audit interceptor that converts `Delete` → soft-delete |
| 📦 | **Standardized infrastructure** | `Response<T>` envelope, `Result<T>`/`Error`, `PagedList<T>`, `GlobalExceptionHandler` |
| 🔄 | **Mapster** mapping | Auto-discovered `IRegister` configs |
| ✅ | **FluentValidation** | Runs automatically before handlers (no manual `Validate()` calls) |
| 📝 | **Serilog** structured logging | Console + rolling file |
| 📚 | **Scalar** or **Swagger** | Modern API documentation UI |
| 🔐 | **Keycloak JWT** auth (optional) | Production-ready Bearer authentication |
| 🔐 | **ASP.NET Core Identity** (optional) | Local Identity with JWT Bearer — login/register/refresh/logout endpoints |
| ⚡ | **Output caching** (optional) | ASP.NET Core output caching middleware |
| 🔧 | **Central Package Management** | `Directory.Packages.props` for version control |

### Feature template (`vsa-feature`)

> ✂️ Generates a **complete CRUD feature in a single command** — 22 files.

```
Entity + EF Configuration + 5 operations (Create, GetById, GetList, Update, SoftDelete)
   ↳ each with Command/Query, Handler, Endpoint, Validator/ViewModel
   + Mapster config
```

---

## 🚀 Quick Start

### 1. Install the templates

```bash
# From NuGet (recommended)
dotnet new install NetVerticalSlice.Templates
```

**…or install from source** (latest, unreleased changes):

```bash
# Clone the repository
git clone https://github.com/SaadAlsaree/net-vertical-slice.git
cd net-vertical-slice

# Install both templates from the local clone
dotnet new install ./templates/vsa-webapi
dotnet new install ./templates/vsa-feature
```

### 2. Create your API

```bash
dotnet new vsa-webapi -n MyApi -D PostgreSQL -A Scalar -C OutputCache -Au None
cd MyApi
dotnet restore
```

### 3. Configure the database & migrate

```bash
dotnet ef migrations add InitialCreate -o Data/Migrations
dotnet ef database update
```

### 4. Run

```bash
dotnet run
```

Open **http://localhost:5000/scalar/v1** — you'll see a working `Samples` API (the `SampleFeature` shipped with the template). 🎉

### 5. Scaffold a real feature

```bash
dotnet new vsa-feature -n Product \
  --ResourceName products \
  --Category Catalog \
  --RootNamespace MyApi \
  --DbContext AppDbContext \
  --output .
```

Register the `DbSet`, migrate, and you have a full `/api/products` CRUD endpoint. See [**📖 examples.md**](https://github.com/SaadAlsaree/net-vertical-slice/blob/main/docs/examples.md) for the complete walkthrough.

---

## 📦 Templates

### `vsa-webapi` — Project Template

Scaffolds a complete, production-ready Web API from scratch.

```bash
dotnet new vsa-webapi -n <ProjectName> [options]
```

| Option | Short | Default | Choices |
|--------|:-----:|---------|---------|
| `--Framework` | `-F` | `net10.0` | `net10.0`, `net9.0` |
| `--DbProvider` | `-D` | `PostgreSQL` | `PostgreSQL`, `SqlServer`, `Sqlite` |
| `--ApiDocsTool` | `-A` | `Scalar` | `Scalar`, `Swagger` |
| `--CacheTool` | `-C` | `OutputCache` | `OutputCache`, `None` |
| `--AuthProvider` | `-Au` | `None` | `None`, `Keycloak`, `Identity` |

> These options are also exposed in the **Visual Studio / Rider New Project dialog** via `.template.config/ide.host.json`.

<details>
<summary><b>🖼️ See it in Visual Studio</b></summary>

The template appears in the **Create a new project** dialog and exposes all configuration options through the wizard:

<p align="center">
  <img src="https://raw.githubusercontent.com/SaadAlsaree/net-vertical-slice/main/image/vsa-wizard.png" alt="Wolverine VSA Web API template in the Visual Studio New Project dialog" width="720" />
</p>

<p align="center">
  <em>Select the template, then choose your database provider, API docs tool, caching, and authentication provider in the next step.</em>
</p>

<p align="center">
  <img src="https://raw.githubusercontent.com/SaadAlsaree/net-vertical-slice/main/image/vsa-wizard-config.png" alt="Configuring VSA template options in the Visual Studio wizard" width="720" />
</p>

</details>

### `vsa-feature` — Item Template

Generates a full CRUD feature inside an existing VSA project.

```bash
dotnet new vsa-feature -n <EntityName> [options] --output .
```

| Option | Required | Default | Description |
|--------|:--------:|---------|-------------|
| `-n` / `--name` | ✅ | — | Entity name in PascalCase (e.g., `Product`) |
| `--ResourceName` | ✅ | — | API resource in kebab-case (e.g., `products`) |
| `--Category` | ❌ | `Core` | Entity category/namespace (e.g., `Catalog`) |
| `--RootNamespace` | ❌ | `MyApp` | Project root namespace |
| `--DbContext` | ❌ | `AppDbContext` | DbContext class name |

**Generates 22 files:**

```
Data/Entities/{Category}/{Entity}.cs
Data/Configuration/{Entity}Configuration.cs
Features/{Entity}Feature/
├── Create{Entity}/      Command · Handler · Endpoint · Validator
├── GetById{Entity}/     Query · Handler · Endpoint · ViewModel
├── GetList{Entity}/     Query · Handler · Endpoint · ViewModel
├── Update{Entity}/      Command · Handler · Endpoint · Validator
└── SoftDelete{Entity}/  Command · Handler · Endpoint
Mappings/{Entity}MappingConfig.cs
```

<details>
<summary><b>📋 Post-generation steps</b></summary>

The generated handlers use `_db.Set<{Entity}>()`, so the project compiles immediately. To persist data:

1. **Register the DbSet** in `Data/AppDbContext.cs`:
   ```csharp
   public DbSet<Product> Products => Set<Product>();
   ```
2. **Fix irregular plurals** if needed (e.g., `Categories`, not `Categorys`).
3. **Migrate:**
   ```bash
   dotnet ef migrations add AddProduct -o Data/Migrations
   dotnet ef database update
   ```
</details>

> **💡 Naming tip:** The entity name (`-n`) and URL resource (`--ResourceName`) are independent. For irregular plurals, pair them carefully — e.g. `-n Category --ResourceName categories`. These exact pairs are covered by the template test suite.

---

## 🏛️ Architecture

Code is organized **by feature**, not by technical layer. Each feature is a self-contained vertical slice:

```
HTTP Request
   │
   ▼
Endpoint (Wolverine.Http)         ← [WolverinePost/Get/Put/Delete] + OpenAPI annotations
   │  dispatches via IMessageBus
   ▼
FluentValidation middleware       ← runs automatically before the handler
   │
   ▼
Handler (sealed, primary-ctor DI)← business logic · AppDbContext · IMapper
   │
   ▼
EF Core (AppDbContext)            ← entities, configs, soft-delete filter, audit interceptor
   │
   ▼
Response<T>                       ← standardized envelope returned to the client
```

### Core infrastructure (shipped in every project)

| Component | Responsibility |
|-----------|---------------|
| **`Response<T>`** | API envelope: `Succeeded`, `Data`, `Message`, `Code`, `Errors` |
| **`Result<T>` / `Error`** | Internal domain result pattern with typed errors (`NotFound`, `Conflict`, `Forbidden`…) |
| **`PagedList<T>`** | Pagination with `MaxPageSize = 200` + `HasPreviousPage`/`HasNextPage` |
| **`BaseEntity`** | `Id`, audit fields, soft-delete fields, `[Timestamp] RowVersion` |
| **`AuditSaveChangesInterceptor`** | Stamps `CreatedAt`/`LastUpdateAt`, converts `Delete` → soft-delete |
| **`GlobalExceptionHandler`** | Maps exceptions → HTTP status, adds `X-Correlation-ID` to every error |
| **`QueryableExtensions.ApplySort`** | Dynamic, case-insensitive property sorting |

### Technology stack

| Technology | Version | Purpose |
|------------|---------|---------|
| ASP.NET Core | .NET 10 | Web framework |
| WolverineFx (+ Http, FluentValidation) | 5.1.0 | CQRS mediator & HTTP routing |
| EF Core (+ provider) | 10.0.0 | ORM |
| FluentValidation | 11.10.0 | Input validation |
| Mapster (+ DependencyInjection) | 7.4.0 / 1.0.1 | Object mapping |
| Serilog (Console, File) | 9.0.0 / 6.0.0 | Structured logging |
| Scalar.AspNetCore | 2.1.3 | API reference UI |

---

## 📂 Project Structure

```
net-vertical-slice/
├── SKILL.md                          # AI skill: full VSA pattern reference
├── README.md                         # This file
├── image/                            # README screenshots
├── docs/                             # Extended documentation
│   ├── examples.md                   # End-to-end walkthroughs
│   ├── CONTRIBUTING.md               # Contribution guide
│   └── IMPROVEMENTS.md               # Improvement suggestions
│
└── templates/
    ├── NetVerticalSlice.Templates.csproj  # NuGet pack project (v1.1.0)
    │
    ├── vsa-webapi/                        # 📦 PROJECT TEMPLATE
    │   ├── .template.config/              #   template.json, ide.host.json, icon
    │   ├── Program.cs                     #   Wolverine + EF + Mapster + FV wiring
    │   ├── VsaTemplate.csproj             #   conditional PackageReferences
    │   ├── Directory.Packages.props       #   central package versions
    │   ├── Data/                          #   AppDbContext, BaseEntity, audit, Sample
    │   ├── Features/SampleFeature/        #   full CRUD sample (5 slices)
    │   ├── Infrastructure/Common/         #   Response, Result, Error, PagedList
    │   ├── Extensions/                    #   GlobalExceptionHandler, Queryable, Auth
    │   └── Mappings/                      #   SampleMappingConfig
    │
    ├── vsa-feature/                       # ✂️ ITEM TEMPLATE
    │   ├── .template.config/template.json #   symbols & post-actions
    │   ├── Data/                          #   entity + EF config templates
    │   ├── Features/__Entity__Feature/    #   5 CRUD slice templates
    │   └── Mappings/                      #   Mapster config template
    │
    └── TemplateTests/                     # ✅ xUnit verification harness
```

---

## 📚 Documentation

| Document | What's inside |
|----------|---------------|
| [**📖 docs/examples.md**](https://github.com/SaadAlsaree/net-vertical-slice/blob/main/docs/examples.md) | 7 end-to-end walkthroughs: zero-to-running, scaffolding features, annotated anatomy, customizations, cURL calls, Keycloak, SQLite |
| [**🤖 SKILL.md**](https://github.com/SaadAlsaree/net-vertical-slice/blob/main/SKILL.md) | The AI skill: every CQRS pattern, naming conventions, code-style rules, best-practices checklist |
| [**🤝 docs/CONTRIBUTING.md**](https://github.com/SaadAlsaree/net-vertical-slice/blob/main/docs/CONTRIBUTING.md) | How to edit templates, the verification workflow, packaging & publishing |
| [**💡 docs/IMPROVEMENTS.md**](https://github.com/SaadAlsaree/net-vertical-slice/blob/main/docs/IMPROVEMENTS.md) | Prioritized findings & suggestions for docs and DX |

---

## 🛠️ Commands Reference

```bash
# Create a new VSA Web API
dotnet new vsa-webapi -n <Name> [-F net10.0|net9.0] [-D PostgreSQL|SqlServer|Sqlite] \
                              [-A Scalar|Swagger] [-C OutputCache|None] [-Au None|Keycloak|Identity]

# Generate a CRUD feature inside an existing project
dotnet new vsa-feature -n <Entity> --ResourceName <kebab> [--Category Core] \
                         [--RootNamespace MyApp] [--DbContext AppDbContext] --output .

# Verify templates build (contributors)
dotnet test templates/TemplateTests/TemplateTests.csproj --configuration Release

# Pack into a NuGet package (maintainers)
dotnet pack templates/NetVerticalSlice.Templates.csproj -c Release
```

<details>
<summary><b>🔧 Uninstall</b></summary>

```bash
# If installed from NuGet
dotnet new uninstall NetVerticalSlice.Templates

# If installed from a local path
dotnet new uninstall ./templates/vsa-webapi
dotnet new uninstall ./templates/vsa-feature
```
</details>

---

## 🤝 Contributing

Contributions are welcome — new slices, additional providers, docs fixes, or test coverage.

1. **Fork & branch** off `main` — [**fork the repo**](https://github.com/SaadAlsaree/net-vertical-slice/fork).
2. **Make your change** in the relevant template under `templates/`.
3. **Verify locally** — the fastest check is the test harness:
   ```bash
   dotnet test templates/TemplateTests/TemplateTests.csproj --configuration Release
   ```
4. **Keep options consistent** — if you add a `template.json` choice, update `ide.host.json`, the conditional `PackageReference`s, and `#if` blocks.
5. **Open a [pull request](https://github.com/SaadAlsaree/net-vertical-slice/pulls).**

Found a bug or have an idea? [**Open an issue**](https://github.com/SaadAlsaree/net-vertical-slice/issues/new).

See [**docs/CONTRIBUTING.md**](https://github.com/SaadAlsaree/net-vertical-slice/blob/main/docs/CONTRIBUTING.md) for the full guide.

> **🔒 Security:** never commit API keys or secrets. NuGet publishing uses the `NUGET_API_KEY` environment variable only.

---

## 📄 License

This project is licensed under the **MIT License** — see [LICENSE](https://github.com/SaadAlsaree/net-vertical-slice/blob/main/LICENSE) for details.

---

## 👤 Author

**Saad Alsaree**

[![GitHub](https://img.shields.io/badge/GitHub-SaadAlsaree-181717?logo=github&logoColor=white)](https://github.com/SaadAlsaree)

---

## 🙏 Acknowledgements

Built on outstanding open-source work:

- [**Wolverine**](https://wolverinefx.net/) — CQRS & messaging
- [**EF Core**](https://learn.microsoft.com/ef/) — Object-relational mapping
- [**FluentValidation**](https://fluentvalidation.net/) — Input validation
- [**Mapster**](https://github.com/MapsterMapper/Mapster) — Object mapping
- [**Serilog**](https://serilog.net/) — Structured logging
- [**Scalar**](https://github.com/scalar/scalar) & [**Swagger (Swashbuckle)**](https://github.com/domaindrivendev/Swashbuckle.AspNetCore) — API documentation (your choice)

---

<div align="center">

<sub>Built with ❤️ for the .NET community</sub>

**[⬆ Back to top](#top)**

</div>
