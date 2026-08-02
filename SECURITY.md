# Security Policy

Security reports are welcome. Net Vertical Slice Templates generates application
source code, so a defect in the template can be copied into every newly generated
project. Please report suspected vulnerabilities privately before opening a public
issue.

## Supported versions

| Version | Security support |
|---|---|
| 2.x | Supported |
| 1.x | Best effort; upgrade to the latest release is recommended |
| Older or modified generated projects | Not directly supported |

Security fixes are normally released from the latest maintained version. Installing
a newer template does not update projects that were generated previously, so
maintainers of affected applications must review the advisory and apply the relevant
source changes themselves.

## Report a vulnerability

Use GitHub's private vulnerability-reporting flow:

<https://github.com/SaadAlsaree/net-vertical-slice/security/advisories/new>

Please include:

- The affected template, version, and selected options.
- A concise description of the impact and realistic attack scenario.
- Reproduction steps or a minimal generated project when possible.
- The affected files and any suggested mitigation.
- Whether the report or proof of concept may be shared publicly after a fix.

Do not include secrets, access tokens, personal data, or production credentials in
the report. Please do not open a public issue for an unpatched vulnerability.

## What to expect

The maintainer will aim to acknowledge a complete report within seven days, assess
its impact, and coordinate a fix and disclosure timeline when the issue is valid.
Complex reports may require additional reproduction details. Credit will be given
to reporters who request it, unless doing so would create additional risk.

## Scope

Reports are in scope when they concern:

- Source emitted by `vsa-webapi` or `vsa-feature` with supported options.
- Authentication, authorization defaults, token handling, rate limiting, secrets,
  error handling, dependency choices, or generated deployment configuration.
- The template packaging, release workflow, or repository automation.
- A reproducible dependency vulnerability that materially affects generated output.

Application-specific business logic, infrastructure configured after generation,
and unsupported local modifications are generally outside this project's direct
scope. They may still reveal a documentation or default-setting problem, so reports
with a clear connection to generated output are welcome.

## Security expectations for generated projects

The templates are production-oriented, not production-complete. Consumers remain
responsible for reviewing authorization policies, secrets, migrations, proxy and
CORS configuration, dependency updates, domain validation, audit requirements, and
deployment-specific controls before production use.
