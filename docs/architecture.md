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
5. **WindowGuard** — best-effort new/restore/maximize safe placement, live drag constraint, and a narrowly scoped **oversized-window rescue state** that repositions one unshrinkable window in four directions without battling the normal guard; preserve intentional auxiliary placement and leave other monitors untouched.
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

## Oversized-window rescue: separate exception state

**User requirement:** for a window too large to fit any practical intact rectangular work area, offer first-version **up/down/left/right position shifting**, exposing currently hidden controls in the healthy main area without changing physical monitor settings or injecting into the app.

Candidate state machine (interaction details require agreement and Windows tests):

```text
NORMAL_WINDOW --[detected cannot fit]--> RESCUE_OFFERED(window-id)
RESCUE_OFFERED(window-id) --[explicit user starts]--> RESCUE(window-id)
RESCUE_OFFERED(window-id) --[window becomes fit, closes, or topology changes]--> NORMAL_WINDOW
RESCUE(window-id) --[direction input]--> RESCUE(window-id, shifted-position)
RESCUE(window-id) --[other application focused or ordinary minimize/restore]--> RESCUE(window-id, same shifted-position)
RESCUE(window-id) --[explicit end]--> NORMAL_WINDOW or SAFE_FALLBACK
RESCUE(window-id) --[target closed]--> NORMAL_WINDOW (clear window exception)
RESCUE(window-id) --[target lost, topology invalid, global emergency disable]--> SAFE_FALLBACK
```

- Detection is not activation: only offer rescue for a supported window that remains unfit after normal safe-placement efforts. Place a keyboard-reachable **Start rescue** control wholly inside healthy main space. Do not steal focus or immediately move the window merely because the offer appeared.
- Only after user activation, add that window's **individual rescue session** to a multi-session registry. Multiple sessions can coexist, with separately retained shift offsets and bypass of the ordinary full-containment clamp **for their own window only**. Other supported windows and the pointer remain protected. Never couple a session's lifetime to foreground focus. Ending/closing A removes only A and cannot silently remove B; invalid topology/emergency global disable may safely terminate all.
- Reposition the window using bounded Win32 APIs (for example `SetWindowPos`) and safe visible controls. This is **window movement**, not content scrolling, scaling, clipping or a true virtual-screen pan. Hidden parts remain outside the healthy viewing rectangle until moved back.
- Store per-window rescue state in-memory keyed by a *validated live-window identity* (not merely a stale or recycled numeric HWND). Track original placement, shifted placement, active status, and lifecycle; revalidate before each move. Do not persist sensitive titles/content or reapply prior-session handles across restart.
- Host **one shared rescue control panel**, as a **movable, collapsible floating window wholly constrained to the main intact rectangle**, keyboard-operable and separate from all rescued windows. Both expanded and collapsed bounds must remain valid; compact mode retains a reachable handle and a keyboard path to reopen. Maintain a single current UI target reference distinct from the per-window session registry. On genuine foreground activation of any *rescue-enabled* window, follow it; also expose a manual picker of currently valid rescue sessions. Focus changes to the panel itself or unrelated apps must never silently change targets or end sessions. Revalidate target identity before every directional command; disable movement until selection is unambiguous after close/invalidated handles. Main-geometry changes must safely relocate/restore the panel without altering per-window rescue offsets. On explicit Start rescue the shared floating controller transitions to **expanded**. After entry, **only explicit user expand/collapse actions** change its presentation; do not respond to timeout, foreground app changes or minimize/restore with auto-collapse or auto-expand. New explicit rescue activation expands the shared controller. Initial position, post-last-session visibility and manual-selection override precedence remain UX questions.
- Never fight an app enforcing its own position/minimum dimensions; stop repeated attempts and offer manual/keyboard fallback.
- Underlying Windows constraints include application-defined minimum tracking sizes (`WM_GETMINMAXINFO`) and the limits of cross-process window repositioning; **success requires real Windows integration testing**.
- Separate **agreed functionality** (automatic detection + user-activated start, four-way shifting, concurrent independent per-window rescue, one focus-follow/manual-picker **movable/collapsible floating panel**) from **undecided UX** (offer/banner style, panel startup location and persistence, post-last-session visibility, precise manual-override precedence, repeated-offer policy, step sizes and reset behavior).

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

## Further agreed UX and proposed implementation consequences (2026-10-25)

- **Push threshold:** expose user-facing sensitivity; start usability prototyping at approximately **40 px-equivalent extra inward motion**, but do not assume cursor-position changes during clipping provide that attempted delta. Calibrate actual units and reset rules on real hardware.
- **Windows monitor topology:** preserve OS-configured resolutions, positions, monitor numbering/identity and primary selection. Determine candidate destination from existing relative monitor placement, and redirect across dead pixels only after intentional inward push. Handling ambiguous or nonaligned layouts remains a prototype question.
- **Main area:** one user-selectable intact **rectangle**; maximize and custom taskbar are bounded by it. Other intact pixels remain auxiliary, not a second logical Windows monitor.
- **WindowGuard during drag:** separate active drag clamping from post-creation/restore rescue. Clamp proposed *window bounds* before they overlap dead pixels, not only the cursor hotspot. Prefer movement before resizing. If a window exceeds the largest available intact rectangle or the application disallows resize, cease futile reposition loops and provide a keyboard-usable fallback.
- **Cursor versus window drag conflict:** the pointer may push-through freely while the dragged window must stay intact; decide and test whether to suppress a window transfer or support a separately validated transfer to another fully accommodating area. Never teleport a held window into an invalid placement.
- **SafeDesktopShell:** dock top/bottom/left/right **inside the main rectangle**; default bottom; switch horizontal/vertical layouts. Offer a visible/hidden dedicated-bar preference. Native-bar coexistence presets and reversible OS configuration approaches are proposals, subject to Windows 11 integration tests, not guarantees.

## Next evidence needed

Windows proof of concept to validate push-through delta capture, external portal behavior, emergency disable and shell/window interactions. Unit geometry tests cannot establish OS login-screen support or third-party shell compatibility. Track rollout and remaining questions in [development.md](development.md).
