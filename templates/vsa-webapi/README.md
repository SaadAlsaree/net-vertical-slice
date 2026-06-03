# Wolverine VSA Web API Template

This template creates an ASP.NET Core Web API using Wolverine CQRS, Vertical Slice Architecture, EF Core, FluentValidation, and Mapster.

## 1) Create a project from the template

```bash
dotnet new vsa-webapi -n Microchip.Api -D SqlServer -A Swagger -C OutputCache
cd Microchip.Api
dotnet restore
```

Template options:

- `-D|--DbProvider`: `PostgreSQL`, `SqlServer`, `Sqlite`
- `-A|--ApiDocsTool`: `Scalar`, `Swagger`
- `-C|--CacheTool`: `OutputCache`, `None`

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

## 6) Example Microchip API calls

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
