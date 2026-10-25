# Behavior contract (planned, not implemented)

This document owns user-observable requirements, safety invariants, and acceptance criteria. It does not claim a working binary exists.

## Primary use case

A Windows laptop's **left half of its built-in screen** is unusable. The user needs the **right half** for work, must not accidentally send the pointer into the damaged area, and must retain full pointer access to multiple external monitors.

The initial profile is a selectable monitor and a normalized disabled rectangle `x=0..0.5, y=0..1` within that monitor. Geometry is configurable; do not hard-code 1920×1080, monitor index, DPI, or topology.

## Behavioral requirements

| ID | Requirement | Acceptance |
| --- | --- | --- |
| DZ-01 | Target only the explicitly selected monitor and disabled rectangle. | External-monitor input is unchanged; a different internal panel can be selected without code changes. |
| DZ-02 | While active on a normal user desktop, prevent a pointer entering the selected forbidden area during supported pointer movement. | Entry attempts at every boundary redirect/stop at an allowed position; cursor never becomes trapped at an unreachable location. |
| DZ-03 | Preserve navigation from healthy laptop area to *each* connected external monitor and back. | Physical paths requiring a crossing of the forbidden strip still have an intentional, reversible transition path. |
| DZ-04 | Keep windows on the laptop display usable in the surviving region. | Ordinary new, restored, and maximized windows can be reached and interacted with without moving important controls into the forbidden area, subject to documented OS/app limitations. |
| DZ-05 | Provide always-accessible disable/recovery. | An accessible keyboard shortcut disables interception and a process exit restores normal pointer behavior without requiring pointer access to the damaged region. |
| DZ-06 | React safely to display arrival/removal, resolution changes, scaling, rotation, and sleep/resume. | Recalculate correctly, or temporarily disable restriction with a visible warning if the target monitor cannot be identified reliably. |
| DZ-07 | Avoid unnecessary privilege and data collection. | No administrator requirement for ordinary use, no input-content logging, remote connection, or telemetry by default. |
| DZ-08 | Make activation deliberate and recoverable. | First launch starts disabled; user enables it after seeing the target display/zone and escape shortcut. Persisted autostart must not preempt this safety baseline. |

## Interaction semantics

- The **forbidden region** is scoped to the target monitor's coordinates, not a global x-value.
- Treat monitor rectangles in Windows virtual-desktop coordinates; external monitors may have negative x/y coordinates, gaps, different DPI, or different resolutions.
- When a direct pointer trajectory toward an external display traverses the forbidden region, route the transition to a usable landing position on the intended display. Avoid bounce loops and accidental clicks during correction.
- If geometry is ambiguous or crossing cannot be resolved reliably, **fail open** or offer a clear disabled state; don't trap the pointer in a rectangle.
- The cursor restriction and window management are independently switchable where practical. Window management is best-effort; never imply all third-party windows can be controlled.
- Full-screen games, secure UAC desktop, sign-in, firmware/boot and protected or higher-integrity app surfaces are outside the guaranteed behavior boundary.

## Safety invariants

1. User can always disable the feature through a keyboard-only recovery mechanism on supported normal desktops.
2. Losing the process or disabling the feature must not leave a persistent OS cursor clip, injected driver, altered display mode, or blocked input.
3. No unintended restriction may apply to the external monitors.
4. An unknown/untrusted topology does not activate an assumed dead zone.
5. No keystroke content or pointer trajectory history is stored or transmitted.

## Acceptance matrix (required before first release)

- Single laptop display: both attempted dead-zone entries and intentional disable.
- Laptop + external monitor to left; laptop + external monitor to right.
- Laptop + two external monitors, including an arrangement that would ordinarily require crossing the left-half strip.
- Target monitor is not primary; negative virtual coordinates and mixed DPI.
- Hot-unplug/replug; resolution/scaling/orientation changes; sleep/resume.
- Rapid cursor movement, dragging, edges/corners, alt-tab, overlays, and keyboard recovery.
- Minimized/restored/maximized ordinary windows and app exceptions.
- Forced process termination or hook failure followed by restored normal pointer motion.

Use synthetic geometry tests for deterministic behavior; **physically test on Windows** for the actual pointer/window controls. Keep observed and unverified outcomes separate.
