# Architecture and safety design (proposed)

This document owns **technical proposals**, not user-visible requirements. The agreed behavior and acceptance criteria live in [behavior-contract.md](behavior-contract.md). No Windows mechanisms below have yet passed a real prototype test.

## Target product mode

**Mode A first:** after Windows sign-in, a normal per-user desktop application loads and applies the previous valid enabled configuration. The laptop must be useful on its surviving display area alone and must still permit optional external-monitor movement.

**Mode B later:** Windows sign-in/password/PIN screen support is a separately researched future feature. Ordinary user-mode hooks, startup items and a dedicated taskbar do not control the secure logon desktop. Do not promote a tentative Mode B idea into a Mode A dependency.

## Proposed module boundaries

1. **MonitorTopology** — enumerate display identities/bounds, mapping from physical to virtual coordinates and DPI-aware snapshot changes; return an explicit unresolved result for ambiguous targets.
2. **ZoneModel** — validate one arbitrary target-local physical-pixel rectangle and user-selected main intact rectangle, expose full allowed regions including auxiliary areas. Support a future list of rectangles without exposing it as an implemented feature.
3. **PointerRouter** — pure geometry and **push-through state machine** consuming allowed topology, prior pointer position, attempted movement/direction and movement accumulation. Return stop, normal movement, mapped jump, or fail-open.
4. **Win32InputAdapter** — bounded event handling and safe cursor reposition/hook lifecycle; needs real Windows validation and must not interfere with unrelated keyboard/mouse input.
5. **WindowGuard** — best-effort new/restore/maximize rescue to the main rectangle; preserve intentional auxiliary placement where possible and leave other monitors untouched.
6. **SafeDesktopShell** — independent dedicated taskbar, launcher, app switching, settings and system control access in the main rectangle. It is **not** an Explorer shell replacement. Native taskbar coexistence and optional hiding must be reversible.
7. **ProfileLifecycle** — first-run review/enable, persisted last-enabled profile, sign-in autostart, topology validation, hotkey disable, failure status, safe shutdown.

Keep engine, shell and persistence independently testable. Failure of the shell UI must not prevent emergency keyboard disable.

## Geometry and identity

- Target-local dead zone: `{x, y, width, height}` in **physical pixels**, with a positive size and coordinates inside the display's current rotated physical bounds.
- Main work area: one separate rectangular subset of the intact area, selected by the user; it cannot overlap the dead zone.
- Auxiliary areas: allowed portions outside main. Do not model them as separately enumerated OS monitors or assume they are all rectangles.
- Convert to Windows virtual desktop pixel coordinates in a DPI-aware adapter. External displays may have negative origins, gaps, different resolutions, scaling and rotation.
- Never infer the built-in display solely from 'primary', `DISPLAY1`, enumeration order or x=0. Saved profiles must be validated against the actual selected display on every activation.
- If screen identity or geometry cannot be trusted, suspend guarding and expose an accessible diagnostic.

## Push-through cursor semantics

Agreed interaction: **do not jump merely because a boundary was touched**. The pointer stops on the permitted side; additional deliberate motion in the blocked direction triggers a jump to a reachable allowed region (on the same display or an external monitor).

Unsettled implementation policy that must be measured and reviewed:

- Accumulation of *attempted* delta while blocked; source may require raw relative mouse data or equivalent. An absolute clamped cursor position alone is not enough to infer repeated push intent.
- Adjustable trigger threshold; exact number and units are TBD (a previously mentioned 30px was an illustration only).
- Reset/decay on movement away, timeout, direction change, or topology change; do not hard-code behavior without usability tests.
- Portal target resolution among adjacent valid regions and external monitors, with stable reverse traversal and safe landing coordinates.
- Behavior at rectangle corners, along diagonal movement, with drag buttons pressed, and on high-speed moves.
- Suppressing synthetic-movement feedback loops; no accidental clicks, stuck cursor, excessive CPU or mouse-hook deadlock.

If no valid region exists in the attempted direction, stop at the permitted edge instead of teleported movement. If the intended destination cannot be resolved safely, fail open or provide a recoverable stop/explicit crossing method—never trap the user.

### Why not just ClipCursor?

A single global rectangular clamp cannot express a usable right-hand portion plus external screens positioned beyond a forbidden central/left region. The permitted set is a union of areas with explicit crossing transitions. The Windows input adapter is **a hypothesis to validate**, not a guarantee of universal pixel-perfect interception; `WH_MOUSE_LL` is a candidate with callback/timing limitations.

## Main area, windows and desktop shell

The main work area is the default placement and maximize rectangle. The dedicated taskbar is hosted within it. Ordinary windows may be moved into auxiliary areas by the user; a best-effort rescue should not fight legitimate repositioning.

The OS still knows the full physical display rectangle: modifying a normal window work area or adding an appbar does **not** automatically shrink all Windows shell surfaces or transform the panel into a virtual monitor. The product offers reachable equivalent controls where repositioning system panels is unreliable.

SafeDesktopShell should focus on:

- App launch/search/pinned/running-app switching.
- File Explorer and Settings access from intact space.
- Audio, networking and battery/clock information plus power controls.
- Narrow-main-area responsive layout.
- Reversible native-taskbar coexistence policy.

Third-party notification-area icons, native Start, protected overlays and all possible dialogs are **not** assumed trivially clonable. Test and document missing functions before claiming standalone usefulness.

## Startup, persistence and recovery

```text
UNCONFIGURED --[select/preview + first opt-in]--> ACTIVE
ACTIVE       --[user disable]---------------> DISABLED
ACTIVE       --[topology mismatch/error]----> FAIL_OPEN
DISABLED     --[explicit enable + valid profile]--> ACTIVE
FAIL_OPEN    --[validated recovery + allowed restart]--> ACTIVE
ACTIVE --[sign-out/process exit]--> OS native pointer behavior
next sign-in --[saved last-enabled profile + valid target]--> ACTIVE
next sign-in --[invalid/missing/ambiguous target]---------> FAIL_OPEN
```

Persist settings for the Windows user, not device-wide driver state. Subsequent **sign-in** autostart with the last intentionally enabled profile is required; initial install/first run does not silently enable blocking. Resolve how a user who explicitly disabled the feature expects persistence before implementation—never override a deliberate disable.

A keyboard-only emergency disable must work without a visible shell, tray or external monitor. Disabling/exit/crash must leave no persistent `ClipCursor`, injected driver, modified resolution or blocked input. Topology changes must be applied atomically or guarded mode suspended.

## Safety/privacy and operational boundaries

- Windows normal desktop, per-user privileges; drivers, services, elevation, credential providers and secure desktop manipulation are out of Mode A.
- No keystroke text logging, mouse path history, screenshots, telemetry, hidden network dependencies or unconsented privileged actions.
- Shell app and pointer hook failures are separately contained; no open-ended synchronization, hot-loop or recursive warp.
- New GitHub Actions workflows and publishing/signing require explicit maintainer approval under [AGENTS.md](../AGENTS.md).

## Next evidence needed

Windows proof of concept to validate push-through delta capture, external portal behavior, emergency disable and shell/window interactions. Unit geometry tests cannot establish OS login-screen support or third-party shell compatibility. Track rollout and remaining questions in [development.md](development.md).
