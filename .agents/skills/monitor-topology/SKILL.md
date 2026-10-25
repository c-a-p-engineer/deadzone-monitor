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
3. Convert normalized local coordinates to virtual desktop rectangles only for the current topology snapshot.
4. Validate target existence, unique identity, valid bounds and zone area. If ambiguous, return `unresolved` and disable intervention.
5. Recompute after attach/detach, resolution, rotation, scaling and resume events; make updates atomic from the routing engine's perspective.

## Wizard integration

For initial onboarding, provide an explicitly selected physical target monitor, correctly scaled local-pixel schematic and exact coordinate editing without modifying the OS monitor arrangement. Revalidate display identity, orientation and bounds between the wizard's monitor selection, area preview and explicit enable. Keep the configuration in draft mode until consent, and provide keyboard-only monitor selection and recovery when the damaged panel cannot display the whole wizard. The later detailed-settings view must use the same validated geometry model. **First-run activation has an agreed 60-second trial:** explicit trial start, confirmation before the deadline commits the draft, and timeout/cancel returns to the last confirmed profile if still valid or disables guarding if none is available. Confirmation and countdown must remain in reachable space with keyboard access; global emergency disable is immediate. Handling later advanced-settings edits with the same trial remains an open product decision.

## Verification

- Unit tests: negative origins; mixed resolutions; noninteger scaling; rotation; adjacent and gapped displays; swapped primary; unplugged/replaced target.
- Integration tests: external-left, external-right, dual external; dock hot-plug/resume.
- Confirm external monitor coordinates are never classified as part of the target dead zone.

## Stop conditions

Target identity ambiguous, monitor geometry invalid, inconsistent DPI coordinate systems, or stale topology: fail open rather than restricting guessed pixels.

Read [behavior contract](../../../docs/behavior-contract.md) for the required outcomes.
