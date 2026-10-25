---
name: monitor-topology
description: Use when discovering monitors, identifying the damaged built-in panel, converting DPI/coordinates, or reacting to topology changes.
---

# Monitor topology

**Input:** current Windows display snapshot, saved target identity, configured normalized dead zone.
**Output:** verified `MonitorDescriptor` set, selected target, computed allowed/forbidden regions or a fail-open result.

## Procedure

1. Query current monitor bounds and work areas through Windows APIs; record DPI awareness and relevant scaling conversion at the boundary.
2. Identify the built-in monitor from stable available identifiers or user selection. Do not assume primary/index/leftmost/zero-origin.
3. Convert normalized local coordinates to virtual desktop rectangles only for the current topology snapshot. Expose each monitor's verified identity/bounds and forward-route validity to the routing engine. **After one external target is uniquely selected**, let routing preserve the orthogonal coordinate where valid, or clamp Y on horizontal and X on vertical crossing to the nearest intact point **on that target only**. Never infer a different monitor across virtual-desktop gaps.
4. Validate target existence, unique identity, valid bounds and zone area. If ambiguous, return `unresolved` and disable intervention.
5. Recompute after attach/detach, resolution, rotation, scaling and resume events; make updates atomic from the routing engine's perspective.

## Wizard integration

For initial onboarding, provide an explicitly selected physical target monitor, correctly scaled local-pixel schematic and exact coordinate editing without modifying the OS monitor arrangement. Revalidate display identity, orientation and bounds between the wizard's monitor selection, area preview and explicit enable. Keep the configuration in draft mode until consent, and provide keyboard-only monitor selection and recovery when the damaged panel cannot display the whole wizard. For main-area suggestion, expose fully intact axis-aligned candidate rectangles derived from the currently selected target and dead-zone geometry. The wizard should recommend a large rectangle only when usable width/height support the essential taskbar and controls, not based on raw area alone; preserve a keyboard-operable validated manual override. Numerical UI minima and behavior when every candidate is too small are open for design and testing. The later detailed-settings view must use the same validated geometry model. **First-run activation has an agreed 60-second trial:** explicit trial start, confirmation before the deadline commits the draft, and timeout/cancel returns to the last confirmed profile if still valid or disables guarding if none is available. Confirmation and countdown must remain in reachable space with keyboard access; global emergency disable is immediate. Safety-critical later detailed-settings edits (monitor target, dead-zone bounds, main intact rectangle and pointer guard/crossing behavior) **also require a 60-second confirm-or-revert trial**; purely cosmetic edits do not. For rollback, revalidate the previously confirmed target and topology, and fail open instead of applying stale coordinates. An edit combining critical and cosmetic fields follows the critical trial.

## Verification

- Unit tests: negative origins; mixed resolutions; noninteger scaling; rotation; adjacent and gapped displays; swapped primary; unplugged/replaced target.
- Integration tests: external-left, external-right, above/below and dual external; dock hot-plug/resume. Verify uniquely identified forward monitor, unchanged coordinate when possible, nearest orthogonal-coordinate correction on offset target, and blocked jumps for ambiguous or invalid gapped layouts.
- Confirm external monitor coordinates are never classified as part of the target dead zone.

## Stop conditions

Target identity ambiguous, monitor geometry invalid, inconsistent DPI coordinate systems, or stale topology: fail open rather than restricting guessed pixels.

Read [behavior contract](../../../docs/behavior-contract.md) for the required outcomes.
