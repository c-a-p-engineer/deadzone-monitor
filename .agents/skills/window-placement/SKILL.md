---
name: window-placement
description: Use for rescuing, moving, sizing, maximizing, or exempting windows from the damaged region on the selected display.
---

# Window placement

**Input:** validated target/allowed region; window bounds, placement state and relevant ownership/visibility.
**Output:** minimal safe placement for a normal, supported window or a documented untouched exception.

## Procedure

1. Confirm a window is associated with the selected target monitor; leave windows on all other monitors alone.
2. Determine whether controls or title bar are inaccessible within the surviving region.
3. Calculate a position/size that keeps the supported normal window reachable. Use stable sizing; don't endlessly fight apps restoring their own geometry.
4. Keep focus stable, avoid moving fullscreen, protected, off-desktop, transient or system windows without a proven contract.
5. Recover user-visible windows on relevant restore/maximize events, not an unconditional global polling loop.

## Verification

- New, restored, minimized, maximized and moved windows; dialogs; mixed DPI; taskbar/work area; external-to-internal transfers.
- No focus stealing, layout oscillation, dragging conflicts, or changes to windows on external monitors.
- Clearly report Windows-native maximize limitations and observed unsupported app classes.

## Stop conditions

Unsupported window class/integrity, ambiguous ownership or repeated repositioning: cease intervention for that window and retain keyboard accessibility.

Refer to [the behavior contract](../../../docs/behavior-contract.md) for user-visible goals.
