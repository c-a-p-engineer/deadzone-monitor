---
name: window-placement
description: Use for choosing the primary intact work area, preserving auxiliary regions, window rescue/maximize, or safe-taskbar placement.
---

# Window placement

Read [the behavior contract](../../../docs/behavior-contract.md) for main versus auxiliary region semantics and [architecture](../../../docs/architecture.md) for OS limitations.

**Input:** validated target bounds, dead-zone rectangle, selected main rectangle, optional auxiliary-usage intent, window placement/visibility.

**Output:** safe proposed main rectangle and window placement, best-effort repair, or a documented untouched exception.

## Procedure

1. Confirm the main rectangle is wholly intact. Offer a reasonable candidate but preserve explicit user selection.
2. Place supported new/restored/maximized normal windows relative to main; do not assume `MONITORINFO.rcWork` can represent a hole in a physical display.
3. Keep auxiliary intact pixels available for manually moved windows. Avoid forcibly snapping every auxiliary window back into main.
4. Rescue inaccessible title bars and dialogs without focus theft, endless repositioning or changes to windows on unrelated external screens.
5. Keep dedicated taskbar inside main area; it must not overlap the dead rectangle. Do not assume all native system panels can be moved.
6. Preserve fallback keyboard/window operations for unsupported and protected windows.

## Agreed drag and docking decisions

- During a user drag, enforce the whole window's valid bounds **before** any dead-zone overlap; do **not** wait for mouse release to snap the window back. Do not move already valid auxiliary windows unnecessarily.
- Move first, then reduce size if needed; on unshrinkable or oversized windows enter a **separate per-window rescue mode** rather than oscillating. Enable four-direction repositioning with an always-reachable keyboard-operable control panel, and prevent the ordinary clamp from immediately undoing an intentional rescue shift.
- Dedicated taskbar docks **top/bottom/left/right inside the user-selected main rectangle** (default bottom), with horizontal/vertical layout and user-visible show/hide.
- Treat native taskbar hiding/coexistence as a separately tested reversible mechanism; never assume all monitors permit independent native-taskbar control.

## Oversized rescue procedure

1. Confirm the supported window cannot fit within a useful intact rectangle despite permitted move/resize. Do not offer rescue for an unrelated external-window or an app that already fits.
2. **Offer, do not auto-start:** render an accessible Start rescue action wholly in healthy/main space; keep the normal guard until the user explicitly activates the rescue for that window. Do not steal focus.
3. Capture the original placement safely and mark **only that window** as the current rescue exception. Do not stop pointer guarding or protections for other windows.
4. Offer controls to move the window **up/down/left/right** and a reset/exit action entirely inside intact space; movement reveals previously hidden content sequentially.
5. Stop if the target closes, changes privilege, refuses moves, changes display or repeated attempts fail. Do not turn attempted rescue into a tight reposition loop.
6. On exit, restore a feasible and usable placement or surface a keyboard-accessible fallback if full containment remains impossible; clear the exception state.

## Verification

- Dead rectangle on edge and in center, with main chosen from either side; auxiliary usable.
- Ordinary created/restored/minimized/maximized windows, dialogs, narrow main region, DPI/taskbar interactions.
- Native and dedicated taskbar coexistence, no layout oscillation, untouched external windows.
- Validate actual Windows behavior; simulated geometry alone does not prove system-shell relocation.

## Stop conditions

Invalid main area, uncertain window integrity/ownership, placement loops or inaccessible controls: stop the intervention for affected window and preserve a recovery path.
