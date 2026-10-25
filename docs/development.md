# Development plan and verification ledger

**Checkpoint (2026-10-25):** initial repository bootstrap plus a new agreed-requirements pass. **No application implementation, GitHub Actions workflow, Windows integration result or release exists yet.** This plan distinguishes signed-off product behavior from unverified implementation techniques.

The owner of requirements is [behavior-contract.md](behavior-contract.md); do not use this task plan as a conflicting specification.

## Phase 0 — requirements and repository basis

- [x] Public repository, contributor/agent rules, local task skills and MIT license.
- [x] Decide **Mode A first**, Mode B secure sign-in as future exploratory track.
- [x] Decide **arbitrary rectangle** with physical px coordinates from the first version.
- [x] Decide **one main intact work area** plus remaining auxiliary areas.
- [x] Decide **push-through cursor jump** rather than immediate jump.
- [x] Decide standalone usability and dedicated taskbar/launcher over native shell fidelity.
- [x] Decide last-settings restoration after user sign-in and emergency disable.
- [x] Decide **four-edge docking** for the dedicated taskbar (bottom by default) and user-oriented native coexistence/visibility goals.
- [x] Decide **move first, resize only as needed** for normal windows and **boundary-stop while actively dragging**, not snap-back on release.
- [x] Preserve Windows' monitor sizes/relative layout; allow user-adjustable push-through sensitivity with **40 px-equivalent as a tentative starting value**.
- [x] Decide **initial-version oversized-window four-way rescue mode** with per-window scoped exception to ordinary full-containment guard.
- [x] Decide **auto-detect then offer** rescue in healthy area: explicit user action starts it (no silent activation).
- [ ] Resolve unresolved parameters and feasibility questions identified in the behavior contract.

## Phase 1 — safe geometry and pointer proof

- [ ] Pure geometry model for arbitrary rectangle, main/auxiliary regions and external screens.
- [ ] Validated monitor selection and physical-pixel-to-Windows coordinate conversion.
- [ ] Compare proposed 40 px-equivalent sensitivity against real input-unit measurements; verify reset and false-positive behavior.
- [ ] Prototype **push-through** state machine, attempted movement delta capture and configurable sensitivity.
- [ ] Define and test portal mapping (internal healthy regions and external screens) with a reversible route.
- [ ] Prototype keyboard-only fail-open, hook release, process death and recovery.
- [ ] Test edge/center dead zones, different sizes, corners and no-destination boundaries.
- [ ] Verify on a real Windows laptop with/without an external monitor.

**Gate:** usable pointer and emergency keyboard recovery must be confirmed on hardware before background automatic enable or packaging as working software.

## Phase 2 — window management and profile lifecycle

- [ ] Main-area selection (suggestion and user override); auxiliary areas remain usable.
- [ ] Live window-drag guard: stop full window bounds before forbidden overlap; test drag/cursor-portal interactions.
- [ ] **Detect unfit supported windows**, show a keyboard-accessible **Start rescue** action inside healthy/main space, and require explicit user activation; suppress false offers for fit-capable windows, avoid focus theft.
- [ ] **Four-direction rescue mode** for an oversized/minimum-size affected window, scoped to the selected window; implement keyboard-operable controls, reset/exit, avoid ordinary guard tug-of-war and verify no input trap.
- [ ] Test 1600px-minimum-size window in 1200px healthy rectangle, both horizontal and vertical overflow, app refusal to move, external monitor unchanged, and topology changes during rescue.
- [ ] Best-effort spawn, restore, maximize, dialog rescue; never alter external monitor windows.
- [ ] Visual and numeric rectangle configuration, safety preview and rollback.
- [ ] Save last enabled profile with stable target identity; validate on restore.
- [ ] Hot-plug, disconnect/reconnect, DPI, orientation, resolution and sleep/resume safety handling.
- [ ] Windows sign-in **post-login** automatic startup and restoration, after escape gate passes.
- [ ] Test invalid/ambiguous profile restores, intentional disable and emergency recovery.

## Phase 3 — stand-alone Safe Desktop Shell and first release

- [ ] Implement **top, bottom, left, right** docking within main with responsive horizontal/vertical bar layout; default bottom and user-selectable visibility.
- [ ] Dedicated taskbar entirely inside the main work area and responsive for narrow layouts.
- [ ] Launcher/search, running/pinned apps, Explorer, Settings, clock/battery, audio/network access, power controls.
- [ ] Select a reversible method for dealing with the native taskbar; document incompatible tray/notification features.
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

1. Push-through **trial sensitivity** of 40 px-equivalent (final threshold, input units, accumulation/decay/reset), diagonals, and Windows-topology-preserving destination mapping.
2. Main-area best-fit algorithm, too-small dimensions and auxiliary-area behavior.
3. Visual zone editing UI, safe preview/undo and keyboard-only accessibility.
4. Native taskbar reversible hiding/coexistence (candidate Safe-first / both / native-first), effect on external monitors, and third-party tray/notification access.
5. Which everyday shell operations work safely from the dedicated UI across Windows versions.
6. Monitor identity, saved profiles across topology changes, and auto-start mechanics.
7. Hotkey selection, .NET version/packaging and release validation conditions.
8. Rescue offer presentation (banner versus dedicated-taskbar button), duplicate/dismiss policy, arrow movement increments, exit/restore with no fully safe placement, and applicability to uncooperative/system windows. **Detection + explicit start** is decided.

## Rollback

Keep pointer enforcement behind a reversible toggle. Prove disabled mode and process-exit recovery before enabling autostart. A bad release must be stoppable from the keyboard and from the usual Windows application management tools without requiring access to the damaged region.
