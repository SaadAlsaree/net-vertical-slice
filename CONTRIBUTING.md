# Contributing to Net Vertical Slice Templates

Thank you for helping improve the templates. Contributions can include bug reports,
documentation, tests, dependency updates, security improvements, and focused feature
proposals.

## Before you start

- Use [GitHub Issues](https://github.com/SaadAlsaree/net-vertical-slice/issues) to
  describe a reproducible bug or propose a substantial change.
- Check the [roadmap](ROADMAP.md) for planned work and stated non-goals.
- Report suspected vulnerabilities through the private process in
  [SECURITY.md](SECURITY.md), not through a public issue.
- Keep pull requests focused. Separate unrelated formatting, dependency, and
  behavior changes where practical.

## Repository layout

| You want to change | Edit here |
|---|---|
| Generated Web API source or structure | `templates/vsa-webapi/` |
| Generated feature files | `templates/vsa-feature/` |
| CLI or IDE options | `templates/vsa-webapi/.template.config/template.json` and `ide.host.json` |
| Conditional packages | `templates/vsa-webapi/VsaTemplate.csproj` and `Directory.Packages.props` |
| Package metadata or version | `templates/NetVerticalSlice.Templates.csproj` |
| Verification coverage | `templates/TemplateTests/` |
| Package-facing documentation | `README.md` and `templates/README.md` |

`README.md` and `templates/README.md` intentionally contain the same package-facing
content. Update both in the same pull request so the repository and NuGet package do
not drift.

## Template substitution conventions

The Web API template uses `sourceName: "VsaTemplate"`. When a user runs
`dotnet new vsa-webapi -n Store.Api`, every `VsaTemplate` occurrence is replaced
with `Store.Api`. Keep that token consistent in new source files.

The feature template targets an existing project and uses explicit tokens:

| Token | Input | Example |
|---|---|---|
| `__Entity__` | `-n`, `--name` | `Product` |
| `__Category__` | `--Category` | `Catalog` |
| `__RootNamespace__` | `--RootNamespace` | `Store.Api` |
| `__DbContext__` | `--DbContext` | `AppDbContext` |
| `__resource__` | `--ResourceName` | `products` |

## Development workflow

1. Create a branch from `main`.
2. Make the smallest coherent change.
3. Add or update tests for every affected option or generated behavior.
4. Run the verification suite:

   ```bash
   dotnet test templates/TemplateTests/TemplateTests.csproj --configuration Release
   ```

5. If package content changed, also verify packaging:

   ```bash
   dotnet pack templates/NetVerticalSlice.Templates.csproj --configuration Release
   ```

6. Open a pull request describing the generated variants tested, user-visible
   behavior, security impact, and documentation changes.

When adding or changing a template choice, update the CLI definition, IDE host
configuration, conditional package references, compile symbols, generated settings,
documentation, and representative tests together.

## Verification matrix

The project template currently has five independent option groups: three database
providers, two documentation tools, two cache choices, three authentication
providers, and two observability choices. That produces up to 72 generated project
combinations before feature-template variations.

Running every combination on every pull request is expensive. The current suite uses
15 focused cases to cover authentication output isolation, all database providers
with Identity, Keycloak, OpenTelemetry, production publishing with generated
Wolverine code, runtime validation and rate limiting, and irregular feature names.

Contributors should add the smallest high-risk combinations that prove their change.
Changes to shared conditional logic may justify expanding the matrix or running an
additional local set beyond the committed tests.

## Coding conventions

- Follow the existing generated style: file-scoped namespaces, records for messages
  and view models, sealed handlers, and focused static endpoint classes.
- Keep each vertical slice cohesive and avoid adding abstractions without a concrete
  generated use case.
- Preserve provider isolation: unselected authentication and observability code and
  packages must not appear in generated output.
- Never commit credentials, API keys, connection secrets, or real identity-provider
  configuration.
- Treat generated output as the product. A repository build alone is insufficient
  when a change can affect `dotnet new` output.

## Pull request checklist

- [ ] Generated output builds for every affected representative combination.
- [ ] Tests cover the change or the pull request explains why no test is needed.
- [ ] Security and backward-compatibility effects were considered.
- [ ] `README.md` and `templates/README.md` remain synchronized when applicable.
- [ ] User-visible changes include release-note guidance.
- [ ] No generated build output, secrets, or machine-specific files are included.

## Releases

Publishing is maintainer-only. The release workflow runs the verification suite,
packs the NuGet package, uploads the package artifact, and publishes tagged or GitHub
releases with the configured NuGet secret. Contributors should not include or request
access to publishing credentials.
