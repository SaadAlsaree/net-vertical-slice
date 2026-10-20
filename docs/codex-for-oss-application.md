# Codex for Open Source application draft

This is a maintainer working draft, not a claim that the project is critical
infrastructure. Refresh the public metrics immediately before submitting the form.

## Evidence snapshot

Snapshot date: **2026-08-02**

| Signal | Verified value | Source or method |
|---|---:|---|
| NuGet total downloads | 1,125 | NuGet public search API |
| Latest NuGet version | 2.0.0 | NuGet public search API |
| Public NuGet releases | 6 | Published package version history |
| GitHub stars | 2 | Public repository page |
| GitHub forks | 2 | Public repository page |
| Automated test cases | 15 | xUnit cases in `templates/TemplateTests/` |
| Core project-template option combinations | Up to 72 | 3 databases × 2 docs × 2 cache × 3 auth × 2 observability |

Do not convert total downloads into a monthly number. NuGet's total is exact for the
snapshot, while the package page's daily average and version counts change over time.
Use the refreshed total and its date unless a trustworthy monthly value is collected
for a clearly stated period.

## Honest positioning

Use these points:

- The project is active, public, MIT-licensed, packaged on NuGet, and maintained by
  its primary maintainer.
- It provides two coordinated templates: a .NET 10 Wolverine Vertical Slice API and
  a repeatable feature-slice generator.
- Its value is reducing duplicated architecture and inconsistent scaffolding for
  .NET teams, not being a runtime dependency used by critical infrastructure.
- The maintenance burden comes from generated output across up to 72 core option
  combinations, framework and package upgrades, security-sensitive defaults,
  documentation synchronization, and release verification.
- Current adoption is early. Do not describe it as broadly adopted or critical.

Avoid these claims unless future evidence supports them:

- "Critical infrastructure" or "widely adopted."
- "Production-ready" without the qualification "production-oriented, not
  production-complete."
- "All combinations are tested." The committed suite currently uses 15 focused
  cases; it does not exhaustively execute all 72 combinations.
- Monthly downloads inferred from the all-time total.

## Form answer: why is this repository eligible?

Character limit: 500. Refresh the dated download value before submitting.

> NetVerticalSlice.Templates is an actively maintained, MIT-licensed .NET 10 template package for Wolverine Vertical Slice APIs. As of 2026-08-02, it has 1,125 total NuGet downloads across six public releases. Its two coordinated templates reduce repeated architecture work for .NET teams, while a 72-combination option space creates ongoing testing, dependency, release, documentation, and security-maintenance work.

## Form answer: how would API credits be used?

Character limit: 500.

> API credits would automate contributor PR summaries, issue triage, dependency-upgrade impact analysis, release-note drafting, and generated-project verification. Codex would help select and test high-risk cases across the 72-option matrix, inspect generated output for security regressions, and keep documentation synchronized with template behavior. Maintainers would review every change before merge or release.

## Form answer: anything else to consider?

Character limit: 500.

> The project is production-oriented rather than production-complete and does not claim to be critical infrastructure. Its value is reducing inconsistent boilerplate for teams adopting .NET Vertical Slice Architecture. Support would help expand cross-platform verification, security guidance, contributor workflows, and evidence of real-world adoption while keeping the package free and MIT-licensed.

## Proposed Codex workflow

Codex would assist with bounded maintenance tasks while the maintainer retains final
review and release authority:

1. **Contribution review:** summarize the behavior change, identify affected template
   options, propose missing tests, and flag documentation or security impact.
2. **Issue triage:** classify reproducibility and impact, request missing template
   parameters, and connect reports to existing tests or roadmap items.
3. **Dependency upgrades:** inspect release notes and advisories, generate affected
   variants, compare package output, and prepare a reviewed upgrade pull request.
4. **Security review:** scan template and generated output, validate findings, and
   draft private remediation steps without publishing an unpatched vulnerability.
5. **Release verification:** run the representative matrix, build and publish
   generated projects, verify package contents, and draft release notes and upgrade
   guidance.

Codex must not merge, tag, publish to NuGet, disclose a vulnerability, or change
security-sensitive defaults without maintainer review.
