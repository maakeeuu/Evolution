# HANDOFF — EV-CP3 Restore the Rod-Shaped Organism

Repository: https://github.com/maakeeuu/Evolution  
Full task specification: [EV_CP3_SPEC.md](EV_CP3_SPEC.md)  
Base branch: `main`; inspect actual HEAD before work.  
Unity: 6000.6.10f1, Built-in, 2D only.

The previous version had the simple elongated rod/stick silhouette the user wanted. The current EV-CP2 organism is perceived as an oval. This is a regression and must be corrected.

Read the full `docs/EV_CP3_SPEC.md` before editing. First perform a forensic comparison against Git history and locate the exact earlier rod implementation. Identify which code/geometry change caused the oval-like result. Then restore the rod silhouette while retaining EV-CP2's real articulated segments, joints, antagonist muscles, diagnostics, and physical movement.

Do not revert to EV-CP1, do not replace physics with a static sprite, and do not merely make the oval thinner. The body must be a long, narrow, bendable articulated organism. Its continuous procedural shell must follow the skeleton and remain slender in straight and bent poses. Avoid thick overlapping capsule patches and bulbous joint regions.

Implement tests for shell proportions, continuity, bending, determinism, and separation of visual shell from physical colliders/mass. Provide a Unity demo scene/builder path for user inspection.

Do not claim native Unity validation unless it was actually run in Unity 6000.6.10f1. Preserve EV-CP1 and do not touch PEE. No force-push or history rewrite.

Start with a concise audit report, then implement the correction. Do not stop at analysis unless a specific blocker prevents safe implementation. Report exact files, tests, commit SHA, CI, and pending Unity checks.
