---
name: deadzone-pointer-routing
description: Use when implementing cursor interception, forbidden-region blocking, cross-monitor movement, emergency disable, or pointer-safety regressions.
---

# Dead-zone pointer routing

**Input:** validated topology snapshot, current/previous cursor position, candidate movement, enabled state.
**Output:** unchanged allowed movement, bounded safe position / transition, or fail-open.

## Procedure

1. Start from an explicit opt-in state. Validate monitor identity and the keyboard disable path before enabling hooks.
2. Implement geometry and crossing policy as a pure function with deterministic test fixtures.
3. Evaluate direct moves to the forbidden area separately from travel toward an external monitor across that area.
4. For intermonitor traversal, preserve the intended external target and choose an allowed landing location; test return paths. Surface any ambiguous gesture/portal requirement.
5. Keep Windows callbacks short and nonblocking. Avoid busy polling, recursive synthetic movements, unbounded event queues and indiscriminate suppression of input.
6. On topology uncertainty, hook exception, process shutdown, or recovery hotkey, return normal OS pointer behavior.

## Verification

- Boundary and corner entry, fast motion, click-drag, negative coordinates, gapped arrangements, multiple external monitors.
- No bounce loop, unbounded CPU, accidental click, external monitor lockout or inaccessible recovery.
- Execute keyboard-only disable and forced-process-exit tests on a real Windows machine.

## Stop conditions

Failed hotkey, lost pointer control, unknown external target, spinning cursor feedback, or a persistent cursor clip: disable feature immediately.

Consult [architecture](../../../docs/architecture.md) for adapter boundaries; this skill does not promise perfect global coverage.
