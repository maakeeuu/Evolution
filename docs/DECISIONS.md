# Decisions — Evolution

## Current confirmed project direction
- Unity 6000.6.10f1; strict 2D.
- The organism has procedural anatomy and continuous vector geometry.
- The primary concept is one organism learning and changing during its own lifetime. Generations, reproduction, inheritance, death and rebirth are not mandatory mechanics.
- Procedural development should be driven by deterministic, reproducible simulation rules, interaction and experience. A seed may reproduce initial conditions but must not replace the development mechanism.
- The environment is natural only: terrain, slopes, depressions, rocks, roots, vegetation, water, currents, weather and other natural factors. No artificial traps or contrived obstacle courses.
- Behavioral learning changes control and behavior; morphological adaptation changes anatomy. These are separate mechanisms.
- EV-CP1 remains preserved as a radial technical fixture. Its oval is not the target organism anatomy.
- EV-CP2 is based on a procedural skeleton, joints, force-producing muscle actuators, real 2D physical interaction and a continuous procedural body shell.
- The initial organism should be minimal and stick-like, not a predetermined animal with fixed legs, head, tail or a predefined evolutionary tree.
- Keep Evolution_Source/ canonical and isolate future EV-CP2 runtime files from Assets/Evolution/EVCP1/.
- PEE is a separate project and must not be modified.
- Full EV-CP2 requirements: docs/EV_CP2_SPEC.md. Agent kickoff instructions: docs/HANDOFF_EV_CP2.md.

## Historical and temporary decisions below


## Confirmed
- Unity 6000.6.10f1.
- Strictly 2D.
- Procedural vector creature.
- Morphology must change gradually and seamlessly.
- The creature learns from the consequences of its actions and adapts.
- Minimalist environment: obstacles, water, weather effects.
- The active game view should have almost no HUD.

## Temporary prototype choices
- First body uses a closed radial contour with a fixed number of vertices.
- Demo changes a small number of continuous shape parameters slowly. This is a geometry demonstration, **not** biological adaptation or learning.
- Prototype palette is temporary.

## Open
Camera view, player control model, creature type, water/weather details, generations/inheritance, death rules, long-term goal.

## Source-only EV-CP1 choices — 2026-11-24
- Active code lives in Evolution_Source until explicit Unity integration approval; root Unity folders remain untouched.
- The contact fixture temporarily uses gravity-free XY motion and one rectangular obstacle. Camera/control model remains open; this does not commit the final game to a top-down design.
- Use one fill silhouette (no outward outline) so visual boundary and PolygonCollider2D use the same coordinates. A small local unlit shader avoids new rendering packages.
- MorphologyDemo2D is a demonstration driver only. Target interpolation is not an adaptation policy; motion intent is not learning; no generation/inheritance system is added in EV-CP1.


## Integration Readiness lifecycle contract — 2026-11-24
- Body and obstacle own their generated geometry and collider path while attached. Use dedicated prototype objects; externally supplied Mesh/Material assets are never destroyed. Previous render slots/property blocks are restored only if the component's assignment is still present; teardown disables its collider rather than restoring an unknown prior collider path.
- Initialization failure is fail-closed: rendering/collision disabled, partial owned resources released, component disabled, one error. Recreate the component after fixing configuration; re-enable is not an implicit retry. A normal disable/re-enable preserves the valid shape.
- Motor readiness is evaluated in FixedUpdate after Awake ordering has settled. Late-added Rigidbody2D is resolved when geometry next changes.

- Independent adapter verification uses actual Roslyn C# 9 parsing with SDK-provided compiler assemblies, not Unity stubs. This is a syntax/contract gate only; Unity API binding and native behavior remain explicitly pending.
- CI runs for progress/decisions-only checkpoints as well as source changes and verifies the checked-out SHA so the reported main HEAD always has its own run.

- Pin standalone SDK resolution to .NET 8 inside Evolution_Source and assert it in CI. Installing an SDK alone does not select it when hosted runners include newer SDKs; SDK-provided Roslyn must match the standalone verifier runtime.

## Authorized EV-CP1 integration — 2026-11-24

- Use additive Assets/Evolution/EVCP1 runtime mirrors; preserve Evolution_Source canonical sources and enforce byte equality in CI. Root source outside Assets is not imported by Unity, avoiding duplicate asmdefs.
- Preserve legacy source/menu and namespace; change only the invalid four-argument RequireComponent declaration. Do not co-locate legacy/new render owners on the same GameObject.
- Enable only the necessary Editor built-in Physics 2D module from the previously empty manifest; no external package or engine migration.
- Prepare manual empty-scene/bootstrap instructions rather than hand-authoring scene GUIDs. Let the target Editor generate genuine .meta files and serialized scene references during user validation.


## User-confirmed Built-in shader correction — 2026-11-24

- In the target Unity 6000.6.10f1 Built-in Render Pipeline, the user locally observed that `LightMode=SRPDefaultUnlit` left the procedural mesh fill invisible while the selected-object outline remained visible.
- Changing the pass tag to `LightMode=Always` made the creature fill and obstacle render in Play Mode. Treat this as a user-validated correction for the current Built-in baseline, not as evidence of SRP compatibility.


## EV-CP2 first physical slice — 2026-11-24

- Keep CP2 under Evolution_Source/Articulated with distinct Evolution.Articulated.Core/Unity assemblies and runtime mirror Assets/Evolution/EVCP2. Reuse CP1 pure geometry helpers and single shared Built-in Resources shader; preserve all CP1 runtime files.
- Begin with a serial, tapered rule-generated chain, not an animal or a general graph framework. Data and activation commands are separate from Unity bodies and control. Branching and continuous anatomical growth are future work, not simulated by a seed or rebuild.
- Muscles are antagonistic pull-only spring/damper paths across hinges. Activation shortens preferred length; bounded equal/opposite endpoint forces must use AddForceAtPosition, not root velocity or transform animation. No motorized joint or scripted trajectory is a substitute.
- Unity native physics is not promised bitwise deterministic across platforms. Core generation and actuator math are reproducible; repeatability of native trajectories requires actual target-Editor measurements.


## EV-CP2 initial Unity integration contract — 2026-11-24

- Distinct physical siblings per segment, data parent indices, HingeJoint2D endpoint constraints and pairwise neighbor collision suppression. Keep non-neighbor self-contact. No joint motor, root motor, Rigidbody velocity commands or post-initialization Transform motion.
- Separate organism physics/actuator owner, optional wave command driver, optional render view and scene bootstrap. Read actual angular/body/contact state as the future sensing seam; do not introduce empty AI abstractions.
- First envelope is the opaque geometric union of overlapping convex patches with exact per-patch mesh/collider correspondence and interpolation inherited from the same body. It is a deliberately incomplete shell milestone, not a smooth global skin. Growth/topology adaptation is not implemented or faked by rebuilding.
- Reuse the single user-validated Built-in shader resource from EV-CP1 without changing any EV-CP1 file. New runtime resources are owned/released, and no packages, global physics settings or hand-authored metadata are required.

## EV-CP2 articulation repair A — 2026-11-24
- Separate general muscle defaults from explicit demo load tuning. A source-level gravity/lever budget is reproducible evidence of insufficient previous demo drive, not proof of the sole native failure cause. Do not change gravity/friction/global solver settings to hide it.
- Inspector diagnostics read native state and last completed adapter force batch; they are off by default and quarter-second sampled in unscaled time. Colors still indicate activation only, never proof of force/movement.
- Native probe requires real opposite signed joint displacement plus conservation, not just nonzero tension. Fresh specimens isolate antagonist tests without live-body teleport/velocity reset. No learning/locomotion claims.

## EV-CP2 scene workflow B — 2026-11-24
- Use an Editor-only assembly and explicit menu commands, never an import/startup simulation hook. Create/save a scene only when its target path does not exist; otherwise open without mutation. Preserve unsaved scenes through Unity's save/cancel prompt. Native scene GUIDs and script references must be produced by Unity 6000.6.10f1 locally. No fake YAML placeholders.

## EV-CP2 simulation time C — 2026-11-24
- Keep fixedDeltaTime in game seconds invariant rather than trading away joint accuracy at speed. Do not increase maximumDeltaTime to conceal low throughput; display measured FixedUpdate advancement separately from requested scale.
- Use IMGUI (one required Unity built-in module) instead of pulling in uGUI/Input System for five buttons. Game View focus is required; keyboard shortcuts use IMGUI key events, ignoring modifiers. Controls are confined to an explicitly created demo, not CP1 or project startup.
- A single static adapter owner plus independently tested pure value lease controls writes. Restore the prior per-enable value only if last written value still matches; preserve a detected external writer. Same-value writes cannot be detected without cooperation from all time managers. Disable duplicate controllers rather than multiplying or fighting time scales.
