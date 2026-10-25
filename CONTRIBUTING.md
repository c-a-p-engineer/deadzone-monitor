# Contributing

Contributions and issue reports are welcome, but this project is currently in the **design-only** stage.

1. Read [AGENTS.md](AGENTS.md) and [the behavior contract](docs/behavior-contract.md).
2. Prefer small PRs tied to a specific requirement (e.g., `DZ-03`).
3. Include deterministic test cases for geometry, display topology and all new recovery paths.
4. For Windows hooks or window-placement behavior, provide a manual test plan and state whether it was actually run on Windows.
5. Do not introduce drivers, administrator requirements, background networking, telemetry or new GitHub Actions workflows without maintainer approval.
6. Do not report a Windows build as tested merely because it compiles.
7. Treat all multi-monitor assumptions as suspect: negative coordinates, mixed DPI, external-left, external-right, hot-plug and lost target identity matter.
8. Include the source of third-party code/assets and respect their licenses.

Please avoid submitting machine-specific personal information, screenshots of sensitive windows, raw input logs or credentials in public issues.

## Proposed PR format

- **Goal / requirement IDs:**
- **Changed paths:**
- **Automated tests run:**
- **Manual Windows tests run:**
- **Known gaps / safety fallback:**
- **New privileges, dependencies, network, or workflow changes:**

See [SECURITY.md](SECURITY.md) for vulnerability reports.
