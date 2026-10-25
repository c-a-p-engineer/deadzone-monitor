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
3. On first entry attempt, **stop** at the allowed boundary. Continue accumulating *intentional inward attempted movement* while blocked; only after the configurable threshold is crossed may a jump be emitted.
4. Keep threshold, gesture reset/decay, diagonal/corner interpretation, button-drag behavior and destination coordinate mapping configurable/testable. Their exact values are **open decisions**, not hard-coded requirements.
5. Resolve a reachable region behind the excluded rectangle (internal healthy area or external monitor); preserve a workable reverse path. If there is no destination, stop without teleporting.
6. Keep callbacks bounded and fast, avoid synthetic-event feedback, avoid spin/polling loops and avoid intercepting unrelated input.
7. On ambiguous topology, input adapter failure, invalid settings, explicit hotkey or exit, release interception and return OS-native pointer behavior.

## Current interaction choices

- User-adjustable push sensitivity, with a **provisional 40 px-equivalent** threshold for initial testing; exact input units and reset policy require evidence, not guesses.
- Derive portal candidates from the **existing Windows monitor geometry**. Do not reconfigure screen resolution/position to implement cursor crossing.
- When a window is being dragged, coordinate with WindowGuard: preserve the *whole window* in intact space, even if the cursor can intentionally push through. Do not auto-transfer a held window into forbidden pixels.

## Verification

- Edge and center forbidden rectangles; unequal sizes, negative origins, mixed DPI, gap and orientation.
- The first boundary touch does **not** jump; continued inward motion does, provided a valid destination exists.
- Reverse traversal, moving away and pushing again, oscillation, diagonal/corners, high speed and drag.
- External monitor to left/right, two external monitors, and laptop-alone operation.
- No accidental click, bounce loop, inability to disable, unbounded CPU or persistent cursor clipping.
- Actual Windows manual testing of the attempted movement signal and forced-process-exit behavior.

## Stop conditions

Unreachable emergency shortcut, invalid mapping, repeated unintended teleport, lost pointer, feedback loop, unexpected global clip or lost monitor identity: disable guarding and fail open.
