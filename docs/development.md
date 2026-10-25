# Development plan and verification ledger

**Checkpoint (2026-10-25):** initial repository bootstrap plus a new agreed-requirements pass. **No application implementation, GitHub Actions workflow, Windows integration result or release exists yet.** This plan distinguishes signed-off product behavior from unverified implementation techniques.

The owner of requirements is [behavior-contract.md](behavior-contract.md); do not use this task plan as a conflicting specification.

## Phase 0 — requirements and repository basis

- [x] Public repository, contributor/agent rules, local task skills and MIT license.
- [x] Decide **Mode A first**, Mode B secure sign-in as future exploratory track.
- [x] Decide **arbitrary rectangle** with physical px coordinates from the first version.
- [x] Decide **one main intact work area** plus remaining auxiliary areas.
- [x] Decide **push-through cursor jump** rather than immediate jump.
- [x] Decide **reset accumulated push-through distance when the pointer retreats from a blocked edge into healthy space**; renewed contact begins at zero (no carryover).
- [x] Decide **diagonal push-through uses only the inward normal component** of attempted movement at the contacted edge, ignoring tangential movement and diagonal total distance.
- [x] Decide **corner contact edge priority**: lock first-contacted edge; simultaneous contact chooses larger inward-normal component; ambiguous tie never jumps.
- [x] Decide standalone usability and dedicated taskbar/launcher over native shell fidelity.
- [x] Decide last-settings restoration after user sign-in and emergency disable.
- [x] Decide **four-edge docking** for the dedicated taskbar (bottom by default) and user-oriented native coexistence/visibility goals.
- [x] Decide **move first, resize only as needed** for normal windows and **boundary-stop while actively dragging**, not snap-back on release.
- [x] Preserve Windows' monitor sizes/relative layout; allow user-adjustable push-through sensitivity with **40 px-equivalent as a tentative starting value**.
- [x] Decide **initial-version oversized-window four-way rescue mode** with per-window scoped exception to ordinary full-containment guard.
- [x] Decide **auto-detect then offer** rescue in healthy area: explicit user action starts it (no silent activation).
- [x] Decide **rescue remains active across application switches** and ordinary minimize/restore; explicit user End or target window Close normally terminates it, safety fail-open can override.
- [x] Decide **concurrent independent per-window rescue sessions**; multiple oversized windows keep separate offsets and ending one does not end another.
- [x] Decide **one shared rescue panel** with automatic target-follow for focused rescue-enabled apps plus a manual target picker.
- [x] Decide **freely movable, expanded/collapsed floating rescue controller** within intact main area; compact reachable handle and keyboard-only reopen.
- [x] Decide **expand on user-initiated rescue start; thereafter manual-only collapse/expand**, never triggered by idle timeout or application focus change.
- [x] Decide rescue directional tap steps and held continuous movement, with configurable step distance and speed.
- [x] Decide safe-baseline positioning on explicit rescue End, with a reachable way to re-enter rescue.
- [x] Decide **guided first-run setup wizard** (physical target -> damaged rectangle -> main work rectangle -> safety review), with **later detailed-settings editing**.
- [x] Decide **60-second confirm-or-rollback trial** for first-run activation; unconfirmed settings are not saved, timeout returns to the prior confirmed state or disabled first-run state.
- [x] Decide **60-second trial for critical detailed-settings changes only** (dead zone, main area, monitor selection and pointer guarding/routing); cosmetic settings apply without trial.
- [x] Decide **dedicated taskbar docking and show/hide changes** also require a 60-second trial; its cosmetic color/icon changes apply immediately.
- [x] Decide **hidden dedicated bar keeps a compact reveal tab inside the healthy main area and a keyboard shortcut** for independent recovery; exact hotkey/tab geometry remain undecided.
- [x] Decide **v0.1 native Windows taskbar stays unchanged**: run dedicated bar alongside it; defer native hide/reposition features to later investigation.
- [x] Decide **immediate, confirmation-free global emergency keyboard shortcut** (single activation, not long hold); release guarding, stop rescue movement and cancel pending trials.
- [x] Decide **default emergency chord plus user customization**, requiring successful registration/keyboard-only test before replacing a working binding; unsuccessful changes revert.
- [x] Decide **emergency disable stays active across app restarts, Windows sign-outs and reboots** until deliberate user re-enable; auto-login restoration must respect the persisted disabled latch.
- [ ] Resolve unresolved parameters and feasibility questions identified in the behavior contract.

## Phase 1 — safe geometry and pointer proof

- [ ] Pure geometry model for arbitrary rectangle, main/auxiliary regions and external screens.
- [ ] Validated monitor selection and physical-pixel-to-Windows coordinate conversion.
- [ ] Compare proposed 40 px-equivalent sensitivity against real input-unit measurements; verify reset and false-positive behavior.
- [ ] Test partial blocked-edge push followed by retreat into allowed space and a new contact: require full fresh push before crossing. Check tangential sliding (not departure), sensor jitter, corners, diagonals, and left/right external-monitor routes without unintended jumps.
- [ ] Verify inward-normal projection for oblique and tangent movement separately at each of four rectangle edges (mouse and touchpad); tangential travel alone never reaches crossing threshold and retreat resets.
- [ ] Test all four rectangle corners: sequential first-contact lock, truly simultaneous unequal inward components, exact/ambiguous tie with no jump, no mid-gesture edge oscillation, and fresh selection after retreat. Test laptop-only plus external monitors.
- [ ] Prototype **push-through** state machine, attempted movement delta capture and configurable sensitivity.
- [ ] Define and test portal mapping (internal healthy regions and external screens) with a reversible route.
- [ ] Prototype keyboard-only fail-open, hook release, process death and recovery.
- [ ] Prototype a normal-desktop **global emergency shortcut** with no confirmation/hold: immediate pointer hook release, window guard shutdown, cancellation of ongoing rescue holds and trial drafts, no same-session automatic re-enable. Test conflicts, repeated keydown and no accessible taskbar/normal mouse.
- [ ] Prototype **safe emergency shortcut reassignment**: documented default, reject reserved/colliding combinations, maintain prior operational binding during candidate test, confirm new chord using keyboard only, and roll back after cancel/failed registration/interrupted test. Validate laptop layouts and no disabled escape route.
- [ ] Implement/test durable per-user `emergencyDisabled` latch and fail-open on unknown/corrupt marker, interrupted write, restart during active trial or interrupted manual re-enable. Never automatically restore hooks while the latch is set.
- [ ] Test edge/center dead zones, different sizes, corners and no-destination boundaries.
- [ ] Verify on a real Windows laptop with/without an external monitor.

**Gate:** usable pointer and emergency keyboard recovery must be confirmed on hardware before background automatic enable or packaging as working software.

## Phase 2 — window management and profile lifecycle

- [ ] Main-area selection (suggestion and user override); auxiliary areas remain usable.
- [ ] Live window-drag guard: stop full window bounds before forbidden overlap; test drag/cursor-portal interactions.
- [ ] **Detect unfit supported windows**, show a keyboard-accessible **Start rescue** action inside healthy/main space, and require explicit user activation; suppress false offers for fit-capable windows, avoid focus theft.
- [ ] **Four-direction rescue mode** with a per-window session registry; support independent offsets, reset/exit and keyboard-operable controls; prevent ordinary guard tug-of-war without disabling protection for unrelated windows.
- [ ] Test 1600px-minimum-size window in 1200px healthy rectangle, both horizontal and vertical overflow, app refusal to move, external monitor unchanged, and topology changes during rescue.
- [ ] Test active rescue with browser/other app foreground switching, target minimize/restore, repeat switching, user End, target Close, and emergency disable. Verify shifted position retention, independent other-window safety and clearing of stale window IDs.
- [ ] **Concurrent rescue validation:** start A and B in independent sessions, give distinct offsets, switch focus repeatedly, end/close one without changing the other; reject stale/reused HWND and wrong-target control actions, then test global fail-open cleanup.
- [ ] **Single shared rescue controller:** select target on rescue-enabled foreground changes; support manual picker of valid sessions, accessible target label, keyboard operations, no accidental retarget on panel/nonrescued focus, safe close/destroy handling, no duplicate panels.
- [ ] **Movable/collapsible floating-panel geometry:** free drag within intact main, expanded/collapsed controls, reachable handle and keyboard reopen, clamping after resolution/topology/main-area changes, no dead-zone overlap or inaccessible off-screen panel.
- [ ] **Manual visibility policy regression:** newly started rescue opens shared panel expanded, manual collapse remains collapsed across idle/focus/minimize events; manual reopen preserves per-window sessions and offsets; keyboard-only reopen works.
- [ ] Implement configurable single-tap step and press-and-hold directional rescue movement; validate settings and release behavior.
- [ ] On explicit rescue End, reposition only the affected oversized window to a reachable safe baseline with an operable titlebar or equivalent keyboard route, then allow rescue to be started again. Test rejected moves and unchanged concurrent sessions.
- [ ] Best-effort spawn, restore, maximize, dialog rescue; never alter external monitor windows.
- [ ] Implement keyboard-accessible first-run **wizard**: monitor choice, scaled dead-zone preview and physical-pixel numeric input, intact main suggestion/override, emergency-escape demonstration and explicit enable.
- [ ] Implement post-setup **detailed settings** that edit the same underlying validated profile and retain a recoverable previous configuration.
- [ ] Test first-time wizard with broken-panel layout, optional external monitor, entirely keyboard-driven completion/cancellation and later detailed edits; do not enable guarding before explicit consent.
- [ ] Implement an **initial 60-second uncommitted trial** with intact-region confirmation/countdown and keyboard access; on timeout or cancel restore last confirmed valid state, or disable if no prior state; commit only on explicit confirmation.
- [ ] Classify detailed-settings changes by reachability risk and reuse the 60-second transaction for critical changes; save cosmetic-only changes immediately. Test mixed edits, rollback validation against current monitor identity, emergency disable and non-persistence of unconfirmed drafts.
- [ ] Test 59s confirmation versus 60s timeout race, user cancellation, no prior profile, invalid monitor/topology during test, sleep/resume, emergency hotkey and abrupt process termination; verify no unconfirmed startup persistence.

- [ ] Save last enabled profile with stable target identity; validate on restore.
- [ ] Hot-plug, disconnect/reconnect, DPI, orientation, resolution and sleep/resume safety handling.
- [ ] Windows sign-in **post-login** automatic startup and restoration, after escape gate passes.
- [ ] Test reboot/sign-out/app-relaunch after emergency stop: startup UI may appear, but cursor guard, rescue offsets and window restrictions remain off. Deliberate enable revalidates monitor and emergency key, and only a successfully confirmed activation clears the disabled latch.
- [ ] Test invalid/ambiguous profile restores, intentional disable and emergency recovery.

## Phase 3 — stand-alone Safe Desktop Shell and first release

- [ ] Implement **top, bottom, left, right** docking within main with responsive horizontal/vertical bar layout; default bottom and user-selectable visibility.
- [ ] For **dedicated bar** docking and visibility changes, show an accessible 60-second confirm-or-rollback trial. Keep a keyboard recovery path even when testing hidden state; appearance-only edits do not require the countdown.
- [ ] Validate cancel/expiry returns to confirmed docking/visibility, mixed critical/cosmetic edits use the critical gate, and a missing/moved monitor fails open rather than restoring invalid geometry.
- [ ] Dedicated taskbar entirely inside the main work area and responsive for narrow layouts.
- [ ] When dedicated bar is hidden, keep a visible healthy-area reveal tab; implement an independent keyboard shortcut. Confirm no invisible or lost entry point after main-area/topology changes, including during a 60-second visibility trial.
- [ ] Launcher/search, running/pinned apps, Explorer, Settings, clock/battery, audio/network access, power controls.
- [ ] **Verify non-interference:** do not programmatically hide/reposition/restart or reconfigure native taskbar/Explorer; test coexistence with dedicated bar on laptop and external monitors, and document incompatible tray/notification features.
- [ ] Laptop-only end-to-end acceptance, including basic daily controls without any external display.
- [ ] Pin Windows/.NET SDK and build/package instructions; create CI workflow **only after maintainer approval**.
- [ ] Sign-in startup validation, crash/fail-open regression and actual Windows desktop manual testing.
- [ ] Document limitations, unsigned binary/SmartScreen and release evidence; publish only after separate authorization.

## Phase B — deferred research, not part of Mode A release gate

- [ ] Assess options for usable **Windows sign-in/password/PIN** interaction when the screen is damaged.
- [ ] Compare legitimate OS accessibility or physical-display workarounds before considering system credential-provider or other invasive integrations.
- [ ] Record Windows secure-desktop limitations, permission/reliability risks and separate explicit approvals.
- [ ] Do not claim Mode B works or modify system authentication behavior during Mode A delivery.

## Acceptance evidence to collect

| Scenario | Automated geometry evidence | Real Windows confirmation |
| --- | --- | --- |
| Arbitrary center/edge rectangle and main/aux choice | Required | Required |
| Push-through stops first, then intentional jump | Required | Required |
| Reverse transfer, external-left/right, two external monitors | Required | Required |
| Standalone application/window/taskbar/system controls | Supporting | Required |
| Saved settings after next sign-in; invalid target fail-open | Supporting | Required |
| Emergency hotkey, hook failure, forced process exit | Supporting | Required |

For every future PR: list changed requirement IDs, actual build/test commands, OS/runner, results, gaps and fallback instructions. `passed CI` does not equal `verified on Windows hardware`.

## Open product/engineering decisions (not yet approved)

1. Push-through **trial sensitivity** of 40 px-equivalent (final threshold and input units), contact-order/jitter tolerance, and Windows-topology-preserving destination mapping. **Normal-only accumulation, retreat reset and first-contact corner priority are decided.**
2. Main-area best-fit algorithm, too-small dimensions and auxiliary-area behavior.
3. Wizard on-screen placement/recovery for users with preexisting damaged displays, plus **keyboard recovery and on-screen confirmation when the dedicated taskbar is hidden during its agreed 60-second trial**. **Guided steps, detailed-settings editing and 60-second rollback for first-run and critical post-setup edits are agreed.**
4. **Native taskbar hide/reposition is deferred beyond v0.1**; v0.1 keeps it untouched. Confirm coexistence and document unavailable tray/notification functions. Optional advanced native integration needs a separate feasibility investigation.
5. Which everyday shell operations work safely from the dedicated UI across Windows versions.
6. Monitor identity, saved profiles across topology changes, and auto-start mechanics.
7. Exact **default** keyboard chord for the agreed immediate emergency disable, robust replacement/verification implementation, hidden dedicated-bar reveal shortcut, durable disabled-state write/recovery strategy, reveal-tab size/placement, .NET version/packaging and release validation conditions. **Default plus user-changeable shortcut, safe rollback, and emergency-disabled persistence until explicit re-enable are agreed.**
8. Rescue offer presentation (banner versus dedicated-taskbar button), duplicate/dismiss policy, arrow movement increments, safe-baseline positioning algorithm and fallback for refused moves, and applicability to uncooperative/system windows. **Detection + explicit start and focus-switch persistence** are decided.
9. Rescue controller's **initial position**, placement persistence and behavior after all rescue sessions end, plus nuanced manual override versus subsequent automatic focus-follow. **One movable/collapsible shared floating panel, expanded on explicit rescue start and manually collapsed/expanded thereafter, with auto-follow and manual picker is agreed.**

## Rollback

Keep pointer enforcement behind a reversible toggle. Prove disabled mode and process-exit recovery before enabling autostart. A bad release must be stoppable from the keyboard and from the usual Windows application management tools without requiring access to the damaged region.
