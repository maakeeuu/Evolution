# Progress — Evolution

## Current milestone: EV-CP2 — Articulated Organism

**Milestone specification:** [EV_CP2_SPEC.md](EV_CP2_SPEC.md)  
**Agent handoff:** [HANDOFF_EV_CP2.md](HANDOFF_EV_CP2.md)  
**Implementation status:** first physical slice implemented and mirrored in EVCP2; native Unity validation pending.  
**Previous milestone:** EV-CP1 technical radial-form fixture, integrated under Assets/Evolution/EVCP1/.

### EV-CP2 target
Move from the EV-CP1 radial oval to a minimal, genuinely articulated procedural organism:
- procedural skeletal segments and joints;
- meaningful joint constraints;
- force-producing muscle actuators that physically affect motion;
- interaction with 2D physics and surfaces;
- a continuous procedural body shell that follows the articulated structure;
- architecture that can later support sensing, behavioral learning and gradual morphological adaptation during one organism's lifetime.

### Scope boundaries
- Keep the game strictly 2D and target Unity 6000.6.10f1.
- Preserve EV-CP1; do not replace or delete its runtime files.
- Keep Evolution_Source/ canonical. When EV-CP2 Unity runtime integration is ready, use a separate destination such as Assets/Evolution/EVCP2/.
- No mandatory generations, reproduction, inheritance, death or rebirth.
- The world is intended to contain natural environmental conditions only. EV-CP2 focuses on the organism, not full biome or weather simulation.
- No learning or morphological adaptation claims until actual mechanisms exist.
- No changes to the separate PEE project.

### Evidence policy
Standalone tests, source inspection and CI do not prove UnityEngine API compilation, shader import, Play Mode, native physics or rendering. Record exactly which checks ran. Unity Editor validation must be performed in the target Editor; do not claim it was performed remotely.

---

## Historical milestone: EV-CP1 — Procedural Morphology


**Repository bootstrap:** committed.  
**Procedural creature code:** committed.  
**Unity integration:** prepared in Assets/Evolution/EVCP1 (2026-11-24); see docs/UNITY_VALIDATION.md.  
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

## Integration Readiness CI feedback — 2026-11-24

- Checkpoint 2 ba721ebf4e4145e9e1b45e9d5929833a05ad2610 was published and its own run 45415906977 checked. The current job compiled netstandard2.1, then failed during actual core regression execution (exit 134); syntax/Python steps were correctly skipped. It was NOT reported as passing.
- The new exact-endpoint probe exposed a real rounding issue to address: from + (target-from)*1 need not equal target bit-for-bit (independent reproduction: .58+(.15-.58) != .15). Saturated/zero interpolation now returns the validated target/from directly, without arithmetic cancellation. Assertions remain unchanged.
- Core runner now reports caught regression exceptions as CI error annotations and exits nonzero, so failures remain blocking and have inspectable messages rather than only an abort code. Exact new HEAD CI must verify whether any further failure remains.

- Follow-up a3658889c5977c6dda3ad7bec366316c50504963 / run 45416270712: actual core execution now PASS (1,226,597 assertions, including zero managed bytes in the warmed core probe); netstandard2.1 builds successfully. Roslyn standalone step failed before Python. Compiler diagnostics are now preserved from stderr and exposed as CI annotations through run_checked.py; the failing gate remains enabled. No claim of a successful overall run yet.

## Integration Readiness standalone SDK selection fix — 2026-11-24

- Verified aafaeab6c52bae4bb9dc55f6b9427805d741542a and its run 45416756391. Core still passes 1,226,597 assertions. Captured Roslyn diagnostics identify the real failure: despite setup-dotnet installing 8.0.x, the hosted runner resolved SDK 11.0.100, whose compiler assemblies require newer framework assemblies than this net8.0 verifier.
- Added Evolution_Source/global.json selecting stable .NET 8 (latest installed 8.0 feature band). All shell checks run from Evolution_Source; CI asserts the actual SDK is 8.0.x. Old revision jobs get an equivalent ephemeral policy in their checkout, not a rewritten commit.
- This changes test tooling selection only, not Unity version, packages, gameplay or geometry. Roslyn checks and core allocation assertions remain blocking and unchanged. Re-run all jobs on the new HEAD to validate the correction.

## Integration Readiness — verified standalone gate, 2026-11-24

- Verified published main 3c85bc83242abd318f2b11f31bf38d093c5491a3 and all four completed/success jobs in run 45417164290, each bound to that exact workflow head_sha. Current job explicitly verified its checkout, selected SDK 8.0.x, built netstandard2.1/C#9 Compatibility, built/executed net8.0 core regressions, built/executed AdapterSyntax and passed the independent Python checks. The original three checkpoints also passed their actual C# execution/Python jobs under selected .NET 8.
- Actual core regression output: 1,226,597 assertions. Includes all resolutions 24..128, 3,780 extreme/seeded shape cases, exact float contour coordinates, fan winding/indices/area, transitions and motion edge cases; added deterministic repeated publication, stable topology and a warmed 512-step probe asserting zero managed core allocations. This is not a Unity allocation measurement.
- Local Python suite: 21/21 passed. XML csproj, JSON asmdef and workflow YAML were parsed. These are references/structural checks, not C# or Unity execution.
- AdapterSyntax successfully parsed actual adapter and pending PlayMode sources as C# 9 and checked lifecycle signatures/component filename contracts/RequireComponent arity. This is syntax/contracts only: UnityEngine member binding, shader compilation, callbacks and native physics/resources have NOT been tested. No mocks were used.
- Lifecycle fixes cover partial Awake cleanup, material ownership/previous assignment restoration, invisible obstacle collider teardown, motor readiness, late Rigidbody lookup and aborting partial fixture initialization. Valid-shape disable/re-enable preserves geometry. Unchanged quantized contour publication remains skipped; mesh and polygon still receive the same prepared float array.
- Assets tree e9cbc5a33c1b38a0a30eed09a86bf8a0fc175c2b, Packages 2c20327ac0c1e78557b84d0221de2454d721623f, ProjectSettings 796cc0004f62f0667b1397e5e3f0e21305c38af7 remain unchanged from the baseline. No Unity process, project integration, PEE work, learning, adaptation or evolution was started; no force push/history rewriting.
- Conditional readiness: ready to enter a separately authorized Unity 6000.6.10f1 integration/validation gate, not ready to claim EV-CP1 complete or production behavior. Run all five prepared PlayMode cases plus fault-injection/material ownership/manual contact and visual tests, compile the real Unity APIs and vector shader, and profile native publication/allocation behavior there. The precise user action for that future step is explicit authorization to perform controlled integration; no action is needed for current standalone checks.
- This final documentation checkpoint must receive its own successful CI run; the preceding successful run is evidence for its parent, not a substitute for checking the new HEAD.

## EV-CP1 Unity Integration & Validation — integration prepared, 2026-11-24

- User explicitly authorized integration into the existing project, but prohibited Unity/Play Mode/Build execution. Initial actual main was 00c40c0139ae7ed6f2db7d1606665294ae60cf54 (newer than the user's remembered 3c85bc8). Verified its latest run 45417518464: all four jobs completed successfully for that exact workflow SHA.
- Audited the complete existing Assets tree (one legacy runtime component, one Editor scene builder; no scenes/asmdefs/metas), empty package manifest and sole ProjectVersion setting. Safe additive destination: Assets/Evolution/EVCP1. No namespace/type/assembly-name collision found with Evolution.Creatures / Evolution.EditorTools.
- Copied 13 runtime files from Evolution_Source/Code and Art byte-for-byte into Assets/Evolution/EVCP1, preserving Core/Unity asmdefs and one Resources/EvolutionVector2D shader. Retained canonical source outside Assets with a CI equality gate. No tests, Python, standalone projects, SDK or CI tools imported into Assets.
- Preserved both old components. Necessary minimal legacy compile-blocker repair: split the unsupported four-argument RequireComponent into two legal attributes; no legacy gameplay rewrite. Existing Editor builder remains byte-identical and still creates the old demo.
- Added only com.unity.modules.physics2d:1.0.0 (Editor built-in module needed by the existing/new Rigidbody2D and Collider2D use). No third-party package, Input System, SRP or Test Framework added. Unity version/ProjectSettings unchanged.
- No GUID/meta/scene/prefab was fabricated. New .meta files must be generated/preserved by the user's first actual Editor import. Detailed fresh-scene setup, independent keyboard test, acceptance/failure/resource checks and known limitations are in docs/UNITY_VALIDATION.md; root/source README updated.
- Added six integration reference/structure checks and extended Roslyn syntax coverage to actual Assets, including the legacy/editor sources. Workflow triggers now cover integration files/config/manual instructions. All new integration CI results must be checked against the published integration commit itself.
- No Unity process was launched; C# binding against UnityEngine, shader compilation, native collision/render/resource lifecycle and five pending PlayMode tests remain UNVERIFIED. EV-CP1 is not declared complete. Next action belongs to the user: open 6000.6.10f1, import, follow the manual checklist, report results. No next game system until then.

- Executed before publishing integration: 27/27 local Python reference/structure checks passed; all asmdef/project/manifest JSON/XML and workflow YAML parsed successfully. Actual C#/Roslyn results for this integration commit remain subject to its own CI run.


## User-run Unity rendering fix — 2026-11-24

- During local Unity 6000.6.10f1 validation, the user confirmed that both runtime objects had assigned meshes/materials and were moving, but their fill geometry was not rendered normally in the Built-in Render Pipeline.
- Replacing the shader pass tag `LightMode=SRPDefaultUnlit` with `LightMode=Always` in the local runtime shader made both the creature and obstacle render as filled shapes. This is direct user-observed Play Mode evidence for the Built-in pipeline fixture.
- Synchronize the canonical `Evolution_Source/Art/Resources/EvolutionVector2D.shader` and integrated mirror `Assets/Evolution/EVCP1/Art/Resources/EvolutionVector2D.shader` to the tested tag. This fixes rendering only; it does not add skeletal anatomy, muscles, locomotion articulation, learning, or biological adaptation.


## EV-CP2 checkpoint 1 — deterministic anatomy and muscle mechanics, 2026-11-24

- Audited main 895bf8d6eb1d7152d74508d241e1361b0c1a1ea9 and its successful CI run 45431804330; read the full handoff/spec and all required project documents plus three relevant C#/physics/timestep skills.
- EV-CP1 user-confirmed Built-in rendering fix is present in both shader copies. Preserve EV-CP1; reuse only Point2, finite guards, contour validator/fan topology and the existing vector resource. Radial morphology, its single-body motor and demo do NOT model articulated anatomy.
- Added a deterministic 2..8 segment serial developmental rule with proportionate taper, parent topology, dimensions/masses/joint limits and two antagonistic attachment paths per hinge.
- Added force-capped pull-only spring/damper muscles with activation-dependent rest length and dt-based activation dynamics. No learned control or adaptation is implied. Added a convex float-compatible segment envelope intended to share coordinates between mesh and collider.
- Added actual-C# standalone tests for topology, attachment coincidence, repeatability, contour winding/indices/area, muscle tension/torque/momentum, activation rate invariance and invalid inputs. Included CP2 Core in netstandard2.1/C#9 compatibility and added a blocking CI execution step.
- Local C# toolchain is unavailable; this checkpoint's actual C# result must be read from its own CI, not inferred. Unity APIs, solver, locomotion and rendering have NOT been tested. No Unity runtime integration in checkpoint 1.


## EV-CP2 checkpoint 2 — physical chain and force-driven demonstration, 2026-11-24

- Verified checkpoint 1 a7507eead5e0a8c3b166c625ac251931882cee81: run 45434470704 has four completed/success jobs for exactly that head_sha. Current passed actual CP2 C# mechanics, CP1 regressions, C#9/netstandard2.1 compatibility, Roslyn and Python.
- Added Unity physical builder: separate dynamic bodies, exact shared float segment contours, connected HingeJoint2D anchors/limits, no joint motors, equal/opposite point forces from antagonistic muscles in FixedUpdate, point-velocity damping and bounded activation/tension. Native motion has not been observed remotely.
- Added separate open-loop wave excitation (explicitly not learning), render diagnostics for bones/joints/muscles and first opaque overlapping-patch envelope copying collider coordinates. This is not the final smooth global manifold shell.
- Added opt-in ArticulatedDemo2D with one natural soil profile and gravity-loaded chain; no artificial traps, velocity/root transform propulsion, biome/AI/generations or new packages/settings. Owned partial objects/resources fail closed and are released; topology edits/removal are not anatomical development.
- Canonical Evolution_Source/Articulated mirrors into Assets/Evolution/EVCP2 under distinct assemblies. All EV-CP1 runtime files preserved; shared Built-in Resources shader reused without a duplicate key. No scene GUID/meta fabricated.
- Added eight Python reference/contract groups and four pending Unity PlayMode cases outside Assets; Roslyn now parses canonical CP2 adapters and pending tests as well as actual Assets. Expanded assembly registry, retained exact CP1 mirror and all existing tests/matrix gates.
- README and docs/EV_CP2_VALIDATION.md contain local setup, passive/active/antagonist/COM/contact/limits/shell/lifecycle checks and explicit remaining gaps. Full smooth skin, continuous anatomical change and learned locomotion remain unimplemented.
- Actual Unity API/shader compilation, Play Mode, native forces/contacts, rendering and the four new PlayMode cases have NOT been performed. This commit's CI must be verified on its own SHA.

- Pre-publication review checked target-version HingeJoint2D/point-force APIs against Unity 6000.6 reference. Corrected local-to-world attachment conversion to GetRelativePoint (GetPoint is world-to-local) and added a source regression contract. Cached the view property block rather than allocating one per muscle/frame. Rounded Inspector float bounds on conversion to strict double Core settings; no runtime anatomy rebuild.
- Executed locally before publication: 35/35 Python reference/structural groups passed; assembly JSON, csproj XML and workflow YAML parsed. New Core executed successfully in checkpoint 1 CI; checkpoint 2 expanded syntax/integration checks still require exact-HEAD CI.
