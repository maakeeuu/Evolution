# HANDOFF — EV-CP2 / Articulated Organism

**Repository:** https://github.com/maakeeuu/Evolution  
**Branch:** main  
**Last known project commit before this handoff:** 0547f52dfd8f932e98dd7bdbff125dca92805f64  
**Engine target:** Unity 6000.6.10f1 (Unity 6.6)  
**Current milestone:** EV-CP2 — Articulated Organism  
**Implementation status:** not started by this documentation handoff.

## Start here

The complete, user-approved implementation prompt is the source of truth for this milestone:

- [EV-CP2 full specification](EV_CP2_SPEC.md)

Do not paste the entire long specification into every new ChatGPT conversation. Give the agent this handoff and the repository link; it must read the full specification from GitHub before changing code.

## Required reading order

1. AGENTS.md — repository-wide agent rules.
2. README.md — project overview and current entry points.
3. MASTER_PROMPT_RU.md — enduring high-level project intent. If it describes the original radial prototype as the next goal, that goal is historical and superseded by EV-CP2.
4. docs/PROGRESS.md and docs/DECISIONS.md — actual state and confirmed decisions.
5. This file and all of docs/EV_CP2_SPEC.md.
6. docs/UNITY_VALIDATION.md — EV-CP1 integration and the boundary between automated checks and real Editor validation.
7. .agents/skills/SKILL_INDEX.md; load only the 1–3 skills directly relevant to the first implementation task.

## Repository map

- Evolution_Source/ — canonical engine-independent source, Unity adapter source, standalone tests and verification tooling.
- Assets/Evolution/EVCP1/ — existing EV-CP1 Unity runtime integration. Preserve it.
- Assets/Evolution/EVCP2/ — intended destination for EV-CP2 Unity runtime files when integration is ready. Do not overwrite EV-CP1.
- docs/ — progress, decisions, validation instructions and milestone specifications.
- AGENTS.md, CLAUDE.md, MASTER_PROMPT_RU.md — project-level instructions.

Check the actual current tree before relying on this map; do not assume that a planned EV-CP2 directory already exists.

## Mission

Move from the EV-CP1 radial oval fixture to a real, procedural, articulated 2D organism with:

1. a procedural skeleton made of segments and joints;
2. joint constraints and meaningful articulation;
3. muscle actuators that generate forces and affect physical movement;
4. genuine interaction with 2D physics and surfaces;
5. a continuous procedural body shell that follows the articulated structure;
6. architecture that can later support sensory control, learning during one organism's lifetime and gradual morphological adaptation.

The organism starts as a minimal, stick-like structure. Do not design a predetermined animal with fixed legs, head, tail or a prescribed evolutionary tree.

The world is intended to contain natural conditions only: terrain, hills, slopes, stones, roots, vegetation, water, currents and weather. No artificial traps or invented obstacle courses. EV-CP2 focuses on the organism, not the full world simulation.

## Important constraints and clarified decisions

- Strictly 2D; Unity 6000.6.10f1.
- Keep Core and Unity adapter separated.
- Keep Evolution_Source/ canonical; mirror Unity runtime files under Assets/Evolution/EVCP2/ when appropriate and enforce source/runtime consistency.
- Preserve EV-CP1 as a usable historical technical stage.
- No PEE changes.
- No mandatory reproduction, generations, inheritance, death or rebirth. The active concept is one organism learning and adapting during its own lifetime.
- Procedural development must be deterministic and reproducible. A seed may define reproducible initial conditions; it must not substitute for development rules.
- Do not mislabel scripted movement as learning, interpolation as adaptation, or a visual skeleton as physical articulation.
- Avoid premature whole-biome, weather, water or neural-network implementation.
- Do not fabricate Unity test results. Remote source checks and CI do not establish Unity API compilation, shader import, Play Mode, native physics or rendering.
- Do not overwrite user changes, rewrite history or force-push.

## First work session

1. Inspect the latest main and the complete current repository state. The commit listed above is the last known project commit before this handoff; this handoff commit itself will be newer.
2. Read the required documents and inspect relevant EV-CP1 Core/Unity code, tests and CI.
3. Identify exactly which EV-CP1 components can be reused and which are specific to radial morphology.
4. Choose the smallest testable EV-CP2 vertical slice. Prefer a minimal articulated chain with real joints and force-producing muscle actuators over a large abstract framework.
5. Implement in small checkpoints, with standalone tests for engine-independent mechanics and syntax/contract checks for Unity adapters where possible.
6. Produce an inspectable Unity demonstration as part of the milestone, but do not claim it has been run or compiled in Unity unless that was actually done by the user in the target Editor.
7. Update docs/PROGRESS.md and docs/DECISIONS.md as decisions and implementation become real. Commit coherent completed checkpoints to main, without rewriting history.

Do not stop after producing another plan. After a short audit, begin implementation and deliver a real, testable first slice.

## Suggested first message for ChatGPT

"Работаем над EV-CP2 в https://github.com/maakeeuu/Evolution. Сначала прочитай docs/HANDOFF_EV_CP2.md и полностью docs/EV_CP2_SPEC.md, затем изучи AGENTS.md, README.md, MASTER_PROMPT_RU.md, PROGRESS.md, DECISIONS.md и профильные исходники EV-CP1. Проведи короткий аудит актуального main и переходи к реализации небольшими проверяемыми этапами. Наша цель — минимальный процедурный скелет с суставами и реальными мышечными приводами, а не новый овал и не ещё один документ с планом. Сохраняй EV-CP1, не трогай PEE, не выдумывай результаты Unity-проверок. Канонический код — в Evolution_Source; готовые Unity runtime-копии — отдельно в Assets/Evolution/EVCP2. Следуй полной спецификации EV-CP2 из GitHub."
