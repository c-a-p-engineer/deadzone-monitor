# Repository agent instructions

This file governs AI-assisted work in **deadzone-monitor**. It is self-contained: no private repository, off-repo memory, personal profile, or external agent identity is required to contribute.

## Purpose and source of truth

Build a Windows utility that excludes damaged parts of a *specific* monitor from pointer and window use, while external displays remain reachable.

Read before substantive work, in this order:

1. This file and the current repository state, including the actual default branch.
2. [Behavior contract](docs/behavior-contract.md): user-visible requirements and acceptance criteria.
3. [Architecture](docs/architecture.md): safety model and system boundaries.
4. [Development plan](docs/development.md): status, phase boundaries, tests.
5. The specific local skill(s) needed for the task. Do not load unrelated skills.

User instructions and platform safety restrictions take precedence over repository guidance. The currently committed code and tests are evidence of **current behavior**; a planned document is not proof of implementation.

## Skill routing (read the complete selected SKILL.md)

| Task | Local skill |
| --- | --- |
| Enumerating monitors, identifying internal panel, DPI, hot-plug, screen layout | `.agents/skills/monitor-topology/SKILL.md` |
| Mouse movement filtering, dead-zone entry prevention, intermonitor crossing, fail-open | `.agents/skills/deadzone-pointer-routing/SKILL.md` |
| Window rescue, work area, maximize and application exceptions | `.agents/skills/window-placement/SKILL.md` |
| Test evidence, Windows builds, release packaging, distribution, regression | `.agents/skills/verification-release/SKILL.md` |

Read multiple skills when a change crosses boundaries. The skills provide domain procedures; this file owns repository-wide rules. Do not duplicate these rules in every skill.

## Hard behavioral requirements

- **Never trap the user.** Provide a keyboard-accessible escape path and fail open when topology, identity, hooks, or settings are unreliable.
- Restrict only the configured damaged region on the identified target display. Never apply a whole-desktop cursor clip.
- External monitor navigation must remain possible even if a straight-line path would cross the disabled portion of the laptop panel.
- Do not assume the internal display is primary, leftmost, at virtual coordinate (0,0), or the first enumerated monitor.
- Keep pointer handling extremely lightweight; avoid blocking calls, recursive warps, busy waiting, and input interception unrelated to routing.
- Do not elevate privileges, install drivers, modify display EDIDs/resolutions, or alter OS-wide accessibility settings without separate explicit approval.
- Secure desktops, login screens, and exclusive-fullscreen software are outside guaranteed coverage; do not claim otherwise.
- Default to privacy: no telemetry, keystroke recording, mouse-path history, screenshots, or network dependency.
- Always distinguish `specified`, `implemented`, `tested on Windows`, and `released`.

## Change and verification workflow

1. Inspect current files, branch, pending work, and existing tests before changing anything.
2. State the affected behavior and smallest verifiable change; don't build unnecessary framework abstractions.
3. For behavior changes, add or update synthetic multi-monitor geometry tests (including negative coordinates) and recovery tests.
4. For Windows-specific input/window code, include a manual test procedure; non-Windows CI cannot establish native integration correctness.
5. Test keyboard escape and process-exit recovery **before** enabling any automatic startup.
6. Review diffs, secrets, license notices, external dependencies, and regressions.
7. Report exactly what was changed, which commands actually ran, what remains unverified, and links to commit/PR.

### Approval boundaries

- New or substantively repurposed GitHub Actions workflows, privilege expansion, paid runners/services, drivers, code-signing purchases, and public releases require maintainer approval before execution.
- Publishing anything that claims to be a tested Windows release requires real validation evidence; a green compile is not enough.
- Never overwrite concurrent upstream changes; re-check the target branch before writing.

## Documentation boundaries

- `docs/behavior-contract.md` owns externally observable behavior.
- `docs/architecture.md` owns internal design and rationale.
- `docs/development.md` owns task progress, acceptance plan, and known gaps.
- Skills own **task-specific procedures**, not product requirements or personal identity.
- Keep local documentation concise; link to its owner rather than copying a contract.

## Completion rule

For each task, identify completion criteria, verify against artifacts and tests, and label unverified portions honestly. A document, marker, green compilation, or tool success response alone does not prove pointer safety on a real Windows desktop.
