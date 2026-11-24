# EV-CP3 — Restore the Rod-Shaped Organism

**Status:** specification / implementation task  
**Repository:** `maakeeuu/Evolution`  
**Base:** current `main` at the time the agent starts; inspect the actual HEAD first.  
**Unity:** 6000.6.10f1, Built-in Render Pipeline, strictly 2D.

## 1. Purpose

EV-CP3 corrects a fundamental visual and anatomical regression introduced during the transition to EV-CP2.

The user explicitly wanted a simple, elongated, rod-shaped organism. A previous project version displayed the desired stick/rod silhouette. The newer EV-CP2 presentation is perceived by the user as an oval. This is not an acceptable interpretation of the target organism.

The task is **not** to make another oval more detailed, add decorative lines to it, or merely make an oval narrower. The task is to identify why the previous elongated silhouette was lost, recover the actual earlier shape/geometry as a reference, and integrate the correct silhouette with the articulated physical organism.

## 2. Non-negotiable visual target

The organism must read immediately as a **long, narrow, rod-like living body**, not as:
- an oval, ellipse, blob, disk, capsule-shaped lump, or rounded mass;
- a stack of overlapping thick capsules that visually merge into an oval;
- a decorative oval with bones or muscles drawn on top;
- a rigid line that cannot bend.

The intended starting organism is a simple elongated body with a clear longitudinal axis and a small cross-section relative to its length. It must be able to bend through its articulated structure while retaining a slender overall body profile.

Do not invent a new target silhouette from personal taste. **Inspect the previous implementation and its Git history** to locate the exact version the user approved as a rod/stick. Use its geometry, proportions, contour logic, and visual behavior as the primary reference. If the exact historical source cannot be located, state that explicitly and use the simplest narrow rod silhouette consistent with the repository's documented concept; do not silently claim it is an exact restoration.

## 3. Required first action: forensic audit, no edits yet

Before changing code:

1. Inspect current `main`, recent EV-CP1/EV-CP2 commits, and Git history for the version in which the user saw the desired rod.
2. Identify the exact source files, components, parameters, and geometry responsible for:
   - the old rod-shaped appearance;
   - the current oval-like appearance;
   - the EV-CP2 articulated skeleton and muscle layout;
   - the current capsule-patch body shell and how patches overlap.
3. Compare old and current geometry. Explain concretely which implementation change caused the silhouette regression. Do not guess based only on class names.
4. Determine whether the oval comes from the physical segment layout, shell-generation algorithm, segment radii, patch overlap, camera/render scaling, or a combination.
5. Produce a short audit report with file paths, relevant functions/parameters, historical commit references, and a proposed minimal correction.
6. Only after the audit, implement the correction.

Do not stop after writing the report. Continue into implementation unless the repository evidence makes a safe implementation impossible; if blocked, explain the exact blocker and the smallest required clarification.

## 4. Preserve the EV-CP2 physical foundation

Do not solve the visual regression by reverting to EV-CP1 or replacing the articulated organism with a static mesh.

Preserve and verify the existing physical architecture:
- individual dynamic 2D body segments;
- real joints and their constraints;
- antagonist muscle pairs and their actual force application;
- physical movement through the physics engine, not transform animation;
- deterministic procedural construction;
- existing diagnostics and isolated joint probe;
- existing scene-builder workflow and time-control behavior unless a specific defect requires a change.

The rod silhouette must be generated from and follow the articulated anatomy. It must bend when the physical joints bend. It must not be a separately animated visual object that merely imitates the physics.

Do not claim that a successful source-level test proves native Unity physics works.

## 5. Body geometry and shell requirements

Design the body representation around the longitudinal articulated chain.

- The shell must follow the actual segment/joint pose every physics/render update using a stable, deterministic method.
- The cross-section must remain narrow relative to body length.
- Adjacent sections must connect continuously without large bulbous overlaps.
- Joint regions may flex, but must not inflate into circular/oval masses.
- The body must remain visually slender in straight and bent poses.
- Geometry must avoid self-intersection where reasonably possible and must not flicker, invert winding, or develop visible gaps during bending.
- Do not use one global ellipse or a radial fan as the organism's body.
- Do not use a set of thick overlapping capsules as a shortcut for the final shell.
- A simple strip/ribbon-like contour or another suitable articulated 2D skin representation is acceptable if it is generated from the skeleton and preserves the slender profile.
- Keep physical colliders and visual shell conceptually separate: visual smoothing must not silently alter the physical mass, collision shape, or joint mechanics.
- Do not fake a rod by scaling a sphere/ellipse in the renderer while the underlying physical anatomy remains inconsistent.

The implementation should make the geometry understandable and maintainable. Document how the shell is constructed from segment centerlines, local directions, widths, and joint transitions.

## 6. Procedural and deterministic behavior

The same initial state, procedural parameters, and environmental inputs must produce the same initial anatomy and deterministic construction result. Do not add arbitrary random variation to hide geometry defects.

Future organisms may differ when their actual procedural inputs or interaction histories differ, but random noise is not a substitute for a coherent morphology system.

Do not implement learning, natural selection, reproduction, inheritance, or a large new environment system as part of this correction. This checkpoint is specifically about restoring and validating the organism's basic anatomy and silhouette while retaining articulation.

## 7. Required visual acceptance criteria

Create or update a dedicated Unity demo scene / builder output so the user can inspect the actual organism without manually assembling GameObjects.

At minimum, provide:
1. **Straight/rest pose:** unmistakably a long, narrow rod, not an oval.
2. **Bent pose:** visibly articulated and curved while remaining slender.
3. **Joint/muscle inspection:** bones, joints, and muscles correspond to the same anatomy; optional diagnostic visualization can be toggled.
4. **Shell inspection:** no capsule pile, no bulbous joint knots, no large oval mass, no disconnected patches.
5. **Reproducibility:** rebuilding the same scene with the same parameters yields the same initial geometry.

Use clear, neutral presentation. Do not add a heavy HUD or decorative effects to distract from the body shape.

The user must be able to judge the silhouette immediately. Provide a simple visual validation mode or scene arrangement that shows straight and bent configurations side by side or sequentially, without replacing the actual runtime organism with illustrations.

## 8. Tests and verification

Add source-level tests for:
- slenderness / longitudinal proportions of the generated rest-pose shell;
- no accidental radial/elliptical global body generation;
- continuity of adjacent shell sections;
- stable contour generation under representative joint angles;
- deterministic geometry for identical inputs;
- geometry following articulated segment poses;
- no changes to physical mass/collider/joint values caused by visual shell updates;
- preservation of existing EV-CP2 mechanics and diagnostics.

Use numerical assertions based on explicit geometry definitions rather than brittle screenshot-only checks. A screenshot or rendered visual is still required as a human acceptance artifact where the available environment supports it.

Unity verification boundary:
- If Unity Editor is unavailable, say so.
- Do not claim Unity API compilation, scene serialization, shader import, Play Mode, physics, or Build success unless actually executed in the target Unity Editor/version.
- Run all available repository tests and CI checks.
- Keep native Unity validation instructions exact and short.
- Do not report the isolated joint probe as proof that the complete rod-shaped organism or its shell is correct.

## 9. Scope and repository safety

- Work in the current repository and current branch unless the user explicitly directs otherwise.
- Do not force-push or rewrite history.
- Do not delete or overwrite unrelated user changes.
- Preserve `Assets/Evolution/EVCP1/` exactly; EV-CP1 is historical and must remain intact.
- Do not touch the separate Procedural Everything Engine (PEE) project.
- Keep the canonical source/runtime mirror relationship consistent.
- Follow repository C# and Unity compatibility constraints.
- Avoid unnecessary dependencies and unrelated refactors.
- Commit completed, coherent work to `main` only if repository workflow and permissions allow it; report exact commit SHA and CI result. If a safe commit cannot be made, leave changes clearly described.

## 10. Definition of done

EV-CP3 is not complete merely because:
- the code compiles outside Unity;
- the joint probe passes;
- bones and muscles are visible;
- the body has become somewhat thinner;
- a static rod-shaped sprite is drawn over the physics.

It is complete only when:
- the old approved rod implementation has been located and compared, or the inability to locate it has been explicitly documented;
- the runtime organism is unmistakably rod-shaped in its initial state;
- it bends through real articulated physics;
- the procedural shell follows that bending while staying slender and continuous;
- tests cover geometry and determinism;
- repository checks pass;
- all unexecuted native Unity checks are explicitly marked pending;
- the user can open a generated demo scene and inspect the result.

**Priority order:** correct silhouette → anatomically coherent shell → real articulation → validation. Never sacrifice the rod silhouette to make the implementation easier.
