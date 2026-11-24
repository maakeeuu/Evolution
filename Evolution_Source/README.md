# Evolution Source Workspace

This directory is the active development workspace for the game's source files before Unity integration.

## Rules

- Put all newly created or modified game code and its supporting project files inside this directory.
- Keep the source organized in focused subdirectories (for example: Code, Data, Art, Audio, Tests), but do not scatter active development files across the repository.
- Do not build, regenerate, or modify the Unity project structure in the repository root, `Assets/`, `Packages/`, or `ProjectSettings/` during this phase.
- Do not require Unity Editor to continue development. The Editor is not available to the coding agent.
- Use static checks and any available standalone tests honestly; clearly distinguish verified results from untested Unity behavior.
- When the user explicitly asks to assemble the game into a Unity project, integrate the contents of this workspace into the existing Unity project, preserve useful work, resolve dependencies, and verify what can actually be verified.
