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

## Verification

- Dead rectangle on edge and in center, with main chosen from either side; auxiliary usable.
- Ordinary created/restored/minimized/maximized windows, dialogs, narrow main region, DPI/taskbar interactions.
- Native and dedicated taskbar coexistence, no layout oscillation, untouched external windows.
- Validate actual Windows behavior; simulated geometry alone does not prove system-shell relocation.

## Stop conditions

Invalid main area, uncertain window integrity/ownership, placement loops or inaccessible controls: stop the intervention for affected window and preserve a recovery path.
