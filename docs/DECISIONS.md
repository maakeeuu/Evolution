# Decisions — Evolution

## Confirmed
- Unity 6000.6.10f1.
- Strictly 2D.
- Procedural vector creature.
- Morphology must change gradually and seamlessly.
- The creature learns from the consequences of its actions and adapts.
- Minimalist environment: obstacles, water, weather effects.
- The active game view should have almost no HUD.

## Temporary prototype choices
- First body uses a closed radial contour with a fixed number of vertices.
- Demo changes a small number of continuous shape parameters slowly. This is a geometry demonstration, **not** biological adaptation or learning.
- Prototype palette is temporary.

## Open
Camera view, player control model, creature type, water/weather details, generations/inheritance, death rules, long-term goal.

## Source-only EV-CP1 choices — 2026-11-24
- Active code lives in Evolution_Source until explicit Unity integration approval; root Unity folders remain untouched.
- The contact fixture temporarily uses gravity-free XY motion and one rectangular obstacle. Camera/control model remains open; this does not commit the final game to a top-down design.
- Use one fill silhouette (no outward outline) so visual boundary and PolygonCollider2D use the same coordinates. A small local unlit shader avoids new rendering packages.
- MorphologyDemo2D is a demonstration driver only. Target interpolation is not an adaptation policy; motion intent is not learning; no generation/inheritance system is added in EV-CP1.


## Integration Readiness lifecycle contract — 2026-11-24
- Body and obstacle own their generated geometry and collider path while attached. Use dedicated prototype objects; externally supplied Mesh/Material assets are never destroyed. Previous render slots/property blocks are restored only if the component's assignment is still present; teardown disables its collider rather than restoring an unknown prior collider path.
- Initialization failure is fail-closed: rendering/collision disabled, partial owned resources released, component disabled, one error. Recreate the component after fixing configuration; re-enable is not an implicit retry. A normal disable/re-enable preserves the valid shape.
- Motor readiness is evaluated in FixedUpdate after Awake ordering has settled. Late-added Rigidbody2D is resolved when geometry next changes.

- Independent adapter verification uses actual Roslyn C# 9 parsing with SDK-provided compiler assemblies, not Unity stubs. This is a syntax/contract gate only; Unity API binding and native behavior remain explicitly pending.
- CI runs for progress/decisions-only checkpoints as well as source changes and verifies the checked-out SHA so the reported main HEAD always has its own run.
