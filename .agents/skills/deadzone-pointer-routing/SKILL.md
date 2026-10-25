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

## Emergency disable behavior

Use a **global normal-desktop shortcut** to perform immediate emergency disable after one invocation, without a confirmation dialog or long hold. Release the pointer guard and transient cursor restrictions first; stop per-window boundary guards and held rescue movement, invalidate rescue sessions and discard any unconfirmed timed-trial draft. Do not re-enable the same session merely because focus changes or a scheduled confirmation timer fires. Keep shortcut processing idempotent on key repeat; avoid keystroke logging. Provide a documented default chord and permit user reassignment only through a safety-checked change: verify new registration and demonstrate it from the keyboard before discarding the previously working chord. A failed, cancelled or interrupted reassignment preserves the last confirmed binding; if no working shortcut is available, do not enable guarding. **Agreed persistent safety state:** emergency disable is retained across process relaunch, sign-out and Windows reboot. Check the durable per-user disabled latch before starting pointer guards at login. If stored state is invalid or cannot be read confidently, fail open rather than guess that enforcement should resume. Clearing the latch requires a deliberate re-enable action and validation of monitor identity and emergency key; never clear it merely because the app restarts, login autostart runs or a trial confirmation is interrupted. The exact default chord and durable persistence implementation need real Windows validation. Verify registration conflicts and all supported input states on real Windows hardware; no claim is made for secure/UAC desktops.

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
