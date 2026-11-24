# HANDOFF — EV-CP1 / First Morphology Prototype

**Repository:** https://github.com/maakeeuu/Evolution  
**Branch:** `main`  
**Current base commit:** `f7d40ba1400ac17d83503ae9bf3fdffbb742384c`  
**Engine target:** Unity 6000.6.10f1  
**Stage:** code committed; Unity Editor validation pending.

## What is in the repository
- `MASTER_PROMPT_RU.md` — main project prompt, Russian.
- `AGENTS.md`, `CLAUDE.md` — shared model instructions.
- `ProjectSettings/ProjectVersion.txt` and `Packages/manifest.json`.
- `Assets/Evolution/Scripts/Creature/ProceduralCreature2D.cs` — fixed-resolution radial contour, parameter interpolation, dynamic mesh, separate fill/outline materials, PolygonCollider2D generated from the same contour.
- `Assets/Editor/EvolutionDemoSceneBuilder.cs` — menu command **Tools → Evolution → Create Morphology Demo**, creates an orthographic test scene at `Assets/Evolution/Scenes/EV_MorphologyPrototype.unity`.
- `docs/PROGRESS.md`, `docs/DECISIONS.md`, `docs/BUILD_TODAY.md`, `docs/SKILLS_TO_ADD.md`.
- Impeccable submodule pinned at commit `b49177d32788f2dc585c42831d845dacf1ebed2c`.

## Implemented behaviour
One organism is generated from a closed contour with fixed vertex count (default 64; configurable 24–128). Continuous parameters control width, height and radial biases. The current values move toward target values at a bounded rate. A stable triangle fan fills the contour. The same sampled contour is passed to `PolygonCollider2D`; a separate `LineRenderer` draws the outline.

The automatic slow parameter wave is only a visual/geometry demo. **It is not learning, memory, biological adaptation or a finished game mechanic.**

## Checks actually performed
- GitHub tree and the committed source files were fetched and verified.
- C# source braces are balanced and the mesh/collider update paths are present.
- A separate Python check mirrored the radial contour formula and tested 32 combinations: all allowed extreme parameter combinations at resolutions 24, 32, 64 and 128 produced positive polygon area and no proper intersections between non-adjacent edges.
- **Not done:** C# compile, Unity import, opening the scene, Play Mode run, collider inspection in Unity, or EditMode/PlayMode tests. Do not claim these passed.

## Exact next action
1. Pull the latest `main` (base SHA above; this handoff commit will be newer).
2. Open the repository root in **Unity 6000.6.10f1**.
3. Wait for script compilation. If errors appear, capture the first full Console errors and fix them before adding systems.
4. Run **Tools → Evolution → Create Morphology Demo**.
5. Press Play and inspect shape smoothness, outline, fill material and collider in the scene.
6. Report real results and commit any fixes. Do not start learning AI until this shape prototype runs.

## Scope discipline
- The user wants a playable game quickly, not a research phase.
- Stay strictly 2D.
- Do not add generations, reproduction, inheritance, complex weather/water, world generation, large HUD or neural networks yet.
- Keep behavioural learning, morphological adaptation and evolution across generations as separate systems.
- The user will add the five recommended Unity/AI skills manually; they are listed in `docs/SKILLS_TO_ADD.md`. Do not delay the current validation waiting for them.
- Main implementation model: ChatGPT 6.1 Sol (larger available context budget). Use Claude Max Effort for targeted review, not concurrent editing of the same files.
- Do not touch the separate PEE repository.

## Suggested first message for the next model
"Прочитай AGENTS.md, MASTER_PROMPT_RU.md, docs/PROGRESS.md и этот handoff. Не проводи длительное исследование. Сначала проверь Unity-проект в 6000.6.10f1: компиляцию, создание сцены через Tools → Evolution → Create Morphology Demo и Play Mode. Не начинай новые системы, пока не подтверждена стабильная непрерывная форма и PolygonCollider2D. Исправляй ошибки маленькими коммитами и обновляй прогресс."
