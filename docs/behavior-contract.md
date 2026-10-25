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
| Dedicated taskbar hidden-state access | **Agreed: reachable reveal tab + keyboard shortcut (A)** | When the dedicated bar is hidden, leave a compact clickable tab wholly on a healthy edge of the main area; activate it to reveal the bar. A keyboard shortcut must also reveal the bar without needing the tab. Never rely on the damaged physical edge. Exact shortcut and tab positioning remain to be selected. |
| Dedicated taskbar placement/visibility trial | **Agreed: risk-gated 60 seconds (A)** | Changing the **dedicated** SafeDesktopShell bar docking side (top/bottom/left/right) or its displayed/hidden state uses the 60-second confirm-or-revert trial. Cosmetic appearance changes save immediately. This does not establish a method of hiding the native Windows taskbar. |
| First-run trial confirmation | **Agreed: 60-second confirm-or-revert (A)** | After explicit trial activation, allow **60 seconds** to confirm; otherwise automatically restore the prior valid profile or disable guarding if this is the first setup. An emergency keyboard-only escape remains available immediately. |
| Detailed-settings risk gate | **Agreed: 60-second trial for safety-critical changes only (A)** | Later edits to the dead-zone rectangle, main intact rectangle, selected monitor identity, and pointer guarding/routing settings require the same explicit **60-second confirm-or-revert** trial. Non-safety appearance and ordinary control preferences are applied/saved without a 60-second countdown. |
| First-run configuration | **Agreed: guided wizard (A)** | Step through physical monitor selection, dead-zone rectangle, user-selected main intact rectangle, and preview/safety review before enabling; advanced settings remain editable later. |
| Rectangle selection UI | **Agreed direction** | Visual preview/drag selection and editable physical-pixel coordinates; exact controls remain design work. |
| Number of zones | **Initial design boundary** | One rectangular dead zone per selected monitor profile initially; multiple regions are a future extension, not yet agreed for initial delivery. |
| Workspaces | **Agreed** | Exactly one user-selected **main work area**, with remaining intact areas available as **auxiliary areas**. |
| Pointer crossing | **Agreed: push-through (B)** | A pointer stops at the boundary first; further deliberate inward movement jumps to reachable healthy space. Sensitivity is user-adjustable; 40 px-equivalent is a *provisional* default for testing, not a measured optimum. |
| External screens | **Agreed** | Use Windows' existing monitor layout, native physical sizes and relative positions to choose crossing destinations without changing OS display configuration. Reach external screens and return even when dead pixels intervene. |
| Windows | **Agreed** | Keep supported windows wholly inside intact space when possible: move first, resize if needed, and use the main rectangle for maximize. Do not disturb valid auxiliary placements. |
| Taskbar | **Agreed** | Dedicated familiar Windows-style taskbar fitted completely inside the main rectangle, dockable on its top, bottom, left or right edge (default bottom), with suitable horizontal/vertical layout. |
| Windows native taskbar coexistence | **Agreed: leave native taskbar unchanged in v0.1 (A)** | Run the dedicated SafeDesktopShell bar **alongside** the Windows-native taskbar. Do not programmatically hide, relocate, replace, terminate, or change native Explorer taskbar settings. Native-taskbar hiding is a separate future feasibility track, not a first-release requirement. |
| Window dragging | **Agreed: stop at boundary (A)** | During a drag, prevent the supported window rectangle from crossing into the dead zone; do not permit temporary hidden overlap and wait for mouse release. Minimum-size exceptions require safe fallback. |
| Oversized-window rescue | **Agreed: rescue mode (A), initial version** | When an ordinary application's minimum dimensions exceed intact space, provide dedicated **four-direction panning/position shifting** to reveal the hidden portions from a healthy display area; never trap keyboard or pointer input. |
| Rescue activation | **Agreed: detect then offer (A)** | Automatically detect supported windows that cannot fit; show an **accessible start-rescue action entirely inside intact/main space**. **Only enter rescue after explicit user activation**, never immediately upon detection. |
| Rescue duration across app switching | **Agreed: retain until explicitly ended (A)** | Keep each rescued window's active session and last shifted position through app switching and ordinary minimize/restore. Explicit ending or that window's closure ends its session; failures may fail safe. |
| Concurrent rescue windows | **Agreed: independent per-window sessions (A), initial version** | Two or more oversized windows may simultaneously retain distinct rescue mode, offsets and status. Switching between them must not reset or silently end either session. Ending or closing one does not clear another. |
| Rescue control panel | **Agreed: single shared panel (A), initial version** | Show **one** rescue controller, not one panel per window. The selected target automatically follows focus when a rescue-enabled window becomes active; users can also select a rescue target manually from a list. Each session keeps its independent offset/state. |
| Rescue panel placement/visibility | **Agreed: draggable, collapsible floating panel (A), initial version** | User can move the single floating rescue panel freely **within the intact main rectangle** and toggle expanded/collapsed presentation. A small reachable handle/control remains when collapsed; neither form may move into the dead zone. Keyboard access/recovery remains available. |
| Rescue exit positioning | **Agreed: safe baseline (A)** | On explicit End rescue, return the affected window to a reachable baseline favoring titlebar and essential controls, even if full containment is impossible. Keep a keyboard-accessible path to reopen rescue; preserve other windows' rescue state. |
| Rescue direction buttons | **Agreed (A)** | Tap an arrow for one configurable step; press and hold for continuous movement at a configurable speed. Stop on release and support keyboard use. |
| Rescue panel expand/collapse policy | **Agreed: manual after rescue start (A)** | On explicit **Start rescue**, show the shared controller **expanded**. Thereafter only user action collapses or expands it; app focus changes and inactivity never automatically collapse or reopen it. The compact handle remains accessible. |
| Everyday shell interaction | **Agreed** | A dedicated launch/control UI may replace day-to-day access to native Start/notifications/controls. It need not clone the whole Windows shell. |
| Standalone laptop | **Agreed** | Common daily actions must be reachable without an external monitor. |
| Auto start/restore | **Agreed** | After initial opt-in, subsequent Windows sign-ins should auto-start and restore the prior enabled profile. |
| Safe escape | **Agreed: immediate global keyboard shortcut (A)** | In the supported Windows user desktop, a global shortcut immediately disables DeadZone Monitor's guarding **without any confirmation or long hold**. Release pointer/window intervention, cancel active rescue movement and unconfirmed 60-second trials; ordinary Windows input resumes. Exact key combination and post-disable reactivation policy require later decisions and real-device validation. |
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
| DZ-30 | Coexist with the native Windows taskbar without modifying it in the initial release. | Enabling, disabling, showing, hiding or docking the dedicated taskbar leaves native taskbar visibility, placement, Explorer process and OS configuration untouched. On external monitors, native Windows taskbar behavior is unchanged. Native taskbar may remain partly inaccessible behind dead pixels; dedicated reachable controls provide a supported alternative. |
| DZ-29 | **Keep dedicated bar recoverable when hidden** by a compact reveal tab plus keyboard shortcut. | After hide is confirmed, a visible clickable tab remains wholly in healthy main space and reveals the dedicated bar. A keyboard-only shortcut also reveals it. The tab stays reachable after main-area changes or topology shifts, or a safe fallback is shown. Hiding the dedicated bar never hides the separate trial confirmation/emergency-exit controls. Native Windows taskbar is not modified by this requirement. |
| DZ-28 | Gate **dedicated taskbar docking and visibility** edits behind a 60-second confirmation; allow cosmetic edits immediately. | Trial a move among top/bottom/left/right edges or display/hide for the dedicated bar, showing confirmation and keyboard recovery in intact space. Cancel/timeout restores the last confirmed usable configuration if topology-valid, otherwise fails open; an unconfirmed hide is not persisted at sign-in. Changing only color/icons does not start a countdown. Native Windows taskbar behavior remains a separate feasibility matter. |
| DZ-27 | Require **60-second temporary trial for safety-critical detailed-settings edits**, without burdening cosmetic edits. | Editing any dead-zone bounds, main rectangle, target monitor or pointer-guard/crossing behavior stages a draft and prompts an accessible confirmation after explicit trial start; expiry/cancel restores the previous confirmed valid profile or disables protection if that profile no longer matches current topology. Appearance-only changes can save without countdown. An unconfirmed trial must never become the sign-in profile. |
| DZ-26 | Protect initial enable with a **60-second temporary trial** and automatic rollback. | Starting trial requires explicit action, shows a confirmation UI entirely in verified reachable space, and does not commit the draft profile. An explicit confirmation before expiry commits it; timeout/cancel reverts to the previous valid enabled profile or disabled normal behavior on first installation. A keyboard-only emergency disable bypasses the countdown. |
| DZ-25 | Provide a **guided first-run setup wizard** and editable detailed settings after setup. | The user can complete target-monitor selection, visual or numeric dead-zone entry, suggested/overridden main rectangle, and preview/safety review in sequence before deliberate activation. Keyboard operation and a return/edit route remain accessible on a damaged display; after setup, detailed settings can modify and validate these values without reinstalling. |
| DZ-01 | Select a physical target display and define a rectangular dead zone anywhere on it. | Coordinates are editable in physical px, visual preview matches them, and a center rectangle as well as an edge rectangle are accepted. |
| DZ-02 | Select one intact main work rectangle, leaving all other healthy pixels usable as auxiliary space. | No main rectangle overlaps the forbidden area; selection can be changed without editing code; auxiliary areas remain pointer-reachable. |
| DZ-03 | Prevent supported normal-desktop pointer movement into the dead zone while guarding is enabled. | Boundary and high-speed entry tests do not leave the pointer in the forbidden region under the supported input paths. |
| DZ-04 | Use **push-through** crossing between reachable intact areas, not immediate jump-on-contact. | At a boundary, first stop; continued movement deliberately transfers to a valid target. No reachable target means stop (no spurious teleport). Reverse travel works. |
| DZ-05 | Preserve normal access to every connected external monitor. | Left/right external screens, two external screens, negative coordinates, and dead-zone crossings allow deliberate outward and return movement. |
| DZ-06 | Prefer main-area placement for ordinary new/restore/maximize windows, and rescue inaccessible windows. | Ordinary supported windows remain operable on the intact screen; auxiliary placement by user is preserved where possible; external-display windows are not repositioned. |
| DZ-14 | Keep the full bounds of a supported window out of the dead zone **during manual dragging**. | While the mouse button remains down, a dragged window stops at the last valid bounds before an overlap; no late snap-back, jitter, or repeated fights with the application. If OS/app restrictions prevent full containment, preserve recovery and show an exception. |
| DZ-16 | Provide **initial-version four-direction rescue mode** for supported windows that cannot fit in the selected healthy rectangle. | If a minimum-size or otherwise unshrinkable window cannot be fully contained, user can move its visible subsection into healthy space via directional controls or keyboard, return/reset/exit safely, and avoid interference from the normal auto-correction loop. |
| DZ-17 | **Detect then offer** rescue without surprise window shifts. | For a supported oversized window, detect its inability to fit, display a keyboard-accessible **Start rescue** action in the main healthy region, and leave rescue inactive until that action is invoked. No forced focus theft or notification/action outside the visible region. |
| DZ-18 | **Retain active rescue across application focus switches** until the user ends rescue or its target closes. | Switch to an unrelated app, work there, then return: original rescued window remains in rescue at the same shifted position without needing to restart rescue. Ordinary minimize/restore does not implicitly finish rescue. Explicit end clears the exception. Closing the target clears it. Failure/topology safety overrides are documented. |
| DZ-19 | **Manage simultaneous oversized-window rescue independently per window**. | Start rescue for windows A and B, shift each in different directions, switch between them: both retain their own offsets and modes. Ending/closing A preserves B; ending/closing B preserves A. The control UI always identifies its intended target and never moves an unrelated window. |
| DZ-20 | Show **one shared rescue panel** supporting focus-follow and explicit target selection. | With rescued A and B, focusing A chooses A and focusing B chooses B; manual target picker can select either. The panel indicates its chosen window and moves only that window. Focus on a nonrescued app or the panel itself does not silently close rescue sessions or cause ambiguous/wrong-target movement. No overlapping duplicate control panels. |
| DZ-21 | Offer a **freely movable, collapsible floating rescue panel** whose controls remain recoverable. | The one panel can be dragged anywhere its full bounds fit inside the intact main rectangle, expanded or collapsed to a small reachable control, without obscuring/dropping into the configured dead zone. Panel repositions into intact space on topology/geometry changes, and keyboard-only access restores expanded controls. |
| DZ-24 | End rescue by returning the affected window to an operable safe baseline; permit rescue again. | After End, a movable titlebar region or equivalent keyboard controls remain reachable to interact with the oversized app. A re-enter-rescue action is accessible from intact space; ending one rescue does not alter other active rescues. Impossible full containment is not falsely promised. |
| DZ-23 | Single tap shifts the selected rescue window by a configurable step; press-and-hold moves it continuously at a configurable rate. | Four directions and configuration options are available. Motion ends when the directional button is released. |
| DZ-22 | **Expand on explicit rescue start; collapse/expand manually thereafter.** | After a user activates Start rescue the single shared panel is expanded. Manual collapse leaves a compact visible/keyboard-reachable control; idle time, application switching, and foreground focus changes do not auto-collapse or re-expand it. Manual expand works without restarting or resetting any per-window rescue session. |
| DZ-07 | Fit a dedicated taskbar/launcher into the **main** intact rectangle. | It docks to any of the main rectangle's **four edges**, default bottom, supports horizontal/vertical layouts and never spans excluded pixels; it launches/switches apps and offers essential controls. |
| DZ-15 | Offer familiar and accessible dedicated-taskbar presentation and show/hide **while preserving Windows' native taskbar unchanged in v0.1**. | Dedicated-bar visibility can change without losing access; Windows' existing taskbar is not programmatically hidden, moved, restarted or reconfigured. Native taskbar control is an optional future research track. |
| DZ-08 | Support everyday laptop-only usage through dedicated UI, even if Windows-native panels cannot be repositioned. | With all external screens unplugged, user can start/switch apps, open files/settings, adjust volume/network, see clock/battery, and reach power controls from intact space, where OS capabilities permit. |
| DZ-09 | Persist target identity, zone, main rectangle, crossing preferences, and UI settings locally. | Sign-out/reboot and the next sign-in restore the last **explicitly enabled and validated** profile without requiring repeated manual setup. |
| DZ-10 | Auto-launch **after** Windows sign-in. First-run activation requires user review. | First setup shows safe preview/hotkey; subsequent successful sign-ins restore an enabled validated profile. Do not activate on unknown monitors or invalid data. |
| DZ-11 | Provide reliable emergency disable and fail-open behavior. | Keyboard-only disable works without tray/mouse; stop/crash/topology mismatch removes interception and avoids persistent OS cursor clipping or a stranded cursor. |
| DZ-31 | Provide **immediate, confirmation-free emergency disable** through a global keyboard shortcut. | From any ordinary app on the supported Windows desktop, invoke the shortcut once (no hold or confirmation): pointer interception and window guards stop immediately, ongoing rescue shifts stop, active rescue exceptions are invalidated and unconfirmed 60-second trials are cancelled **without committing draft settings**. OS-native input remains available. A second accidental keypress cannot silently re-enable guarding. Unsupported secure/UAC desktops are not claimed. |
| DZ-12 | Remain safe across topology and resolution/DPI/orientation changes. | Recalculate when safe or suspend restriction with an accessible warning; do not apply an old rectangle to a misidentified monitor. |
| DZ-13 | Operate privately and without exceptional system permissions. | No admin/driver/service required for Mode A's intended normal-desktop path; no telemetry, input-content recording, screenshot capture, or network requirement by default. |

## Oversized-window rescue mode — agreed initial requirement

**Normal-mode invariant:** Keep a supported window fully inside an intact rectangle wherever feasible. Prefer (1) translate without resizing, then (2) resize only as necessary; preserve deliberate valid auxiliary placements.

**Exception:** If an ordinary window's minimum size exceeds every useful intact placement, support a dedicated **four-direction rescue/panning mode** in the initial version. This does **not** scale the third-party app's content or turn the monitor into a virtual display. It temporarily **repositions the oversized whole window**, so different parts are visible within the same healthy viewport in succession. Some of the window may lie outside the healthy viewport **during this explicitly scoped rescue exception**; it is impossible to show the whole oversized app simultaneously without app-specific reflow or scaling. Pointer exclusion remains active.

Agreed baseline behavior:
- **Detect/offer/enter:** when a supported window is detected as too large for normal placement, surface an accessible **Start rescue** control wholly within the healthy/main rectangle. Do not automatically enable rescue or shift the window merely because it was detected. An explicit user action starts the exception.
- User can shift view toward **left / right / up / down** and return to a safe baseline using an entirely accessible control panel; keyboard-only access and exit are required.
- **Independent concurrent sessions:** multiple affected windows can enter rescue independently. Each retains its own shifted position and explicit active/inactive state. The guard exception is scoped to **each individually user-activated target**; it never disables all-window protection or alters unaffected external-monitor windows. Closing/ending one rescue does not end others.
- **One shared rescue panel:** keep a single healthy-region, keyboard-operable controller for all active rescue sessions. When a window with an active rescue session gains foreground focus, automatically select it as the controller target. Also provide a **manual target picker** listing live rescue-enabled windows; identify the selected target clearly before applying movement. Merely focusing an unrelated application or interacting with the controller must not unintentionally redirect a movement command or end another session. The exact sticky-selection / focus precedence is an implementation detail to confirm with usability tests.
- **Draggable and collapsible floating UI:** user can move this single controller freely **within the selected main intact rectangle**, independently of rescued windows. **On explicit rescue start, show the shared panel expanded**. Subsequently, collapse and expand are **user-initiated only**; never trigger them automatically for inactivity, focus switches or minimized/returned windows. Expanded mode provides directional movement, target selection and exit; collapsed mode leaves a compact visible, keyboard-reachable handle to reopen it. Manual expand must not reset any active per-window rescue offset. Prevent either mode from crossing forbidden pixels or being stranded beyond the valid main area. Reposition/recover it after geometry changes without changing other app rescue offsets. Floating panel is a separate UI layer, not a second rescue session.
- For **each rescued window**, suspend its own normal drag/placement clamp so the guard does not undo intentional shifts. **Retain each independent offset and state across focus changes and ordinary minimize/restore; never terminate other sessions on application switching or when one session ends.** Resume ordinary safeguards individually after rescue exit and restore that window's last feasible position where possible.
- **Explicit End behavior:** move only the ended session's window to the best feasible safe baseline. Prefer keeping an operable titlebar/grab region and a keyboard-accessible way to move, close or re-enter rescue. If full containment is impossible, partial visibility is an acknowledged exception; never assert that all titlebar buttons remain visible. Provide a reachable re-enter-rescue action without requiring access to a broken pixel. Do not alter other active rescue sessions. If the target resists repositioning, stop repeated attempts and offer a safe fallback.
- End rescue on explicit user action or when the target window closes. Safety-critical events (invalid monitor identity/topology, lost window ownership/handle, unexpected process exit or emergency global disable) must invalidate or safely suspend the session rather than preserve a dangerous stale pointer to a window. This exception to normal end conditions is required for safety.
- Do not continuously force moves on uncooperative windows, steal focus, warp the pointer into the forbidden region, or break emergency-disable.
- Accessibility and a recoverable failure state take precedence when an app's integrity, window type, or positioning constraints prevent movement.

**Still open:** form of the safe-region suggestion UI (nonmodal banner, dedicated-taskbar affordance, or another always-reachable action), handling repeated detections and users dismissing the suggestion; step sizes and acceleration; exact safe-baseline position and fallback for windows that reject repositioning; interaction with mouse drag, full-screen surfaces and transient dialogs; **initial floating-panel position, position persistence, panel visibility when all rescue sessions end and precise precedence of manual selection versus later automatic focus-follow**. **Start expanded, then manual-only collapse/expand is agreed; no idle/focus-triggered automatic collapse.** Movable/collapsible single shared floating panel, focus-follow on a rescued window, manual target selection, independent sessions, explicit start and retention through focus switches are agreed.

## Dedicated taskbar and shell access — feature boundary

Initial completed Mode A should cover **functional access**, not a pixel-perfect reimplementation of Windows Explorer:

- Application launcher/search and pinned/recent or running app switching.
- Open File Explorer and Windows Settings in reachable positions.
- Clock/date and relevant laptop indicators, including battery.
- Audio/network controls using supported Windows mechanisms or safe shortcuts to native settings.
- Shutdown/restart/sleep or other suitable power controls with confirmation where needed.
- A compact layout if the selected main work area is narrow.

**Not promised by this agreement:** full third-party system tray mirroring, unrestricted access to every vendor panel, perfect native Start/notification center positioning, or exact replication of all Windows notification actions. Investigate access alternatives and document any remaining coverage gaps before calling Mode A complete.

**Hidden dedicated-bar access (agreed):** When the dedicated bar is hidden, a small visible/clickable **reveal tab** stays within the healthy main rectangle, ideally next to the selected docking edge, and a **keyboard shortcut** provides a second independent reveal route. This does not require moving the pointer over a broken screen edge. Keep the tab reachable after geometry changes, with keyboard escape if no safe space remains. The exact shortcut, tab dimensions/position, and whether a temporary reveal auto-hides again remain open. Temporary opening of a hidden bar is distinct from committing a change to its stored visibility preference, which remains subject to the agreed 60-second trial.

**Taskbar presentation:** The dedicated bar is dockable on **top / bottom / left / right** of the chosen *main work rectangle*, not the full physical panel, and defaults to bottom. Users want a Windows-native-like appearance with improved usability in narrow healthy areas. User-selectable visibility is required. **v0.1 coexistence is fixed:** run the dedicated taskbar alongside the existing Windows taskbar, leaving the native taskbar and Explorer configuration untouched. A native bar may still be partly under dead pixels; normal Windows behavior is not guaranteed there. Earlier Safe-first/native-first native-hiding presets are **deferred research ideas**, not initial-release features. Never disturb native taskbars on external monitors.

Native taskbar hiding/repositioning must not require corrupting shell state; an imperfect but reversible fallback is preferable to a fragile Explorer hack.

## Guided onboarding — initial release agreement

On **first run**, use a guided wizard rather than exposing all settings at once:

1. **Select target display:** identify the intended physical monitor from the currently connected displays. Do not assume primary or leftmost.
2. **Define damaged rectangle:** offer a scaled, non-destructive representation and numeric X/Y/width/height entry in local physical pixels; allow review and correction without interacting with broken pixels directly.
3. **Select main intact rectangle:** offer a best-fit candidate and allow explicit override; keep other intact pixels as auxiliary space.
4. **Review and safety preview:** show the chosen display/areas and emergency keyboard escape; require deliberate opt-in before activation. No silent activation while moving between wizard steps.

After onboarding, provide a **separate detailed-settings view** to revisit and edit the target selection, damaged rectangle, main intact rectangle, push-through settings and dedicated shell preferences. Revalidate changes and preserve a keyboard-operable cancellation/recovery path. **Initial enable trial (agreed):** after the user explicitly starts testing a validated draft, show a confirmation interface fully inside the intact main area and give **60 seconds** to choose Confirm. Confirmation saves/enables the new profile. If the user cancels or does not confirm within 60 seconds, restore the last confirmed valid active configuration, or disable guarding when no prior confirmed configuration exists. Never persist an unconfirmed draft as enabled for the next sign-in. The keyboard emergency stop acts immediately rather than waiting for the timeout. Trial timing must remain safe after suspension, failure, or monitor change. **Later detailed-settings changes that affect target monitor selection, dead zone, main intact area, or pointer restriction/crossing use the same 60-second trial. Cosmetic and other non-safety preferences do not.**

## Detailed-settings safety classification — agreed

**Trial-required edits:** selected physical monitor; dead-zone location/dimensions; main intact work-area bounds; pointer guarding enabled/disabled behavior, push-through/crossing behavior and its sensitivity; **dedicated SafeDesktopShell taskbar docking edge or shown/hidden state**. Treat edits as a validated draft rather than updating the last-confirmed startup profile until an explicit 60-second confirmation. On cancel/expiry, revalidate and restore the prior confirmed profile; if monitor identity/geometry is no longer trustworthy, suspend guarding instead of blindly restoring old coordinates.

**Immediate edits:** dedicated taskbar colors, icon appearance and other cosmetic appearance/theme or display preferences and non-safety convenience controls that cannot relocate the effective allowed space or remove the emergency exit. These need no trial countdown. A change that combines safe and unsafe fields uses the trial gate for the entire draft.

**Classification principle:** any setting whose application could make the pointer, windows, settings or escape controls inaccessible is safety-critical even if it is later exposed through a different UI category. **Dedicated bar** docking/visibility is explicitly trial-gated. Any change to **native Windows taskbar** behavior remains an unverified, separate integration decision; do not silently classify a native-shell alteration as safe.

## Emergency shortcut — agreed initial behavior

In the supported **ordinary Windows desktop**, a **global keyboard shortcut** must immediately perform **emergency disable**, with **no confirmation dialog, delay or long-press gesture**. It must work without access to the dedicated taskbar, rescue panel, mouse, or external display. The shutdown sequence prioritizes releasing input interception and any cursor restrictions; stop continuous rescue motion and window-management enforcement; cancel any pending 60-second configuration trial **without saving the draft**. Emergency disable takes priority over pending confirmation/timeout and must never itself trigger automatic restoration of restrictions. Do not require a third-party application to be focused, and do not hook keystroke content beyond the minimal registered chord. The shortcut is a user-level normal-desktop capability, not a guarantee inside secure login/UAC or exclusive-fullscreen surfaces.

**Still open:** the actual collision-free keyboard chord and whether emergency disable persists as a disabled preference across the next Windows sign-in versus only the current session. Recovery/re-enable must be an intentional user action, not a second invocation of the same emergency shortcut.

## Lifecycle and safety

1. **First run:** show target/rectangle/main-area preview; make emergency keyboard disable discoverable and testable; require explicit first enable. Initial activation enters a **60-second uncommitted trial**; only confirmation persists enabled settings, while cancel/expiry restores the previously confirmed state or disables guarding.
2. **Later sign-ins:** run as the same user and automatically restore the last enabled, validated configuration after desktop initialization.
3. **Monitor missing/ambiguous, invalid zone, failure:** suspend restrictions and show accessible failure status. Retain the saved profile for later recovery, but do not apply it to a guessed device.
4. **User disable, exit, forced termination:** normal OS pointer movement returns; no persistent global clip or driver modification survives.
5. **Re-enable:** require valid geometry and an operational keyboard escape path before enabling interception.

The emergency action is agreed as an immediate no-confirmation keyboard shortcut; its exact conflict-free combination, disabled-state persistence across restart, and recovery from unexpected shutdown require explicit prototype tests.

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
- Check auto-detection of an unfit window; **the Start rescue action appears inside the healthy/main region and remains usable from the keyboard**, no premature activation or focus steal, and initiating rescue requires a user action. Confirm no false suggestion for a window that fits after a valid reposition or resize.
- Start rescue on an oversized window, shift it, switch to a browser, minimize/restore it, then return: rescue remains active with the same shifted position. Explicit End and target Close each clear the per-window exception; topology failure and emergency global disable still recover safely.
- **Concurrent sessions test:** rescue A and B independently and shift them to different offsets; foreground switching preserves both, an End/Close for A does not affect B, and selected rescue commands never apply to a wrong or stale window. All active sessions clean up on global emergency disable without input trapping.
- **Single rescue controller test:** when focused rescued target changes A → B → A, the one shared panel reflects each target correctly; manual picker can select either without moving the wrong window; focus on the controller or a nonrescued app does not clear any session. Closing a selected target safely reselects or disables controls without acting on stale IDs.
- **Floating-panel test:** drag the expanded/collapsed rescue controller to each edge of a narrow main rectangle; its full bounds stay intact and keyboard navigation can expand/collapse it. Test positioning after rotation, external-display removal, main-area resizing and corrupted saved location. No duplicate control panels, lost handle or impossible-to-reopen collapse state.
- **Manual panel visibility test:** explicit Start rescue opens the panel expanded; manually collapse it, wait idle and switch among rescued/nonrescued apps and back, and verify it **stays collapsed** until manually expanded. Manually expand while rescues A and B remain active; neither offset nor selected live window is reset. A newly explicit Start rescue opens the shared panel expanded.
- Verify the dedicated taskbar fits inside main for **all four docking directions** and stays navigable in a narrow work rectangle.
- Confirm push-through uses Windows' existing monitor arrangement (including negative virtual coordinates) without changing resolution, primary assignment or screen positions.
- Test the proposed **40 px-equivalent** sensitivity against alternative user-selectable settings; units and default may be revised from observed data.
- Verify changing dedicated/native taskbar visibility is reversible and does not damage Windows Explorer or unexpectedly change taskbar behavior on other displays.

- Wizard acceptance: simulate a partly unusable built-in screen with and without an external monitor. Verify keyboard-only completion/cancel, correct monitor and physical-pixel rectangle preview, user override of main area, no interception before opt-in, and successful later detailed-settings edits.

## Open decisions (not silently treated as agreed)

- Final calibrated push-through distance/units (trial default 40 px-equivalent), movement accumulation/reset, corner handling, and unambiguous destination mapping that respects the existing Windows layout.
- Precise main-area automatic suggestion and restrictions for very small surviving areas.
- Whether a center hole's tiny surviving strips are suitable auxiliary areas or should be optional.
- Native taskbar modification/hiding is **out of v0.1** and may be researched in a separately approved later phase; third-party tray/notification fallback remains a scope and feasibility question.
- Wizard preview presentation and keyboard-only initial positioning on a damaged screen; **native Windows taskbar coexistence/hiding feasibility, tab placement and concrete keyboard shortcut for revealing the hidden dedicated bar**; chosen hotkey, startup implementation, profile matching and monitor identity fidelity.
- Which difficult system dialogs/overlays require alternate access rather than relocation.
