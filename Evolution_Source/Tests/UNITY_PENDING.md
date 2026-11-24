# Unity checks — NOT EXECUTED

Do not integrate or run these now. After the user explicitly authorizes integration
into Unity **6000.6.10f1**, copy only Code and Art to the existing project (preserving
future .meta files). Keep Tests/Standalone outside Assets; it is a .NET console app.

In an empty scene, manually attach EvolutionPrototype2D to one empty GameObject.
No Editor menu, auto-import scene generation or project-setting changes are used.

Acceptance checks after integration:
- Import/compile C# and EvolutionVector2D shader; inspect Console.
- Default fixture: one creature moves right and meets one solid vector obstacle.
  It must not tunnel or cross the rectangle, including during shape changes.
- Run at 30/60/144 render FPS with the same physics timestep. Inspect contacts and jitter.
- Across at least 120 seconds, compare Mesh.vertices[i+1].xy with
  PolygonCollider2D.GetPath(0)[i] (exact float equality), pathCount=1, offset=(0,0),
  and stable vertex/index counts. Check CCW winding and bounds.
- Verify no sprite swapping, contour gaps or self-intersections; check extreme targets.
- Toggle CreatureBody2D off/on: no morphology reset. Editing response/color in Play Mode
  must not reinitialize shape. Resolution and initial-shape fields are initialization-only.
- The demo intentionally drives into the obstacle; it has no avoidance or learning.
  Temporarily disable MorphologyDemo2D before testing manually supplied morphology targets.
- For keyboard testing enable Use Keyboard Controls on the fixture before Play.
  WASD/arrows require the built-in legacy input backend. If unavailable, the adapter
  emits a warning and disables itself; use SetMovementIntent from the existing input
  integration. Do not install a package/change settings just for this fixture.
- Disable the fixture: its owned root must become inactive. Re-enable: automatic intent resumes.
- Destroy the fixture and check generated meshes/materials/objects are released.

Open risks: collider resizing may inject contact corrections, motor/contact interaction,
fixed-step silhouette smoothness and shader/API compatibility must be assessed in Unity.
These checks have not passed merely because the Python reference passes.

## Optional executable PlayMode suite (not run)

After explicit integration approval, and only if Unity Test Framework is available,
import Tests/Unity/PlayMode separately with its asmdef. It is gated by
UNITY_INCLUDE_TESTS; do not copy Tests/Standalone or Tests/Compatibility into Assets.
Run CreatureIntegrationTests in PlayMode with the normal FixedUpdate physics mode.
The tests cover 120 morph frames of exact Mesh/PolygonCollider equality, unchanged
publication and re-enable behavior, and 200 moving/morphing contact steps plus braking.
They do not replace manual 30/60/144 FPS or 120-second visual/contact inspection.

Shape-growth depenetration and any resulting contact impulses remain a Unity risk.
WakeUp and avoiding unnecessary SetPath calls reduce avoidable contact-state churn;
they are not a claim that resizing contacts or CCD have been validated.

## Integration Readiness additions — not executed

Five PlayMode tests are now prepared. The two added cases cover owned obstacle
mesh/material teardown after component removal and waking a Rigidbody2D attached after
body Start. All five still require compilation/execution in the target Unity version.

Also inject an invalid custom material without _Color (or temporarily omit the resource
shader) on an inactive test object before activation: expect one initialization error,
IsInitialized=false, disabled render/collision and released partial resources. Re-enable
must not create a half-initialized component or repeat exceptions every physics tick.
Verify assigned external materials are not destroyed and previous property blocks and
render slots are restored on teardown. These fault cases remain manual/pending Unity.
