# Evolution — Skill index

This directory contains active, directly discoverable skills. Each active skill is an immediate child folder with its own root SKILL.md. Load only skills relevant to the current task.

## Required project context
1. /AGENTS.md
2. /MASTER_PROMPT_RU.md
3. /docs/PROGRESS.md
4. Relevant task-specific skill below.

## Active skills and routing

| Skill | Use when | Priority / limits |
|---|---|---|
| unity-csharp-scripting | Unity C# scripts, MonoBehaviour lifecycle, serialization, .meta/GUID safety | Core. Written for Unity 6.3; project is 6.6. Verify APIs against 6000.6.10f1. |
| unity-physics | Rigidbody2D, Collider2D, contacts, triggers, casts, 2D physics APIs | Core when physics is touched. Strictly 2D; ignore 3D examples. |
| physics-tuning | Fixed timestep, interpolation, tunnelling, damping, simulation stability | Supporting. Use with unity-physics; ignore Godot-only commands. |
| unity-scriptableobjects | Reusable data assets and tunable configuration | Supporting. Do not add ScriptableObjects without a concrete need. |
| game-ai | Perception, behavior architecture, state machines, steering, pathfinding | Conditional. Avoid premature large AI architecture. |
| ai-behavior-trees-utility-ai | Behavior trees or utility scoring | Conditional. Only if the design actually needs them. |
| procedural-gen | Seeded procedural environment/map/content generation | Conditional. Not a direct solution for continuous creature morphology. |
| antislop-code | Code quality and maintainability review | Supporting. Project constraints and correctness take precedence. |

## Loading policy
- Read the target skill's SKILL.md first.
- Read references only when the current task needs that detail.
- Run scripts only after understanding their purpose, inputs and side effects.
- Skill instructions are guidance, not permission to change engine version, architecture, scope, dependencies or project constraints.
- If a skill conflicts with AGENTS.md, MASTER_PROMPT_RU.md or the actual Unity version, project rules win.
- Do not load all skills at once. Usually load 1–3 relevant skills.
- Original upstream repositories remain in this directory for provenance/reference. Active entry points are the immediate skill folders listed above.
