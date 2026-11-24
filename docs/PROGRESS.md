# Progress — Evolution

## Current milestone: EV-CP1 — Procedural Morphology

**Repository bootstrap:** committed.  
**Procedural creature code:** committed.  
**Unity Editor validation:** pending.  
**Learning AI:** not implemented.  
**Environment simulation:** not implemented.  
**Unity version:** 6000.6.10f1.

## Implemented
- `ProceduralCreature2D.cs`: fixed-resolution radial contour, stable triangle-fan mesh, gradual parameter interpolation, separate fill/outline materials, and PolygonCollider2D generated from the same contour.
- `EvolutionDemoSceneBuilder.cs`: menu command creates an orthographic test scene.
- `MASTER_PROMPT_RU.md`, `AGENTS.md`, `CLAUDE.md`, project version, package manifest and short build docs.
- Impeccable pinned as a Git submodule.

## Checks performed
- GitHub tree and committed file contents verified.
- C# source braces balanced; key geometry/collider paths present.
- Independent Python check of the contour formula: 32 combinations of the allowed extreme morphology parameters and resolutions 24/32/64/128 had positive polygon area and no proper intersections between non-adjacent edges.
- **Not performed:** C# compilation, Unity import, Play Mode run, or Unity EditMode/PlayMode tests. Those require the actual Unity 6000.6.10f1 Editor.

## Acceptance criteria
- Generate one closed procedural vector body.
- Change parameters continuously and gradually.
- Preserve stable contour topology during the demo.
- Keep PolygonCollider2D synchronized with the rendered contour.
- Create and run the minimal 2D scene in the target Unity Editor.

## Handoff
The first implementation handoff is recorded in [HANDOFF_EV_CP1.md](HANDOFF_EV_CP1.md). It accurately records what is implemented, what was statically checked, and what still requires Unity Editor validation.

## Previous next step (superseded by source-only development)
Open the repository in Unity 6000.6.10f1, run **Tools → Evolution → Create Morphology Demo**, press Play, and inspect the body, outline, collider and Console. Fix any import/runtime issues before adding movement or AI.


## EV-CP1 source checkpoint 1 — 2026-11-24

- Active work is now exclusively in `Evolution_Source/`; existing `Assets/`, `Packages/`, `ProjectSettings/` are unchanged. No Unity integration/build is authorized yet.
- Preserved the bounded radial contour and stable fan; extracted engine-independent morphology, geometry and validation into `Code/Core`.
- Added finite-value rejection, complete non-adjacent segment/touch checks and exact exponential target response. No learning, adaptation policy or inheritance is implied by interpolation.
- Added a dependency-free .NET 8 console suite against actual C# sources and an independent Python mathematical/structural reference.
- Executed: 5 Python test groups passed (128 extreme shapes, 80 seeded random shapes, 100 transition frames, constant-target responses at 30/50/60/144 Hz and source contracts). These are reference checks, not execution of the C# implementation.
- Not executed: C# compilation (no dotnet/csc/mcs installed; external DNS unavailable), Unity import, Editor, Play Mode or Unity tests.
- Next: implement a source-only Unity adapter that applies one contour to mesh and PolygonCollider2D in FixedUpdate, without the old four-type RequireComponent declaration or OnValidate state reset.

## EV-CP1 source checkpoint 2 — 2026-11-24

- Added `CreatureBody2D`: stable topology allocated in Awake, finite Inspector normalization without OnValidate resets, one float contour shared by Mesh and PolygonCollider2D, synchronized FixedUpdate publication, generated mesh/material cleanup.
- Added an explicitly non-learning `MorphologyDemo2D` target driver and a minimal unlit vector shader in Resources. A fill-only silhouette deliberately avoids the legacy outline extending beyond the collider. Temporary color only.
- Corrected the legacy four-type RequireComponent pattern in the replacement by requiring only three necessary components. Legacy file remains untouched.
- Runtime namespace and assemblies are distinct from legacy code; source files are not integrated into Assets.
- Executed: 5 additional Python structural checks (component attribute arity, shared float geometry/fixed-step wiring, OnValidate safety, assembly/shader wiring, delimiter smoke checks); all 5 geometry groups rerun and passed.
- Not executed: C# or shader compilation, Unity import/physics/rendering. Shader targets the current dependency-free/built-in baseline; other render pipelines are not validated.
- Next: add acceleration-limited Rigidbody2D movement, a single vector obstacle, and a manually opt-in runtime demo bootstrap under Evolution_Source only.

## EV-CP1 source checkpoint 3 — 2026-11-24

- Added independent `PlanarMotion` velocity math, acceleration-limited `CreatureMotor2D` with Rigidbody2D CCD/interpolation, optional legacy-keyboard adapter, one matching vector mesh/PolygonCollider2D obstacle, and opt-in `EvolutionPrototype2D` runtime fixture. No Editor scripts, scene assets or auto-integration.
- Default fixture intent pushes into the single obstacle; this is a future contact test, not obstacle avoidance/learning. XY/no-gravity motion is temporary and reversible.
- Added 6 Python motion/reference/wiring groups, 1,000 seeded acceleration cases and C# console assertions for velocity/invalid input.
- Executed: complete Python suite, 16 test groups passed (geometry, transitions, velocity math, structural wiring). They do not execute C#, shaders or Unity collision response.
- C# console suite is provided but NOT RUN: no C#/.NET compiler installed and external DNS is unavailable. Unity import/compilation, shader rendering, contact stability, collider-resizing effects and Play Mode tests remain UNVERIFIED.
- Updated source README and pending Unity checklist; recorded temporary decisions. All game code/resources/tests remain under Evolution_Source. Assets/Packages/ProjectSettings, Unity version, package manifest and other repositories are unchanged.
- EV-CP1 is NOT accepted as Unity-validated. Learning within a lifetime, long-term adaptation policy and intergenerational evolution remain separate, unimplemented systems.
- Next concrete gate: execute the actual C# console suite with a .NET 8 SDK; keep Unity contact/render validation pending until explicit integration authorization. Do not build or integrate Unity automatically.
