# Architecture and safety design (proposed)

This is a provisional design. APIs are not considered validated merely because they are named here.

## Platform and components

- **Host:** Windows desktop application, planned C#/.NET with minimal Win32 interop; no kernel driver.
- **Monitor topology:** enumerate display rectangles and stable identities; distinguish selected built-in target from external monitors. Use a DPI-aware process context and the Windows virtual-desktop coordinate system.
- **Pointer routing engine:** pure, testable geometry/transitions separated from actual Windows hooks or pointer-positioning APIs.
- **Platform adapter:** reads current pointer/display events and applies an allowed movement or mapped intermonitor transfer with bounded processing.
- **Window placement adapter:** best-effort adjust normal windows on the affected display without manipulating unrelated displays.
- **Controls:** tray state, selected target/zone, accessible emergency-disable shortcut, diagnostic status without input logs.
- **Persistence:** minimal per-user local settings only. Safe initial state is disabled until user enables and verifies target.

## Why not simply ClipCursor?

A whole-desktop rectangular cursor clamp cannot represent a permitted region made of the laptop's healthy half **plus arbitrary other monitors**. Blocking the left half by globally clipping x would also block some legitimate external displays. Design the allowed region as the union of monitor-specific permitted areas and explicit crossing transitions.

## Coordinate and identity model

Keep two representations distinct:

1. `MonitorDescriptor`: durable enough target identity when available, current bounds, rotation, scaling, work area and connected/disconnected state.
2. `AllowedRegion`: monitor-local normalized zones converted to virtual desktop pixel geometry for a specific topology snapshot.

Never derive internal monitor identity solely from 'primary', enumeration order, left-to-right sorting, `DISPLAY1`, or a fixed coordinate. If the target becomes ambiguous, disable intervention rather than applying it to a different display.

Geometry must cover negative coordinates, non-uniform monitor sizes, adjacency, gaps, coordinate boundaries, and fractional normalization/rounding.

## Pointer routing candidates

A possible Windows adapter is a low-level mouse hook (`WH_MOUSE_LL`) plus carefully controlled positioning; this needs prototype validation. Keep callbacks fast and nonblocking, with no file/network I/O and no allocations in the hot path where practical. Avoid feedback loops from synthetic movement. An event filter must not swallow unrelated input.

Specify and test an intermonitor **portal / edge-transfer** policy. The policy should preserve the destination monitor and transform entry coordinates into a safe landing point, rather than projecting every move onto a single global rectangle. A multi-monitor transfer may need a deliberate shortcut/gesture if automatic routing is ambiguous; explicitly surface that tradeoff.

Do not pretend that merely polling and teleporting a pointer offers a pixel-perfect invariant under all device input paths.

## Window placement

Keep this separate from pointer filtering. The planned adapter observes relevant window state and applies bounded placement/recovery for ordinary windows on the selected monitor. It must not steal focus, move windows on unrelated monitors, endlessly reposition dialogs, or fight fullscreen/privileged apps.

Native maximize behavior and taskbar placement can be constrained by Windows; document any best-effort behavior or required Windows setting instead of claiming true OS-level monitor subdivision.

## Fail-open states

```text
DISABLED --[user opt-in, valid topology]--> ACTIVE
ACTIVE   --[user toggle]---------------> DISABLED
ACTIVE   --[topology mismatch/error]----> FAIL_OPEN
FAIL_OPEN --[verified topology + opt-in]-> ACTIVE
any state --[exit/crash]----------------> OS native pointer behavior
```

The disable key must be testable without a mouse and should not depend on tray UI being visible. On abnormal exit, there must be no durable global clipping state to clean up. Fail-open warnings should not prevent operation of other applications.

## Security / operational boundaries

- Ordinary user rights only; privilege elevation and drivers are outside this plan.
- Do not record key contents, screenshots, mouse path history, or window titles for telemetry.
- No service/API requirement, silent auto-updater, background network requests, or hidden analytics.
- Treat the system's secure desktop, sign-in UI, protected windows, and exclusive games as unsupported unless later measured and scoped.
- Keep public release and binary signing decisions explicit, separate from building or running tests.

## Change discipline

Behavior requirements live in [behavior-contract.md](behavior-contract.md), and implementation milestones in [development.md](development.md). Keep this document focused on internal design; revise only when actual prototype evidence changes the choice.
