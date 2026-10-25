---
name: verification-release
description: Use when planning or validating tests, building a Windows binary, setting up CI, packaging artifacts, or preparing a release.
---

# Verification and release

**Input:** changed requirement IDs, current source tree, build/tool availability, target Windows environment.
**Output:** reproducible commands, measured evidence, artifact information, and explicit verified/unverified status.

## Procedure

1. Inspect the current repository, pinned SDK and existing workflows before proposing new build steps.
2. Map each change to acceptance criteria and pure geometry tests, then identify Win32 paths needing an actual Windows desktop.
3. Run available deterministic tests; record command, runner/OS and outcome. A CI green result is **not** proof that hook/monitor behavior works.
4. For input control changes, verify escape hotkey, exit recovery, external crossing and topology failure on real hardware before announcing readiness.
5. For any proposed new GitHub Actions workflow or material expansion, obtain maintainer approval first. Use standard runners and low-privilege permissions when later authorized.
6. Package only validated artifacts; provide version, checksums, known limitations, license and release notes. Do not silently publish.
7. If physical Windows testing is unavailable, report exactly what remains untested and keep the release blocked.

## Verification

- Correct package architecture/runtime declaration and dependency/license notices.
- No secrets, PFX keys, machine-specific IDs or unreviewed binaries committed.
- Reproducible build after SDK pinning; distinguish build, unit tests, Windows integration and multi-monitor acceptance.

## Stop conditions

No recovery evidence, new privilege requirement, unknown paid runner/signing cost, unverified monitor interactions, or release permission missing: stop the affected action and report.
