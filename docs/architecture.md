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

### Rescue exit and baseline recovery

When a user ends a rescue session, calculate the safest attainable position for that target window: preserve an operable titlebar/grab or keyboard-controlled route within intact main space. Full containment remains the normal preference, but an unshrinkable oversized window may need a partially hidden baseline. End and reset only that session, never the other concurrently rescued windows. Keep a visible or keyboard-reachable path to start rescue again. If the app rejects relocation, stop repeating moves and expose an accessible fallback; do not promise every titlebar button can be visible simultaneously. Exact baseline scoring and rejected-move UX remain to be designed and validated on Windows.

### Rescue directional interaction

Each directional button supports a fixed-distance tap and a continuous press-and-hold shift. Step distance and hold speed are independently user configurable. Continuous motion stops when the button is released. Commands remain associated with the selected rescue session; numeric defaults and hold timing require validation.

## Main area, windows and desktop shell

The main work area is the default placement and maximize rectangle. The dedicated taskbar is hosted within it. Ordinary windows may be moved into auxiliary areas by the user; a best-effort rescue should not fight legitimate repositioning.

The OS still knows the full physical display rectangle: modifying a normal window work area or adding an appbar does **not** automatically shrink all Windows shell surfaces or transform the panel into a virtual monitor. The product offers reachable equivalent controls where repositioning system panels is unreliable.

**Hidden dedicated-bar access:** maintain a single compact reveal tab as an independent accessible UI surface whenever the SafeDesktopShell bar is hidden. Constrain its full bounds to the intact *main work rectangle* near the current bar docking edge, not to raw damaged physical-screen edges. A keyboard shortcut must reveal the bar even when the tab cannot be clicked. Revalidate the tab after topology, DPI, rotation and main-rectangle changes; if no usable placement exists, prefer a reachable fallback/fail-open rather than leaving the user with an invisible launcher. The shortcut combination and dimensions still require a separate decision. Treat transiently showing a hidden bar as a UI action, separate from changing its saved visibility setting (the latter still requires 60-second confirmation). The native Windows taskbar remains outside this mechanism.

SafeDesktopShell should focus on:

- App launch/search/pinned/running-app switching.
- File Explorer and Settings access from intact space.
- Audio, networking and battery/clock information plus power controls.
- Narrow-main-area responsive layout.
- **v0.1 non-interference** with the native Windows taskbar (native shell unchanged); dedicated bar coexists alongside it.

**Initial release boundary:** do not hide, move, restart, replace or configure Windows Explorer's native taskbar; no `SHAppBarMessage`-based interference with native taskbar state or Explorer kill/restart for bar coexistence. The dedicated SafeDesktopShell overlay only manages its **own** surfaces inside verified intact pixels. Native Windows taskbar controls may be inaccessible over damaged parts; unsupported tray/notification functions remain documented limitations. Any native bar modification requires a separate future feasibility review and approval.

Third-party notification-area icons, native Start, protected overlays and all possible dialogs are **not** assumed trivially clonable. Test and document missing functions before claiming standalone usefulness.

## Guided first-run setup and detailed editing

Propose a **wizard shell** around existing MonitorTopology and ZoneModel, not a separate virtual-display implementation. Its steps are: (1) select and validate physical target; (2) choose a damaged rectangle on a scaled schematic or with exact local physical-pixel input; (3) accept or override a suggested intact main rectangle; (4) review the preview, keyboard emergency exit and explicit activation. The wizard must remain keyboard navigable even when its initial window lands partially on broken pixels, with an accessible reposition/recovery path to be proven on Windows.

Keep configuration in a **draft, unenforced** state until the explicit enable/try action. Later detailed settings edit the same validated model, maintaining the previous usable profile until a candidate is accepted. The 60-second confirm-or-revert transaction is **agreed** for initial activation and critical later changes, including dedicated taskbar docking/visibility. Preview is not equivalent to having proven cursor interception or system-shell safety on Windows hardware.

### Stable edge choice at rectangle corners

During one blocked-contact gesture, **lock the contacted edge on first contact**. A later contact with the perpendicular adjacent edge at the corner must not change the chosen edge or the accumulated normal-only progress. When two edges are genuinely contacted at the same initial instant, compare the positive inward-normal components of the first attempted motion and choose the larger. If the components tie or contact order cannot be established safely, do **not** invent a priority or transfer: stop and require retreat to healthy space followed by a clear new contact. Reset the lock and accumulated progress on confirmed retreat or completed safe crossing; never oscillate targets between edges on each input event. Exact jitter/debounce tolerance and signal precision remain prototype questions.

### Push-through boundary contact and reset

Track deliberate inward attempted movement only during an active blocked-edge contact. For an already selected axis-aligned rectangle edge, project each attempted input-motion vector onto that edge's **inward unit normal** and accumulate only the positive normal component; tangential motion contributes zero, so diagonal distance is never counted in full. An outward step that actually retreats into allowed space triggers the agreed immediate reset. On the first contact, stop the pointer at the valid boundary; accumulate inward attempted motion until the configured threshold is reached, then perform a safe mapped transfer if a destination exists. **If cursor motion actually retreats into the healthy region, reset the contact's accumulated value to zero immediately.** A later contact, including another edge or monitor route, begins a new gesture. Movement purely along a blocked edge does not itself imply retreat; sensor jitter and simultaneous-contact classification tolerances require prototype verification; **first-contact edge lock with larger inward component on simultaneous contact** and diagonal normal-only projection are agreed. Never use stale accumulated motion to cause a surprise cross after leaving and returning.

## First-run 60-second trial transaction

An approved onboarding draft remains **uncommitted** until the user explicitly tests it and confirms within **60 seconds**. Model the transitions as `DRAFT -> TRIAL_ACTIVE -> CONFIRMED`, and `TRIAL_ACTIVE -> ROLLED_BACK` on timeout/cancel, or `TRIAL_ACTIVE -> FAIL_OPEN` on invalid geometry, lost target or emergency disable. Keep a snapshot of the last *confirmed* validated state; on expiry restore that snapshot, or disable intervention if none exists. The on-screen countdown and confirmation controls must stay in reachable intact space, with keyboard-only operation. Starting the trial is separate from committing the profile and enabling next-sign-in autostart.

Engineering constraints to validate on Windows: use a monotonic deadline, handle suspend/resume without extending an unsafe active trial indefinitely, and ensure that timeout and confirmation cannot both commit. If the process exits unexpectedly, fail open; do not assume a background watchdog survives its process or that unconfirmed settings should restart automatically. The same timed trial **also applies to safety-critical detailed-settings edits**: monitor target identity, dead-zone bounds, main intact rectangle, and pointer guarding/crossing settings. Cosmetic or unrelated convenience settings can commit immediately. The **dedicated** SafeDesktopShell taskbar docking side and show/hide state are also critical and use the same 60-second trial. Its cosmetic appearance is immediate; interaction with the **native Windows taskbar** remains a separate unverified decision.

### Detailed-settings change classification

Before applying a user edit, classify its impact on reachability. For a critical diff, capture the last confirmed configuration and its current topology identity, validate the new geometry in a draft, and require a deliberate 60-second trial and confirmation before persisting or changing the sign-in profile. Batch edits mixing cosmetic and critical changes as one critical transaction; do not partially persist its critical subset. On cancel/expiry restore the previous confirmed profile **only after revalidation against current monitor identity/topology**; otherwise enter a safe disabled/fail-open state. For purely cosmetic settings, including dedicated-bar theme and icon appearance, apply and persist without a countdown. For dedicated-bar docking or visibility, keep the previously confirmed bar configuration and last reachable recovery mechanism available throughout the 60-second trial; rollback it on timeout/cancel, subject to current monitor validation. Recheck that neither route removes keyboard emergency disable. Do not interpret visual preferences as noncritical if they relocate/remove the final accessible control surface.

### Emergency shortcut and fail-open shutdown

A **global shortcut on the normal user desktop** must execute emergency disable on a single activation: no confirmation prompt, hold timer or modal UI dependency. Give release of pointer interception/topology gating highest priority; stop WindowGuard and current rescue-hold movement, invalidate active rescue exception sessions, and cancel any pending configuration trial without committing the draft. The emergency route must not wait for window repositioning or the 60-second timeout. A global emergency stop also sets a **durable per-user emergency-disabled latch**. This latch survives application restarts, sign-out and Windows reboot; it overrides the previously enabled profile and any post-login autostart restoration. Re-enabling requires deliberate user action and revalidation of target topology and escape shortcut before clearing the latch. An interrupted or failed re-enable must leave the latch active.

Use a documented **default emergency chord** for first enable, with a user-configurable replacement. Changing it must be transactional: retain the previously confirmed functional binding while validating that the candidate is non-reserved, registerable and operable via a keyboard-only demonstration; only commit the replacement after a successful test. If a temporary dual-registration strategy is infeasible on supported Windows configurations, fail the candidate transaction and retain the original until an equally safe replacement method is validated. On cancellation, registration/test failure or interruption, roll back to the old binding rather than leaving no escape path. Do not log key content. Investigate minimally intrusive user-level hotkey registration, focus conflicts and keyboard layout variations on Windows. Handle repeat key-down idempotently. Do not claim operation on secure UAC/logon desktops or exclusive fullscreen without evidence. Test that the emergency path can be reached when the dedicated taskbar and rescue panel are hidden, and ensure unexpected process exit releases any transient interception.

### Persistent emergency-disable safety gate

Separate `lastConfirmedProfile` from a persistent `emergencyDisabled` safety latch. At startup, read the latch **before** restoring hooks, window guards or rescue state. If the latch is present or its status cannot be trusted, fail open and optionally show a keyboard-reachable disabled/status page; never silently resume guarding. On an emergency shortcut, immediately release interception, stop active rescue motion and abandon draft trials, then record the persistent latch through a crash-safe atomic settings update. Implement ordering/recovery so an interrupted durable write cannot cause unexpected next-boot guarding: if the marker/profile state is indeterminate, start disabled. Persist the profile for an eventual deliberate re-enable but do not activate it automatically.

A manual re-enable attempt must validate the current monitor topology, enabled profile and registered working emergency shortcut, and respect the existing trial/confirmation safeguards when appropriate; only successful explicit activation clears the emergency latch. If validation, confirmation or persistence fails, remain emergency-disabled. The app may auto-start as an **inactive settings/status UI** but must not auto-enable hooks. Windows integration and crash/reboot simulation are required before shipping.

## Startup, persistence and recovery

```text
UNCONFIGURED --[select/preview + first opt-in]--> ACTIVE
ACTIVE       --[user disable]---------------> DISABLED
ACTIVE       --[topology mismatch/error]----> FAIL_OPEN
DISABLED     --[explicit enable + valid profile]--> ACTIVE
FAIL_OPEN    --[validated recovery + allowed restart]--> ACTIVE
ACTIVE --[sign-out/process exit]--> OS native pointer behavior
EMERGENCY_DISABLED --[explicit valid and confirmed enable]--> ACTIVE
ACTIVE --[emergency shortcut]----------------> EMERGENCY_DISABLED (persisted latch)
next sign-in --[emergencyDisabled or unreadable safety state]--> EMERGENCY_DISABLED / FAIL_OPEN
next sign-in --[not emergency-disabled + saved valid target]--> ACTIVE
next sign-in --[invalid/missing/ambiguous target]---------> FAIL_OPEN
```

Persist settings for the Windows user, not device-wide driver state. Subsequent **sign-in** autostart with the last intentionally enabled profile is required **only while the persisted emergency-disabled latch is clear**; initial install/first run does not silently enable blocking. Resolve how a user who explicitly disabled the feature expects persistence before implementation—never override a deliberate disable.

A keyboard-only emergency disable must work without a visible shell, tray or external monitor. Disabling/exit/crash must leave no persistent `ClipCursor`, injected driver, modified resolution or blocked input. Topology changes must be applied atomically or guarded mode suspended.

## Safety/privacy and operational boundaries

- Windows normal desktop, per-user privileges; drivers, services, elevation, credential providers and secure desktop manipulation are out of Mode A.
- No keystroke text logging, mouse path history, screenshots, telemetry, hidden network dependencies or unconsented privileged actions.
- Shell app and pointer hook failures are separately contained; no open-ended synchronization, hot-loop or recursive warp.
- New GitHub Actions workflows and publishing/signing require explicit maintainer approval under [AGENTS.md](../AGENTS.md).

## Further agreed UX and proposed implementation consequences (2026-10-25)

- **Push threshold and reset:** expose user-facing sensitivity; start usability prototyping at approximately **40 px-equivalent extra inward motion**, but do not assume cursor-position changes during clipping provide that attempted delta. **On actual departure from a blocked boundary into allowed area, immediately clear that contact's accumulated inward attempted movement. A return starts at zero.** Edge-tangential motion without departing is not a new entry. Calibrate actual units, jitter tolerance and corner treatment on real hardware.
- **Windows monitor topology:** preserve OS-configured resolutions, positions, monitor numbering/identity and primary selection. Determine candidate destination from existing relative monitor placement, and redirect across dead pixels only after intentional inward push. Handling ambiguous or nonaligned layouts remains a prototype question.
- **Main area:** one user-selectable intact **rectangle**; maximize and custom taskbar are bounded by it. Other intact pixels remain auxiliary, not a second logical Windows monitor.
- **WindowGuard during drag:** separate active drag clamping from post-creation/restore rescue. Clamp proposed *window bounds* before they overlap dead pixels, not only the cursor hotspot. Prefer movement before resizing. If a window exceeds the largest available intact rectangle or the application disallows resize, cease futile reposition loops and provide a keyboard-usable fallback.
- **Cursor versus window drag conflict:** the pointer may push-through freely while the dragged window must stay intact; decide and test whether to suppress a window transfer or support a separately validated transfer to another fully accommodating area. Never teleport a held window into an invalid placement.
- **SafeDesktopShell:** dock top/bottom/left/right **inside the main rectangle**; default bottom; switch horizontal/vertical layouts. Offer a visible/hidden dedicated-bar preference. Native-bar coexistence presets and reversible OS configuration approaches are proposals, subject to Windows 11 integration tests, not guarantees.

## Next evidence needed

Windows proof of concept to validate push-through delta capture, external portal behavior, emergency disable and shell/window interactions. Unit geometry tests cannot establish OS login-screen support or third-party shell compatibility. Track rollout and remaining questions in [development.md](development.md).
