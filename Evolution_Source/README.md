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

Only the Python checks were executed in the current development environment.
C# compiler, Unity import, shader compilation, rendering and physics remain unverified.

Do not integrate into Unity yet. When explicitly authorized later, import Code and Art
into the existing 6000.6.10f1 project, not Tests/Standalone. The source namespace differs
from the legacy implementation. There are no serialized scene/prefab GUID references;
new .meta files can be generated on that future first import.

The optional EvolutionPrototype2D component creates its demo objects only during Play
and only when manually attached to an object. Default intent pushes right into the
single obstacle; it is a contact fixture, not autonomous intelligence. Gravity-free XY
movement is a reversible test choice, not a final camera/control design.
