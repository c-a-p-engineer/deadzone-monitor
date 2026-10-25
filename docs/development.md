# Development plan and verification ledger

**Current state (2026-10-25):** repository initialized with specification, agent instructions, local skills and MIT license. **Application implementation has not begun.** No build, automated Windows test, manually validated behavior, or release is claimed.

## Phase 0 — contract and repository bootstrap

- [x] Public repository initialized with authoritative behavior/safety contract.
- [x] Project-specific agent instructions and task-focused skills defined.
- [x] MIT license, security and contributor guidance added.
- [ ] Validate planned input mechanism against a real Windows host.

## Phase 1 — safe pointer prototype

- [ ] Create a small C#/.NET Windows executable and separate pure routing library.
- [ ] Enumerate monitors and require explicit selection of target.
- [ ] Implement disabled-by-default cursor routing; preserve external monitor access.
- [ ] Implement and exercise keyboard-only disable, crash/failure behavior and safe shutdown.
- [ ] Add deterministic synthetic geometry/crossing tests.
- [ ] Test on a physical laptop and at least one external display.

**Gate:** No background startup or release before escape, fail-open, and external crossings work on the actual machine.

## Phase 2 — windows and topology

- [ ] Implement best-effort safe-zone window placement and maximize behavior.
- [ ] Handle negative coordinates, mixed DPI, target identification and hot-plug.
- [ ] Test at least two external displays and unusual ordering.
- [ ] Document known exceptions; don't claim unsupported app/window coverage.

## Phase 3 — usability and packaging

- [ ] Add tray controls and optional per-user startup only after recovery tests.
- [ ] Pin SDK, dependencies, build commands, repeatable packaging.
- [ ] Implement Windows build/test workflow **after maintainer approval of new GitHub Actions usage**.
- [ ] Evaluate unsigned binary trust / SmartScreen behavior, artifact retention and release notes.
- [ ] Distribute a versioned release only after all relevant acceptance tests pass.

## Minimal validation evidence for each future PR

- Changed requirement IDs (from [behavior-contract.md](behavior-contract.md)).
- Automated test command(s), machine/OS, outcomes, and test gaps.
- Manual steps for mouse, keyboard recovery, and external monitors if behavior is affected.
- Expected fallback and process-exit behavior.
- Explicit label: **implemented**, **tested**, **unverified**, or **deferred**.

## Current open design decisions

1. Actual Windows cursor interception method and how it interacts with synthetic moves.
2. Unambiguous portal behavior for external screens located behind disabled regions.
3. Reliable built-in display identity across docks/resume; behavior when ambiguous.
4. Best-effort maximize strategy and compatibility with common apps.
5. Selected .NET SDK and per-user distribution format.

Resolve with prototypes and evidence; avoid turning speculative details into guaranteed behavior.

## Rollback policy

Keep cursor interception behind an opt-in toggle. Before changing startup behavior or installation, prove disabled mode and process exit restore normal pointer access. For a failing published release, disable startup and revert to the last verified binary.
