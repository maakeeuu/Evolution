# Progress — Evolution

## Current milestone: EV-CP1 — Procedural Morphology

**Repository bootstrap:** committed.  
**Procedural creature code:** committed.  
**Unity Editor validation:** pending.  
**Standalone C# validation:** passed in GitHub Actions (hardening runs #1 and #2; see below).  
**Learning AI:** not implemented.  
**Environment simulation:** not implemented.  
**Unity version:** 6000.6.10f1.

## Implemented
- `ProceduralCreature2D.cs`: fixed-resolution radial contour, stable triangle-fan mesh, gradual parameter interpolation, separate fill/outline materials, and PolygonCollider2D generated from the same contour.
- `EvolutionDemoSceneBuilder.cs`: menu command creates an orthographic test scene.
- `MASTER_PROMPT_RU.md`, `AGENTS.md`, `CLAUDE.md`, project version, package manifest and short build docs.
- Impeccable pinned as a Git submodule.

## Checks performed (original bootstrap; superseded by source/CI checkpoints below)
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

## EV-CP1 hardening checkpoint 1 — 2026-11-24

- Added read-only GitHub Actions CI for actual .NET 8 compilation/core assertions and Python checks. The matrix includes all three historical source checkpoints and the current commit.
- Only the required workflow entry point is outside Evolution_Source; game and test code stay inside it. No Unity Editor, build, package changes, secrets, automatic result commits or submodule checkout.
- Local environment still has no C# compiler and external DNS is unavailable. CI execution/results are pending at this checkpoint; configuration is not reported as a pass.
- Next: inspect CI results, extend adversarial C# geometry/motion tests and validate float-quantized geometry used by Unity adapters.

## EV-CP1 hardening checkpoint 2 — 2026-11-24

- Confirmed CI run https://github.com/maakeeuu/Evolution/actions/runs/45410854846 succeeded: all four matrix jobs passed actual C# compilation/execution and Python steps, including all three historical checkpoints. This validates standalone core, not Unity adapters or physics.
- Fixed cancellation in tiny positive morphology steps; preserved zero-time/zero-acceleration velocity without unnecessary overflow arithmetic; rejected overflowing contour wedges.
- Added float-compatible contour generation and extensive actual-C# regressions: all 105 supported resolutions, 32 extreme parameter combinations plus seeded shapes, winding/degeneracy/self-intersection/NaN/overflow rejection, fan-versus-polygon area, 2,000 changing-target frames and 10,000 motion cases.
- Added a C# 9 / .NET Standard 2.1 compile target matching the intended Unity core API surface. It does not reference or compile UnityEngine. CI now builds it for current revisions.
- Local independent Python suite: 19 tests passed. Independent worst analytic radial-area error across all extreme shapes/resolutions was about 1.35%, below the 4% regression bound. New C# regression and compatibility results are pending this commit's CI.
- Next: consume and validate float geometry in Unity adapter, avoid unchanged collider republishing, wake changing bodies, and prepare opt-in Unity contact/equality tests without running/importing Unity.

## EV-CP1 hardening checkpoint 3 — 2026-11-24

- Confirmed hardening run #2 https://github.com/maakeeuu/Evolution/actions/runs/45412090992 completed successfully. The current C# core regression suite and C# 9 / .NET Standard 2.1 build passed, alongside historical matrix checks and Python references.
- CreatureBody2D now consumes/validates the float-compatible contour before native publication; the exact same float coordinates drive mesh and polygon. Validation cannot be disabled. Identical geometry no longer triggers Mesh/SetPath publication; changed geometry wakes a cached Rigidbody2D. GeometryRevision exposes actual publication count.
- Removing the body component now disables its owned collider, avoiding a remaining invisible solid after mesh cleanup.
- Added three opt-in PlayMode integration tests under Evolution_Source/Tests/Unity: exact native Mesh/PolygonCollider coordinates during morphing, unchanged-publication/re-enable behavior, and moving/morphing obstacle contact plus braking. These tests have NOT been compiled/imported/run in Unity and are excluded from standalone CI.
- Updated integration/test instructions. No Unity scene/project assembly or package changes, no Editor invocation, no learning systems.
- Executed locally: 20 Python reference/structural groups passed. This commit's CI result is recorded separately when available; earlier core results do not validate Unity adapter changes.
- Remaining gate: actual Unity 6000.6.10f1 C#/shader import and the pending PlayMode/contact/rendering checks, only after explicit integration approval. In particular, shape-growth depenetration, impulses and visible smoothness are still unverified.

## EV-CP1 Integration Readiness — checkpoint 1, 2026-11-24

- Initial main HEAD was a64cf0bb89428593e6e8779e5d1d7312ce7e6b90; inspected its full patch and confirmed hardening checkpoint 3 publication. Queried run 45412773500 jobs: all four have exactly that head_sha and success; current also passed the C# 9 / netstandard2.1 build. Earlier runs were not used as evidence for that HEAD.
- Fixed failure paths in body/obstacle Awake: render/collision stay disabled until initialization succeeds; partial owned resources are detached/released and one exception is reported. A failed component cannot be re-enabled into a half-initialized state.
- Preserve previous mesh/material slots and property blocks when releasing owned resources; only generated Mesh/Material objects are destroyed. Supplied materials must expose _Color.
- Fixed invisible collision left by deleting VectorObstacle2D, and discover a Rigidbody2D added after body Start on the next actual geometry publication.
- Motor checks shape readiness in FixedUpdate (not across unordered Awakes); fixture removes partial objects if a body/obstacle initialization fails.
- Body disable continues to freeze a valid shape rather than delete its collision; failure and destruction are distinct teardown paths.
- Locally executed: 21 Python reference/structural tests. These lifecycle fixes are still static/structural evidence until real Unity tests run. This checkpoint's new CI must be checked before publishing the next checkpoint.

## EV-CP1 Integration Readiness — checkpoint 2, 2026-11-24

- Verified checkpoint 1 publication a80e5e3cda871f06632f65ed0a0fe95d96f1d135 and all four CI jobs with exactly that head_sha in run 45414947740; current C# execution, netstandard2.1 compilation and 21 Python tests passed. This was checked before publishing checkpoint 2.
- Added actual C# publication regressions: bit-identical float output for unchanged shape, immutable fan topology, warmed core hot-path managed-allocation probe, zero-response/large-time behavior and invalid buffer rejection.
- Added a third standalone project, AdapterSyntax, using the .NET SDK's Roslyn assemblies. It parses real Unity adapter and pending PlayMode source as C# 9, checking callback signatures/component filenames/RequireComponent arity. It does not compile Unity APIs or execute Unity and uses no Unity stubs.
- CI now verifies the checkout SHA explicitly, runs all three available standalone projects for current, and also triggers on PROGRESS/DECISIONS-only changes. Full core/syntax logs are retained as artifacts.
- Expanded pending PlayMode suite from three to five tests with obstacle native resource teardown and late Rigidbody wake-up. These are NOT executed or compiled against Unity.
- Locally reran 21 independent Python reference/structural checks and parsed project/assembly/workflow files. Results of the new C# allocation/syntax probes must be verified in this exact commit's CI, not inferred from checkpoint 1.
- Source is prepared for a controlled future integration gate, not accepted as a Unity-validated build. Remaining checks: actual Unity C#/shader import, resource/failure lifecycle, native Mesh/Collider equality, sleeping/resizing contacts, CCD and visual smoothness. No new game systems or Unity integration were performed.
