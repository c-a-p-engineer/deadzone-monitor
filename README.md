# DeadZone Monitor

A Windows utility **in design** to make a computer with a partly damaged screen usable by excluding bad pixels from pointer movement, window placement and everyday controls.

> **Status: requirements agreed / not yet implemented.** There is no working executable, tested Windows mouse hook, automated build or public release at this time.

## Product target: Mode A first

- Configure an **arbitrary rectangular dead zone** on a selected display, with physical-pixel X/Y/width/height and visual preview. Not limited to halves or edges.
- Select one intact **main work area** for window maximization and a compact dedicated taskbar, while keeping other intact pixels available as auxiliary regions.
- **Push-through cursor jump:** stop at the forbidden boundary first, then jump into another reachable healthy area after intentional further movement. Preserve access to external monitors and back.
- Keep common windows usable on the remaining visible area (best effort given Windows and app limitations).
- Provide a dedicated launcher/taskbar and essential system controls so a laptop can be used **without any external monitor**, even where native Windows shell panels cannot be relocated.
- On subsequent **Windows sign-ins**, automatically start and restore the previous valid enabled settings. First run requires explicit setup; an accessible keyboard-only emergency disable is mandatory.
- Work offline as a normal user with no telemetry or driver requirement.

**Future Mode B:** investigate use of the Windows pre-sign-in password/PIN screen on a damaged display. Ordinary desktop applications cannot promise secure logon desktop relocation.

## Current scope and limitations

Initially one rectangular excluded area per selected monitor profile; the data model may later support multiple damaged regions. The main area is not a new Windows monitor. Protected UAC/login screens, every fullscreen game, native Start/notification positioning, and all third-party tray icons are not guaranteed.

The **push-through threshold, portal destinations, Windows taskbar compatibility and exact system-control implementation** remain to be resolved by prototypes and user testing. Product acceptance is documented, but functionality is not claimed as shipped.

## Read the specs

- [Behavior contract and agreed decisions](docs/behavior-contract.md)
- [Proposed architecture and safety model](docs/architecture.md)
- [Phased development and open decisions](docs/development.md)
- [Agent/contributor guidance](AGENTS.md)
- [Contributing](CONTRIBUTING.md) · [Security](SECURITY.md)

Task-focused local skills live in [`.agents/skills/`](.agents/skills/). They are not copies of any private repository.

## Planned implementation

C# / .NET on Windows with minimal Win32 interoperability. Specific SDK and packaging are to be selected after initial prototype evidence.

## License

[MIT License](LICENSE). Use, modify, redistribute and sell subject to the stated license conditions. Supplied **as-is, without warranties**, subject to the license text.

## Safety

Before testing an input-control prototype, verify keyboard-only disable, fail-open behavior and normal input after process termination on a real Windows machine. Compile success does not prove input safety.
