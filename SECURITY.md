# Security policy

## Status

This repository currently contains design documents and does **not** publish a supported Windows executable. Security testing and product support commitments will be defined when an implementation is released.

## Reporting vulnerabilities

Avoid publicly disclosing a reproducible exploit against a future input-hook or process-control build. Use GitHub's **private vulnerability reporting** mechanism if it is enabled for this repository. If it is unavailable, request a private contact channel from the maintainer without posting sensitive exploit details.

No response-time SLA or bug-bounty program is promised.

## Security expectations for future implementations

- No input-event recording, keyboard content capture, screenshots or movement-history uploads.
- No administrator rights, drivers, services, global hooks beyond strictly necessary pointer routing, or persistent OS cursor clip by default.
- Fail open on uncertain monitor identity, topology error, unhandled hook failures, and abnormal program termination.
- No unsolicited network connections, telemetry or hidden update channels.
- Minimize third-party dependencies and review their source, permissions and distribution terms.
- Do not include secrets or personal machine identifiers in issues, tests, binaries or diagnostic logs.

The [MIT license](LICENSE) supplies the applicable warranty and liability terms; this file does not replace them.
