# Contributing to Net Vertical Slice

Thanks for your interest in improving these templates! This guide covers the practical workflow for making changes, verifying them, and publishing.

## Repository layout (what to edit)

| You want to change… | Edit here |
|---------------------|-----------|
| The generated Web API's source/structure | `templates/vsa-webapi/` |
| The generated feature's files | `templates/vsa-feature/` |
| A new or changed CLI/IDE option | `templates/vsa-webapi/.template.config/template.json` **and** `ide.host.json` |
| Conditional NuGet packages | `templates/vsa-webapi/VsaTemplate.csproj` (conditional `ItemGroup`s) + `Directory.Packages.props` |
| The package metadata/version | `templates/NetVerticalSlice.Templates.csproj` |
| Verification tests | `templates/TemplateTests/` |

## The `VsaTemplate` → project name substitution

The Web API template uses `sourceName: "VsaTemplate"`. When a user runs `dotnet new vsa-webapi -n MyApi`, every `VsaTemplate` occurrence (in namespaces, the `.csproj`, file contents) is replaced with `MyApi`. Keep this token consistent when adding new source files.

The feature template uses explicit symbol tokens instead, because it targets an *existing* project:

| Token | Symbol | Notes |
|-------|--------|-------|
| `__Entity__` | `sourceName` | replaced with `-n` value (e.g. `Product`) |
| `__Category__` | `--Category` | default `Core` |
| `__RootNamespace__` | `--RootNamespace` | default `MyApp` |
| `__DbContext__` | `--DbContext` | default `AppDbContext` |
| `__resource__` | `--ResourceName` (required) | kebab-case URL segment |

## Development workflow

1. **Branch** off `main`.
2. **Make your change.** If you add a `template.json` choice:
   - Add it to `.template.config/ide.host.json` so the IDE dialog shows it.
   - Add the conditional `PackageReference` group in `VsaTemplate.csproj`.
   - Add the matching `#if`/`DefineConstants` block in the affected source files.
   - Add the `DefaultConnectionString` / launch-URL `switch` case if relevant.
3. **Verify locally** — the single command that proves everything still builds:

   ```bash
   dotnet test templates/TemplateTests/TemplateTests.csproj --configuration Release
   ```

   `TemplateFixture` installs both templates into temp directories, generates projects and features (including irregular-plural names like `Category`/`categories`), and confirms they `build`. If this passes, your template produces compiling output.

4. **Manually sanity-check a generated project** when changing options:

   ```bash
   dotnet new uninstall ./templates/vsa-webapi   # clear any stale install
   dotnet new install ./templates/vsa-webapi --force
   dotnet new vsa-webapi -n Scratch -D Sqlite -A Scalar -C OutputCache -Au None -o ../scratch
   cd ../scratch && dotnet build
   ```

5. **Bump the version** in `templates/NetVerticalSlice.Templates.csproj` for user-visible changes, and note it in your PR description.
6. **Open a PR** against `main`.

## Coding conventions (templates)

- Match the existing style: file-scoped namespaces, `sealed` handlers with primary-constructor DI, `static` endpoint classes, `record` commands/queries/view-models.
- Handlers return `Response<T>`. Endpoints dispatch via `IMessageBus`.
- Use `_db.Set<T>()` in the feature template (works before the user registers a `DbSet`), and the concrete `DbSet` property in the sample feature.

## Packaging & publishing

Maintainers only.

```bash
# Build the NuGet package
dotnet pack templates/NetVerticalSlice.Templates.csproj -c Release
# → templates/bin/Release/NetVerticalSlice.Templates.<version>.nupkg

# Publish (requires NUGET_API_KEY in the environment — never commit it)
dotnet nuget push ./templates/bin/Release/NetVerticalSlice.Templates.<version>.nupkg \
  --source https://api.nuget.org/v3/index.json \
  --api-key $NUGET_API_KEY
```

## CI

`.github/workflows/templates.yml` runs `dotnet test templates/TemplateTests/TemplateTests.csproj --configuration Release` on `windows-latest` with .NET 10 on every push and pull request. PRs must keep this green.
