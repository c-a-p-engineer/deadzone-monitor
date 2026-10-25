# DeadZone Monitor

A Windows utility **in early Phase 1 implementation** to make a computer with a partly damaged screen usable by excluding bad pixels from pointer movement, window placement and everyday controls.

> **Status: Phase 1 experimental core source added; no usable Windows app yet.** The pure geometry model and push-through state machine are source-only and have **not been compiled or run in this environment**. No Win32 input hook, automatic enforcement, CI workflow or release exists.

## Product target: Mode A first

- Configure an **arbitrary rectangular dead zone** on a selected display, with physical-pixel X/Y/width/height and visual preview. Not limited to halves or edges.
- Select one intact **main work area** for window maximization and a compact dedicated taskbar, while keeping other intact pixels available as auxiliary regions.
- **Push-through cursor jump:** stop at the forbidden boundary first, then jump into reachable healthy space after further intentional movement. Sensitivity is user-configurable; **40 px-equivalent is a trial default**, not a measured optimum. Preserve existing Windows monitor resolutions/layout and access to external screens.
- Keep supported windows inside intact space: move first, resize if needed, maximize within main, and **stop active window dragging at the forbidden boundary** instead of snapping back only on mouse release (best effort for OS/app minimum sizes).
- Provide a familiar dedicated launcher/taskbar **dockable on all four edges of the main work rectangle** (bottom default) with visibility/coexistence choices, plus essential system controls, so a laptop can be used **without any external monitor**.
- On subsequent **Windows sign-ins**, automatically start and restore the previous valid enabled settings. First run requires explicit setup; an accessible keyboard-only emergency disable is mandatory.
- Work offline as a normal user with no telemetry or driver requirement.

**Future Mode B:** investigate use of the Windows pre-sign-in password/PIN screen on a damaged display. Ordinary desktop applications cannot promise secure logon desktop relocation.

## Current scope and limitations

Initially one rectangular excluded area per selected monitor profile; the data model may later support multiple damaged regions. The main area is not a new Windows monitor. Protected UAC/login screens, every fullscreen game, native Start/notification positioning, and all third-party tray icons are not guaranteed.

The **final push-through threshold/units, portal destination mapping, native Windows taskbar coexistence and exact system-control implementation** remain to be resolved by prototypes and user testing. Product acceptance is documented, but functionality is not claimed as shipped.

## Read the specs

- [Behavior contract and agreed decisions](docs/behavior-contract.md)
- [Proposed architecture and safety model](docs/architecture.md)
- [Phased development and open decisions](docs/development.md)
- [Agent/contributor guidance](AGENTS.md)
- [Contributing](CONTRIBUTING.md) · [Security](SECURITY.md)

Task-focused local skills live in [`.agents/skills/`](.agents/skills/). They are not copies of any private repository.

## Phase 1 implementation (experimental)

The code under `src/DeadZoneMonitor.Core/` uses C# / .NET 10 with **no Windows hooks** or external NuGet dependencies:

- `Geometry.cs`: validated physical-pixel rectangles, selected monitor identity, intact workspace candidates, forward landing and single-target offset correction.
- `PushThroughRouter.cs`: deterministic blocked-edge gesture state, normal-only diagonal accumulation, first-contact corner priority, retreat reset, no-target safety.
- `tests/DeadZoneMonitor.Core.Tests/`: a dependency-free console scenario harness covering these decisions. This is **test source, not test evidence** until run with the selected SDK.

Run with the .NET 10 SDK:

```powershell
dotnet run --project tests/DeadZoneMonitor.Core.Tests/DeadZoneMonitor.Core.Tests.csproj
```

**Important:** this is not a cursor-control executable. The future Windows input adapter must provide validated boundary contacts and raw attempted motion; until the emergency hotkey, teardown path and actual Windows tests exist, nothing should intercept the user's pointer. Do not auto-start or install a mouse hook from this code. Current SDK and test behavior remain unverified on Windows. No GitHub Actions workflow was added.

## License

[MIT License](LICENSE). Use, modify, redistribute and sell subject to the stated license conditions. Supplied **as-is, without warranties**, subject to the license text.

## Safety

Before testing an input-control prototype, verify keyboard-only disable, fail-open behavior and normal input after process termination on a real Windows machine. Compile success does not prove input safety.
