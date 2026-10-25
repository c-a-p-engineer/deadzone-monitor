# DeadZone Monitor

A lightweight Windows utility **planned** to keep pointer and application windows out of damaged areas of a display while preserving access to external monitors.

> **Status: specification / repository bootstrap only.** No executable, tested cursor hook, or Windows release is available yet. The behavior below is the implementation target, not a claim of current functionality.

## Why

When the left half of a laptop panel is damaged, users should be able to work in the healthy right half without losing access to other monitors. Simple window tiling does not prevent pointer travel into the broken region; globally clipping the cursor also blocks legitimate multi-monitor movement.

## Goals

- Configure a disabled area on a **selected** internal display (initial target: left 50%).
- Keep the pointer out of that area during ordinary desktop interaction.
- Preserve mouse access to **all connected external displays**, even when a forbidden strip lies between them in the Windows virtual desktop arrangement.
- Keep ordinary application windows usable on the surviving portion of the internal display (best effort, including maximize handling).
- Offer an immediate keyboard-controlled **disable / fail-open** path; never leave a user trapped by the tool.
- Run locally with no required account, telemetry, or network connection.

## Non-goals and limitations

- Repairing a physically damaged panel.
- Changing Windows' physical monitor boundaries or exposing half a panel as a new monitor.
- Controlling pointer movement on the sign-in screen, secure UAC desktop, pre-boot environment, or every exclusive-fullscreen game.
- Guaranteeing perfect placement for privileged, protected, or uncooperative windows.

## Planned technology

C# / .NET for Windows with narrowly scoped Win32 interoperability. Exact SDK version and packaging are to be pinned when an implementation exists.

## Project documents

- [Agent and contributor rules](AGENTS.md)
- [Behavior and acceptance criteria](docs/behavior-contract.md)
- [Architecture and safety decisions](docs/architecture.md)
- [Milestones and verification plan](docs/development.md)
- [Contributing](CONTRIBUTING.md)
- [Security](SECURITY.md)

Task-focused agent skills are in [`.agents/skills/`](.agents/skills/). These are project-specific adaptations, **not** a copy of the private Tsuzuri core repository.

## License

[MIT](LICENSE) — you may use, copy, modify, distribute, and sell the software, subject to retaining the license notice. **Provided as-is, without warranty or liability to the extent stated by the license.** See the full license text for the governing terms.

## Safety note

Until the escape path and multi-monitor behavior have been verified on a physical Windows machine, **do not use a development build as the only input method on an important system.** A USB mouse/keyboard and a fallback method to terminate the process are recommended for initial tests.
