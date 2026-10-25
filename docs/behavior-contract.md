# Behavior contract — agreed requirements, not yet implemented

**Agreement checkpoint:** 2026-10-25. This is the authoritative product behavior contract. An agreement here is not an implementation or a verified Windows capability.

## Product goal and delivery priority

**Mode A — first priority:** after Windows sign-in, restore the last enabled configuration so that a laptop with a damaged display can be used for ordinary day-to-day Windows operations **using only intact regions**, even without an external monitor. External monitors, when present, remain accessible.

**Mode B — future exploration:** extend the experience to the Windows sign-in/password or PIN screen itself. This is a separate, lower-priority research goal; ordinary user-space software does **not** control the secure Windows logon desktop. Do not imply Mode B is feasible with Mode A's architecture.

The initial user scenario is a broken left portion of a built-in laptop panel, but the product must not assume a left-half-only problem. For example, a 1920×1080 display with the left 620 physical pixels damaged is just one valid configuration.

## Agreed product decisions

| Decision | Status | Meaning |
| --- | --- | --- |
| Arbitrary rectangular dead zone | **Agreed, initial version** | A user chooses any X/Y/width/height rectangle on a selected display; not limited to screen halves or edges. |
| Rectangle selection UI | **Agreed direction** | Visual preview/drag selection and editable physical-pixel coordinates; exact controls remain design work. |
| Number of zones | **Initial design boundary** | One rectangular dead zone per selected monitor profile initially; multiple regions are a future extension, not yet agreed for initial delivery. |
| Workspaces | **Agreed** | Exactly one user-selected **main work area**, with remaining intact areas available as **auxiliary areas**. |
| Pointer crossing | **Agreed: push-through (B)** | A pointer stops at the boundary first; further deliberate inward movement jumps to reachable healthy space. Sensitivity is user-adjustable; 40 px-equivalent is a *provisional* default for testing, not a measured optimum. |
| External screens | **Agreed** | Use Windows' existing monitor layout, native physical sizes and relative positions to choose crossing destinations without changing OS display configuration. Reach external screens and return even when dead pixels intervene. |
| Windows | **Agreed** | Keep supported windows wholly inside intact space when possible: move first, resize if needed, and use the main rectangle for maximize. Do not disturb valid auxiliary placements. |
| Taskbar | **Agreed** | Dedicated familiar Windows-style taskbar fitted completely inside the main rectangle, dockable on its top, bottom, left or right edge (default bottom), with suitable horizontal/vertical layout. |
| Window dragging | **Agreed: stop at boundary (A)** | During a drag, prevent the supported window rectangle from crossing into the dead zone; do not permit temporary hidden overlap and wait for mouse release. Minimum-size exceptions require safe fallback. |
| Oversized-window rescue | **Agreed: rescue mode (A), initial version** | When an ordinary application's minimum dimensions exceed intact space, provide dedicated **four-direction panning/position shifting** to reveal the hidden portions from a healthy display area; never trap keyboard or pointer input. |
| Everyday shell interaction | **Agreed** | A dedicated launch/control UI may replace day-to-day access to native Start/notifications/controls. It need not clone the whole Windows shell. |
| Standalone laptop | **Agreed** | Common daily actions must be reachable without an external monitor. |
| Auto start/restore | **Agreed** | After initial opt-in, subsequent Windows sign-ins should auto-start and restore the prior enabled profile. |
| Safe escape | **Required safety invariant** | A keyboard-only emergency disable and fail-open behavior prevent being trapped. |
| Pre-sign-in UI | **Deferred Mode B** | Track as future investigation, never block Mode A on it. |

## Terms and geometry

- **Target display:** the explicitly selected physical monitor. No restrictions apply to a different monitor unless separately and deliberately configured later.
- **Dead zone:** a positive-area axis-aligned rectangle in target-display-local **physical pixels**, specified by `x, y, width, height`; do not treat physical pixels as the same as DPI-scaled UI units. Validate bounds against the current orientation/resolution.
- **Allowed area:** the selected display bounds minus the dead zone, plus entirely accessible external displays.
- **Main work area:** one user-chosen axis-aligned rectangle wholly within an intact part of the target display. The UI may offer a best-fit suggestion; the user can choose another valid rectangle. It hosts maximize and the dedicated taskbar.
- **Auxiliary areas:** any other intact display portions outside the main work area. They remain pointer-reachable and can host manually placed windows. These are **not** independent Windows monitors and need not be separate, nonoverlapping 'virtual monitor' objects.
- **Push-through:** explicit continued inward movement at the excluded boundary. Users can tune sensitivity. A 40 px-equivalent threshold is a **trial default**, not a validated physical mouse-distance measurement; input units, reset/decay rules and destination mapping are **still open**.
- **Monitor layout:** retain actual Windows monitor identifiers, resolution, relative layout and physical geometry. Jump mapping uses the existing topology; it does not reconfigure the displays.

## Behavioral requirements and acceptance

| ID | Required behavior | Verifiable acceptance |
| --- | --- | --- |
| DZ-01 | Select a physical target display and define a rectangular dead zone anywhere on it. | Coordinates are editable in physical px, visual preview matches them, and a center rectangle as well as an edge rectangle are accepted. |
| DZ-02 | Select one intact main work rectangle, leaving all other healthy pixels usable as auxiliary space. | No main rectangle overlaps the forbidden area; selection can be changed without editing code; auxiliary areas remain pointer-reachable. |
| DZ-03 | Prevent supported normal-desktop pointer movement into the dead zone while guarding is enabled. | Boundary and high-speed entry tests do not leave the pointer in the forbidden region under the supported input paths. |
| DZ-04 | Use **push-through** crossing between reachable intact areas, not immediate jump-on-contact. | At a boundary, first stop; continued movement deliberately transfers to a valid target. No reachable target means stop (no spurious teleport). Reverse travel works. |
| DZ-05 | Preserve normal access to every connected external monitor. | Left/right external screens, two external screens, negative coordinates, and dead-zone crossings allow deliberate outward and return movement. |
| DZ-06 | Prefer main-area placement for ordinary new/restore/maximize windows, and rescue inaccessible windows. | Ordinary supported windows remain operable on the intact screen; auxiliary placement by user is preserved where possible; external-display windows are not repositioned. |
| DZ-14 | Keep the full bounds of a supported window out of the dead zone **during manual dragging**. | While the mouse button remains down, a dragged window stops at the last valid bounds before an overlap; no late snap-back, jitter, or repeated fights with the application. If OS/app restrictions prevent full containment, preserve recovery and show an exception. |
| DZ-16 | Provide **initial-version four-direction rescue mode** for supported windows that cannot fit in the selected healthy rectangle. | If a minimum-size or otherwise unshrinkable window cannot be fully contained, user can move its visible subsection into healthy space via directional controls or keyboard, return/reset/exit safely, and avoid interference from the normal auto-correction loop. |
| DZ-07 | Fit a dedicated taskbar/launcher into the **main** intact rectangle. | It docks to any of the main rectangle's **four edges**, default bottom, supports horizontal/vertical layouts and never spans excluded pixels; it launches/switches apps and offers essential controls. |
| DZ-15 | Offer familiar and accessible taskbar presentation, with dedicated-bar show/hide and reversible native coexistence options. | User can select a dedicated-bar visibility preference without losing a way to operate Windows; native taskbar behavior is documented honestly and no per-monitor hide feature is claimed without Windows testing. |
| DZ-08 | Support everyday laptop-only usage through dedicated UI, even if Windows-native panels cannot be repositioned. | With all external screens unplugged, user can start/switch apps, open files/settings, adjust volume/network, see clock/battery, and reach power controls from intact space, where OS capabilities permit. |
| DZ-09 | Persist target identity, zone, main rectangle, crossing preferences, and UI settings locally. | Sign-out/reboot and the next sign-in restore the last **explicitly enabled and validated** profile without requiring repeated manual setup. |
| DZ-10 | Auto-launch **after** Windows sign-in. First-run activation requires user review. | First setup shows safe preview/hotkey; subsequent successful sign-ins restore an enabled validated profile. Do not activate on unknown monitors or invalid data. |
| DZ-11 | Provide reliable emergency disable and fail-open behavior. | Keyboard-only disable works without tray/mouse; stop/crash/topology mismatch removes interception and avoids persistent OS cursor clipping or a stranded cursor. |
| DZ-12 | Remain safe across topology and resolution/DPI/orientation changes. | Recalculate when safe or suspend restriction with an accessible warning; do not apply an old rectangle to a misidentified monitor. |
| DZ-13 | Operate privately and without exceptional system permissions. | No admin/driver/service required for Mode A's intended normal-desktop path; no telemetry, input-content recording, screenshot capture, or network requirement by default. |

## Oversized-window rescue mode — agreed initial requirement

**Normal-mode invariant:** Keep a supported window fully inside an intact rectangle wherever feasible. Prefer (1) translate without resizing, then (2) resize only as necessary; preserve deliberate valid auxiliary placements.

**Exception:** If an ordinary window's minimum size exceeds every useful intact placement, support a dedicated **four-direction rescue/panning mode** in the initial version. This does **not** scale the third-party app's content or turn the monitor into a virtual display. It temporarily **repositions the oversized whole window**, so different parts are visible within the same healthy viewport in succession. Some of the window may lie outside the healthy viewport **during this explicitly scoped rescue exception**; it is impossible to show the whole oversized app simultaneously without app-specific reflow or scaling. Pointer exclusion remains active.

Agreed baseline behavior:
- User can shift view toward **left / right / up / down** and return to a safe baseline using an entirely accessible control panel; keyboard-only access and exit are required.
- The exception applies to **one selected affected window**, never silently disables monitoring of all other windows or moves an unaffected external-monitor window.
- While rescue is active for that window, suspend the normal drag/placement clamp for that window **only** so the window manager does not immediately undo intentional rescue shifts. Resume ordinary safeguards after exit and restore the last valid/safe position when possible.
- Do not continuously force moves on uncooperative windows, steal focus, warp the pointer into the forbidden region, or break emergency-disable.
- Accessibility and a recoverable failure state take precedence when an app's integrity, window type, or positioning constraints prevent movement.

**Still open:** automatic prompt versus manual entry to rescue; placement of rescue controls; step sizes and acceleration; restore-on-exit behavior if no fully valid placement exists; interaction with mouse drag, full-screen surfaces and transient dialogs. Do not silently regard one solution as approved.

## Dedicated taskbar and shell access — feature boundary

Initial completed Mode A should cover **functional access**, not a pixel-perfect reimplementation of Windows Explorer:

- Application launcher/search and pinned/recent or running app switching.
- Open File Explorer and Windows Settings in reachable positions.
- Clock/date and relevant laptop indicators, including battery.
- Audio/network controls using supported Windows mechanisms or safe shortcuts to native settings.
- Shutdown/restart/sleep or other suitable power controls with confirmation where needed.
- A compact layout if the selected main work area is narrow.

**Not promised by this agreement:** full third-party system tray mirroring, unrestricted access to every vendor panel, perfect native Start/notification center positioning, or exact replication of all Windows notification actions. Investigate access alternatives and document any remaining coverage gaps before calling Mode A complete.

**Taskbar presentation:** The dedicated bar is dockable on **top / bottom / left / right** of the chosen *main work rectangle*, not the full physical panel, and defaults to bottom. Users want a Windows-native-like appearance with improved usability in narrow healthy areas. User-selectable visibility is required. **Safe-first, show-both and native-first** are *proposed coexistence presets*, not proven native Windows per-monitor hide mechanisms. Explore safe reversible options; a native bar on another monitor must not be broken by this tool.

Native taskbar hiding/repositioning must not require corrupting shell state; an imperfect but reversible fallback is preferable to a fragile Explorer hack.

## Lifecycle and safety

1. **First run:** show target/rectangle/main-area preview; make emergency keyboard disable discoverable and testable; require explicit first enable.
2. **Later sign-ins:** run as the same user and automatically restore the last enabled, validated configuration after desktop initialization.
3. **Monitor missing/ambiguous, invalid zone, failure:** suspend restrictions and show accessible failure status. Retain the saved profile for later recovery, but do not apply it to a guessed device.
4. **User disable, exit, forced termination:** normal OS pointer movement returns; no persistent global clip or driver modification survives.
5. **Re-enable:** require valid geometry and an operational keyboard escape path before enabling interception.

A reliable emergency hotkey combination, conflicting shortcuts, safe reactivation strategy and interaction with unexpected shutdown require explicit prototype tests.

## Out-of-scope for initial Mode A

- Controlling the Windows **sign-in/password/PIN** display (Mode B research).
- Secure UAC desktops, firmware/boot screens and protected or elevated windows.
- Perfect handling of exclusive-fullscreen games/video and all system overlays.
- Emulating real OS multi-monitor subdivision; applying a custom global monitor layout driver.
- Multiple dead-zone rectangles per profile (future option).
- A full Explorer shell replacement or guaranteed fidelity for every system tray icon.

## Release acceptance scenarios

1. Laptop alone, dead rectangle on **left edge**; numeric width other than 50%.
2. Laptop alone, dead rectangle **in the middle** with main plus auxiliary regions.
3. Laptop + an external monitor on the left, on the right, and laptop + two external screens; confirm push-through jump and reverse.
4. First approach to dead region stops; continued push triggers transfer; moving away, corners, different input speeds and no-target boundaries behave safely.
5. Main/auxiliary selection, ordinary create/restore/maximize/dialog paths; dedicated taskbar stays inside main rectangle.
6. Run everyday actions **without any external screen** using dedicated controls.
7. Fresh setup, saved profile across sign-out/restart, invalid profile, monitor removal/reconnect, resolution/scale/rotation changes.
8. Hotkey-only disable and killed-process restoration; no persistent capture/clipping or input lockout.

Deterministic geometry tests alone are insufficient: pointer hooks, Windows shell integration, lifecycle and user safety **require real Windows desktop/manual tests**. The implementation/release status must remain distinct from these agreed outcomes.

## Additional acceptance checks from the 2026-10-25 discussion

- Check live dragging against a dead rectangle along each edge and a rectangle in the middle, including dragging windows near every corner; a supported window must not overlap dead pixels even before the mouse button is released.
- Check oversized/minimum-size windows and **four-direction rescue/panning controls**, safe keyboard-only reset/exit, scoped exception to ordinary window clamping, and no interference with normal or external-monitor windows; do not claim impossible simultaneous full containment.
- Verify the dedicated taskbar fits inside main for **all four docking directions** and stays navigable in a narrow work rectangle.
- Confirm push-through uses Windows' existing monitor arrangement (including negative virtual coordinates) without changing resolution, primary assignment or screen positions.
- Test the proposed **40 px-equivalent** sensitivity against alternative user-selectable settings; units and default may be revised from observed data.
- Verify changing dedicated/native taskbar visibility is reversible and does not damage Windows Explorer or unexpectedly change taskbar behavior on other displays.

## Open decisions (not silently treated as agreed)

- Final calibrated push-through distance/units (trial default 40 px-equivalent), movement accumulation/reset, corner handling, and unambiguous destination mapping that respects the existing Windows layout.
- Precise main-area automatic suggestion and restrictions for very small surviving areas.
- Whether a center hole's tiny surviving strips are suitable auxiliary areas or should be optional.
- Reversible Windows-native taskbar coexistence/hiding strategy (including whether proposed Safe-first / both / native-first presets are feasible), plus third-party tray/notification fallback.
- Settings UI details, chosen hotkey, startup implementation, profile matching and monitor identity fidelity.
- Which difficult system dialogs/overlays require alternate access rather than relocation.
