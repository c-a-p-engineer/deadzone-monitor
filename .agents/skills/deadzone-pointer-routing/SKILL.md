---
name: deadzone-pointer-routing
description: Use for cursor interception, push-through crossing, excluded-region avoidance, external-monitor traversal, fail-open and pointer regressions.
---

# Dead-zone pointer routing

Read [the behavior contract](../../../docs/behavior-contract.md) for the agreed **push-through (not immediate)** cursor experience. Read [architecture](../../../docs/architecture.md) for implementation boundaries. This skill owns procedures, not the authoritative requirements.

**Input:** validated monitor topology, target-local physical-pixel excluded rectangle, user main/aux layout, current and proposed movement, state of crossing gesture.

**Output:** permit movement, stop at valid edge, deliberate portal jump into a reachable permitted region, or fail open.

## Procedure

1. Before any interception, require validated target identity, keyboard-only emergency disable and a safe initial/restore activation policy.
2. Separate a pure geometry/portal state machine from Win32 event acquisition; never equate clamped cursor position with repeated attempted movement.
3. On first entry attempt, **stop** at the allowed boundary. While blocked at one rectangle edge, accumulate only the **positive inward perpendicular component** of attempted input movement; exclude tangential distance and full diagonal length. Emit a jump only after the configured threshold, with a validated destination. At corners, lock the first contacted edge. For simultaneous first contact, select the edge with larger positive inward normal motion; an equal or uncertain tie must not transfer. Maintain edge lock until retreat or safe crossing.
4. On retreat into healthy space, reset accumulated blocked-edge push to zero; later contact starts a fresh gesture. Motion tangential to a still-contacted edge is not retreat. Keep threshold, jitter handling, diagonal/corner behavior and destination mapping configurable/testable; precise numeric values are open.
5. After the deliberate push threshold, uniquely validate the **first viable forward destination** (internal target display or external monitor) using current Windows topology. Preserve **Y** on horizontal crossing and **X** on vertical crossing when legal. For a uniquely verified offset external monitor, clamp an out-of-range orthogonal coordinate to its **nearest intact point on that same monitor**. Never use nearest-position correction to guess a monitor, cross an unsafe gap, or resolve an ambiguous route; in those cases remain blocked. Avoid bounce and preserve reverse traversal.
6. Keep callbacks bounded and fast, avoid synthetic-event feedback, avoid spin/polling loops and avoid intercepting unrelated input.
7. On ambiguous topology, input adapter failure, invalid settings, explicit hotkey or exit, release interception and return OS-native pointer behavior.

## Emergency disable behavior

Use a **global normal-desktop shortcut** to perform immediate emergency disable after one invocation, without a confirmation dialog or long hold. Release the pointer guard and transient cursor restrictions first; stop per-window boundary guards and held rescue movement, invalidate rescue sessions and discard any unconfirmed timed-trial draft. Do not re-enable the same session merely because focus changes or a scheduled confirmation timer fires. Keep shortcut processing idempotent on key repeat; avoid keystroke logging. Provide a documented default chord and permit user reassignment only through a safety-checked change: verify new registration and demonstrate it from the keyboard before discarding the previously working chord. A failed, cancelled or interrupted reassignment preserves the last confirmed binding; if no working shortcut is available, do not enable guarding. **Agreed persistent safety state:** emergency disable is retained across process relaunch, sign-out and Windows reboot. Check the durable per-user disabled latch before starting pointer guards at login. If stored state is invalid or cannot be read confidently, fail open rather than guess that enforcement should resume. Clearing the latch requires a deliberate re-enable action and validation of monitor identity and emergency key; never clear it merely because the app restarts, login autostart runs or a trial confirmation is interrupted. The exact default chord and durable persistence implementation need real Windows validation. Verify registration conflicts and all supported input states on real Windows hardware; no claim is made for secure/UAC desktops.

## Current interaction choices

- User-adjustable push sensitivity, with a **provisional 40 px-equivalent** threshold for initial testing; precise units and jitter tolerance require evidence. Reset when retreating from the blocked edge is agreed.
- Derive portal candidates from the **existing Windows monitor geometry**. Do not reconfigure screen resolution/position to implement cursor crossing.
- When a window is being dragged, coordinate with WindowGuard: preserve the *whole window* in intact space, even if the cursor can intentionally push through. Do not auto-transfer a held window into forbidden pixels.

## Verification

- Edge and center forbidden rectangles; unequal sizes, negative origins, mixed DPI, gap and orientation.
- The first boundary touch does **not** jump; continued inward motion does, provided a valid destination exists.
- Test oblique and purely tangential swipes at all four rectangle edges. Only inward normal travel counts; a tangential-only swipe never triggers transfer. Check this with mouse and touchpad inputs.
- Test all four corners: first-touch lock, simultaneous unequal inward normal movement, ambiguous ties staying blocked, and reset/edge reselection only after retreat or safe crossing. Include multi-monitor routes.
- Reverse traversal; partial push, retreat and return to a boundary with zero carried progress; tangential sliding, oscillation, diagonal/corners, high speed and drag.
- External monitor left/right, above/below, multiple external displays and laptop-alone. Validate straight-line landing, unique-target nearest Y/X correction on nonaligned screens, no jump when monitor selection/gap is unsafe or ambiguous, stable reverse travel and anti-bounce.
- No accidental click, bounce loop, inability to disable, unbounded CPU or persistent cursor clipping.
- Actual Windows manual testing of the attempted movement signal and forced-process-exit behavior.

## Stop conditions

Unreachable emergency shortcut, invalid mapping, repeated unintended teleport, lost pointer, feedback loop, unexpected global clip or lost monitor identity: disable guarding and fail open.
