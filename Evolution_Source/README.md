# Evolution Source Workspace

This directory is the active development workspace for the game's source files before Unity integration.

## Rules

- Put all newly created or modified game code and its supporting project files inside this directory.
- Keep the source organized in focused subdirectories (for example: Code, Data, Art, Audio, Tests), but do not scatter active development files across the repository.
- Do not build, regenerate, or modify the Unity project structure in the repository root, `Assets/`, `Packages/`, or `ProjectSettings/` during this phase.
- Do not require Unity Editor to continue development. The Editor is not available to the coding agent.
- Use static checks and any available standalone tests honestly; clearly distinguish verified results from untested Unity behavior.
- When the user explicitly asks to assemble the game into a Unity project, integrate the contents of this workspace into the existing Unity project, preserve useful work, resolve dependencies, and verify what can actually be verified.

## Implemented source slice (EV-CP1)

- `Code/Core/`: engine-neutral morphology, radial geometry, contour validation and acceleration-limited velocity math.
- `Code/Unity/`: body/collider adapter, explicit morphology demo, Rigidbody2D motor, optional keyboard adapter, one static vector obstacle and opt-in runtime fixture.
- `Art/Resources/`: minimal vector shader. Temporary colors; no sprite assets.
- `Tests/Standalone/`: dependency-free .NET 8 tests against actual C# core sources.
- `Tests/verify_*.py`: independent mathematical references and structural checks.
- `Tests/UNITY_PENDING.md`: future Unity acceptance checks, explicitly not executed.

Run independent checks from repository root:

```sh
python3 -m unittest discover -s Evolution_Source/Tests -p 'verify_*.py' -v
```

With a .NET 8 SDK, run the actual core tests:

```sh
dotnet run --project Evolution_Source/Tests/Standalone/Evolution.Core.Tests.csproj
```

GitHub Actions now compiles and executes the actual C# core for all three historical
checkpoints and the current source. It also builds current core against .NET Standard
2.1 / C# 9. Runs and limitations are recorded in docs/PROGRESS.md and Tests/CI.md.
Unity adapters, shader compilation, rendering and native physics remain unverified.

Do not integrate into Unity yet. When explicitly authorized later, import Code and Art
into the existing 6000.6.10f1 project, not Tests/Standalone. The source namespace differs
from the legacy implementation. There are no serialized scene/prefab GUID references;
new .meta files can be generated on that future first import.

The optional EvolutionPrototype2D component creates its demo objects only during Play
and only when manually attached to an object. Default intent pushes right into the
single obstacle; it is a contact fixture, not autonomous intelligence. Gravity-free XY
movement is a reversible test choice, not a final camera/control design.

## Hardening and optional future Unity tests

The adapter validates float-quantized points before publication, skips unchanged mesh/
collider data and wakes an attached Rigidbody2D when geometry changes. GeometryRevision
counts actual publications and is available for diagnostics/tests, not gameplay logic.

Tests/Unity/PlayMode contains five opt-in future integration tests: exact mesh/collider
coordinates through morphing, unchanged-publication/lifecycle behavior, and moving/
morphing contact with a tall obstacle. They have NOT run or compiled against Unity.
Import them only after integration is authorized and the target project's Unity Test
Framework is available. Do not add packages or copy all Tests into Assets automatically.

## Integration Readiness lifecycle

Body and obstacle expose IsInitialized. Failed initialization disables rendering,
collision and the component, releasing partial generated resources. Fix configuration
and recreate the failed component; toggling enabled is not a retry. Normal body disable
freezes a valid shape, retaining its mesh/collider. Motor readiness is checked in the
physics callback, not between unordered Awakes. The fixture aborts partial setup.

Mesh/Material ownership is explicit: supplied assets are never destroyed; previous
render assignments/property blocks are restored only while the component still owns
the assignment. Collider paths are component-owned; teardown disables the collider.
Use dedicated objects, not an existing unrelated collider that must survive removal.

AdapterSyntax is an SDK-only standalone C# 9 syntax/contracts check, NOT Unity API
compilation. Tests/Standalone, Compatibility and AdapterSyntax stay outside Assets.
