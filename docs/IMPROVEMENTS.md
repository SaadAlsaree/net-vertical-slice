# Documentation & Developer-Experience Improvements

Findings from a full review of the repository (source, templates, tests, and existing docs), with concrete suggestions. Items are grouped by theme and marked **[Bug]**, **[DX]**, **[Docs]**, or **[Enhancement]**. Each is intentionally scoped so it can be a standalone PR.

---

## 1. Hygiene: committed build artifacts & IDE state

**[Bug]** The repository ships a large amount of generated/transient output that should be gitignored and removed:

- `templates/vsa-webapi/.vs/` — Visual Studio user state (`.suo`, `DocumentLayout.json`, file-content index). This is user-specific machine state and should never be committed.
- `templates/vsa-webapi/bin/` and `templates/vsa-webapi/obj/` — build output, including the compiled `VsaTemplate.dll` and ~150 satellite resource DLLs.
- `templates/TemplateTests/bin/` and `obj/` — test build output.
- `templates/vsa-webapi/vsa-webapi.sln` — generated solution; consider whether it should be tracked.

**Why it matters:** `.gitignore` already excludes `[Bb]in/` and `[Oo]obj/`, so these slipped in via a force-add or before the ignore rules existed. They bloat the repo, cause noisy diffs, and can confuse `dotnet new install` (the `csproj` correctly excludes them from the pack, but they still ship in clones).

**Suggested fix:**
```bash
git rm -r --cached templates/vsa-webapi/.vs templates/vsa-webapi/bin templates/vsa-webapi/obj \
                templates/TemplateTests/bin templates/TemplateTests/obj
git commit -m "chore: remove committed build artifacts and VS user state"
```
Add `.vs/` (already present) — confirm the patterns cover `.sln` if you don't want it tracked.

---

## 2. Inconsistency between the two READMEs

**[Docs]** There are currently *three* README-ish files with overlapping/conflicting content:

1. `SKILL.md` (top-level) — the comprehensive AI-skill reference.
2. `README.md` (top-level, now created) — the human-facing entry point.
3. `templates/README.md` — packaging notes (`dotnet pack`/`push`).
4. `templates/vsa-webapi/README.md` — ships *inside* generated projects.

Observed inconsistencies in the *template* parameters across sources:

| Parameter | `SKILL.md` | `templates/vsa-webapi/README.md` | `template.json` (truth) |
|-----------|-----------|----------------------------------|-------------------------|
| `-F` Framework | documented | **not mentioned** | `net10.0` / `net9.0` |
| `-Au` AuthProvider | documented | **not mentioned** | `None` / `Keycloak` |

**Suggested fix:** Make `templates/vsa-webapi/README.md` (the one users see in their generated project) the authoritative quickstart and include **all** options. The new top-level `README.md` already reconciles the table against `template.json`.

---

## 3. Documentation gaps to fill

**[Docs]** Things a new user currently has to read source to discover:

- **No LICENSE file.** README says "see repository metadata" but there's no `LICENSE`. Add one (MIT/Apache-2.0 are typical for templates).
- **No CHANGELOG.** With `Version: 1.0.2` in the csproj, a `CHANGELOG.md` would help consumers track template updates.
- **`X-Correlation-ID` behavior** is undocumented for consumers. It's a great feature (the handler reuses the request's header or generates one) — worth a one-liner in the generated project README.
- **Soft-delete semantics** are non-obvious: `Remove()` becomes a soft-delete, and reads auto-filter deleted rows. Document this explicitly in the generated README so users don't expect hard deletes.
- **`PagedList<T>.MaxPageSize = 200`** caps page size silently. Document it so API consumers aren't surprised.

---

## 4. Developer experience

**[DX]** Several low-effort wins:

- **One-command install.** Users must run `dotnet new install` twice (once per template folder) from a clone. Add a `Justfile`/`Makefile` target or a tiny `install.ps1`/`install.sh`:
  ```bash
  # install.sh / install.ps1
  dotnet new install ./templates/vsa-webapi --force
  dotnet new install ./templates/vsa-feature --force
  ```
- **`dotnet new list` verification step** in the README quickstart (already added).
- **Document the test harness as a contribution gate.** It's the single most useful local command and was only in CI. Now in `CONTRIBUTING.md`, but consider a `dotnet test` alias script.
- **`.editorconfig`** is not present. Adding one would keep generated *and* contributor code consistent (the codebase clearly follows strict conventions). It can also ship inside the generated project.
- **`Directory.Build.props`** is absent at the repo root; the test project re-declares common properties. A root props file could centralize `<Nullable>enable</Nullable>`, `<ImplicitUsings>enable</ImplicitUsings>`, and `TreatWarningsAsErrors`.

---

## 5. Template robustness

**[Enhancement]** Ideas that would make the generated output more correct/ergonomic:

- **Pluralization helper.** The feature template requires the user to pair `-n Category --ResourceName categories` correctly and then *manually* fix the `DbSet` plural. Consider a `--PluralName` symbol used in the generated `DbSet` property, or generate `_db.Set<{Entity}>()` only and add a `// TODO: add DbSet<{Entity}> {Plural}` comment with the exact line to paste.
- **`--Category` folder collision.** `Data/Entities/{Category}/{Entity}.cs` is created, but `AppDbContext` doesn't know the namespace changed. The generated handler already imports `Data.Entities.__Category__`, which is correct — but if a user picks a `Category` that doesn't exist as a folder, there's no guidance. A post-action hint would help.
- **`AUTH_KEYCLOAK` source files are always present.** `Extensions/AuthExtensions.cs`, `AuthClaims.cs`, `CurrentUserExtensions.cs`, and `Models/KeycloakSettings.cs` are wrapped in `#if AUTH_KEYCLOAK`, so they compile to empty *but still ship in the generated project*. Consider using template `Modifiers`/`condition` (or `.template.config` exclusion) so non-Keycloak projects don't get empty files. (At minimum this is cosmetic, but it can confuse readers.)
- **Migration output folder.** Both READMEs now use `-o Data/Migrations`, but `AppDbContext` doesn't pin a migrations *assembly* namespace. For non-PostgreSQL providers this is usually fine; just worth a note if users relocate migrations.
- **`GlobalExceptionHandler` uses `Response<object>`** for all error bodies, which is consistent but means the `ProducesResponseType` types on endpoints (`Response<Guid>` etc.) won't match the actual error shape at runtime. Consider documenting that error responses are always `Response<object>`, or use a shared `ErrorResponse` type for `ProducesResponseType`.

---

## 6. Testing coverage

**[Enhancement]** The current harness (`TemplateTests`) verifies that generated code **compiles** — valuable, but it doesn't catch *runtime* regressions. Suggestions, roughly in priority order:

- **Cover all `--DbProvider` × `--ApiDocsTool` × `--CacheTool` × `--AuthProvider` combinations**, not just the single `Sqlite/Scalar/OutputCache/None` build currently tested. `[Theory]` with `[InlineData]` over the matrix would catch conditional-compile bugs (e.g., a typo in an `#if AUTH_KEYCLOAK` block).
- **Add a build of the `vsa-webapi` template *with* Keycloak** — that's the only path that compiles `AuthExtensions.cs` and the OpenAPI security transformer; it's currently untested.
- **Runtime smoke test:** spin up the SQLite variant in `WebApplicationFactory`, `POST` to `/api/samples`, and assert the `Response<T>` envelope. This would catch issues the build can't (e.g., DI wiring, Wolverine endpoint mapping, Mapster config discovery).
- **Cross-platform CI.** The workflow pins `windows-latest`. Nothing in the templates is Windows-specific — adding `ubuntu-latest` would broaden coverage and catch path-casing issues.

---

## 7. Security review notes (no action needed, just documenting)

- ✅ `GlobalExceptionHandler` does not leak exception details on `500`s (returns generic message).
- ✅ `NUGET_API_KEY` is environment-only and called out as "never commit."
- ✅ Keycloak dev handler relaxes TLS validation **only in Development**.
- ⚠️  `appsettings.json` ships a `Keycloak` section with empty secrets. These are placeholders and safe, but a generated-project note recommending `appsettings.Production.json` / secret-manager for real values would be good.

---

## 8. Minor polish

- **[Docs]** `templates/vsa-webapi/README.md` step 1 hardcodes `Microchip.Api` — a fictional name that reads oddly. Use a neutral name like `MyApi` or `Sample.Api`.
- **[Docs]** The README's migration examples mix `-o Data/Migrations` (CLI) and `-OutputDir Data/Migrations` (PMC) — good, but make the CLI/PMC split visually clearer.
- **[DX]** Add a `dotnet new list vsa-webapi` line to confirm install (done in the new top-level README).
- **[Enhancement]** Consider an optional `--IncludeSampleFeature` bool (`true` by default) so users who scaffold over an existing project can skip the `SampleFeature`.

---

## Suggested PR ordering

1. **chore: remove committed build artifacts / `.vs` state** (Section 1) — smallest, highest-value cleanup.
2. **docs: add LICENSE + CHANGELOG** (Section 3).
3. **test: cover the full template option matrix + Keycloak build** (Section 6).
4. **feat: cross-platform CI** (Section 6).
5. **docs: reconcile generated-project README** (Section 2) + soft-delete/correlation-id notes (Section 3).
6. Feature enhancements (Section 5) as interest dictates.
