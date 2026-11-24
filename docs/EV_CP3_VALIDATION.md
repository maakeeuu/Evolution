# EV-CP3 — baseline and longitudinal skin validation

Status: baseline and longitudinal skin implemented at source level; native Unity acceptance pending. Exact historical approved rod geometry was NOT found in published main. The old radial prototype is not the target.

## Baseline pinned before geometry changes
Base: bf1e333ea78b0b225b5fd0d7a25b7c4d85004dac.
ArticulatedSceneBuilder -> ArticulatedDemo2D -> ArticulatedOrganism2D + ArticulatedOrganismView2D + MuscleWaveDriver2D + ArticulationDiagnostics2D. No radial component is created on this path. Legacy Tools/Evolution/Create Morphology Demo is a different, radial fixture and is not EV-CP3.

The baseline View builds one opaque capsule patch from each SegmentContour / PolygonCollider2D, using a per-patch fan. Centerline spans sum(.85*.96^i), i=0..3 = 3.2013856 m. Radii .14*.96^i, masses .35*.96^(2i); hinge limits +/-40 deg, motors disabled, neighboring collisions ignored. Muscle anchors: longitudinal quarter-length, lateral .72 radius; settings 60 N / 600 N/m / 6 Ns/m / contraction .3; activation rate 8 /s. No physical parameters are changed by EV-CP3.

Straight ideal extent with capsule caps = 3.46524864 m, maximum diameter .28 m, ratio 12.375888. This source calculation does NOT identify why a user's actual scene appeared as a large oval. Scene/Inspector configuration and native rendering require local inspection.

Camera is orthographic, size 3.5, at (0,1,-10); organism starts at (-1.8,1,0) over the existing soil profile. Unit scale required by physics adapter. RodDemo2D blocks its own fixture if a legacy radial component exists in that scene; it never deletes user objects. Inspector baseline reads actual initialized bodies/joints and reports the component/mode. Native scene contents have not been observed here.

## Open locally
Unity 6000.6.10f1 -> Tools -> Evolution -> Create/Open EV-CP3 Rod Demo. The Editor saves Assets/Evolution/EVCP2/Scenes/EVCP3_Rod.unity only if missing; existing scenes open unchanged. No scene/.meta/GUID is fabricated remotely. Save dialogs/cancel, CP2 menus, time ownership and Build Settings behavior are preserved.

Press Play. Modes: Normal, Shell only, Bones/colliders, Joints/muscles. Wave drive is initially off: restart Play for the fresh straight/rest initial state; passive gravity can subsequently change that pose. Enable Wave for physical muscle commands, not a scripted bend or pose reset. Space pauses the actual simulation; 1/2/3/4 select 1/2/5/20x. Inspect RodDemo2D baseline and optional ArticulationDiagnostics2D. Capture shell-only straight and physically bent states plus collider inspection.

## Native gates — all PENDING
- Actual C#/shader import, scene generation, serialization, Console and resource lifecycle.
- Run the isolated CP2 A/B/C/D joint probe first; it is not full skin acceptance.
- Confirm real opposite hinge deflections and surface articulation; no static pose overlay.
- Judge long narrow silhouette straight/bent, continuous skin and no inflated joints.
- Confirm collider/skin discrepancy readings and inspect overlays at bend extrema.
- Pause/resume, speed controls, cleanup/re-enable, non-neighbor self-contact.
- Screenshots/video must come from real Unity Play Mode; no such capture exists here.


## Longitudinal geometry (replacement checkpoint)
- Canonical Core/RodSkinGeometry.cs owns reusable, fixed-count buffers. Unity/ArticulatedRodSkin2D.cs reads current body transforms and actual HingeJoint2D anchor/connectedAnchor endpoints in one render coordinate frame. It never writes physical state.
- Cross sections are lower/upper pairs. At joints, the shared axis point is the midpoint of the two anchors ONLY within the explicit .02 m gap tolerance; larger gaps are reported as invalid, never stretched over silently. Segment length mismatch and angles beyond the anatomical limit +2 deg (absolute cap 62 deg) also reject publication.
- Width comes from endpoint anatomical radii; a seam uses the larger adjacent radius. The bisector miter multiplier is 1/cos(turn/2), bounded by 1.2. It is 1.064177772 at 40 deg and 1.154700538 at 60 deg. No clamped/inflated circular joint nodes are added.
- Each terminal cap adds five transverse sections at 5/22/39/56/73 degrees before the full-width endpoint. Extension is < one anatomical radius. The tiny flat terminal section avoids duplicate tip vertices and degenerate triangles; this is not an exact circular cap.
- For the default four segments: 15 sections, 30 vertices, 28 CCW triangles. Every pair of adjacent sections shares vertices; there are no separate segment body patches. Indices are allocated once and unchanged on pose updates. Exact float-published coordinates are checked for positive triangle area, winding and non-adjacent boundary intersections.
- The View now owns diagnostics only (bones, collider outlines, small joint markers, muscle lines). Body fill comes exclusively from one strip Mesh. Small diagnostic joint marker disks are not body skin.
- Normal shows skin over the existing environment; Shell only hides soil rendering, not its collider. Bones/colliders and Joints/muscles expose the same actual organism without skin fill. The read-only skin still updates and measures in inspection modes.

## Collider / skin discrepancy
No physical blob changed: ArticulatedOrganism2D, ChainDefinition, MuscleMechanics and SegmentContour are pinned byte-for-byte by CI. EV-CP1's complete Git tree is pinned too. Existing collider radius, mass, force, anchor arms and limits remain unchanged.

ArticulatedRodSkin2D reports two sampled directed solid-coverage distances, in metres, every .5 unscaled second:
1. Visible skin boundary outside the UNION of actual collider polygons.
2. Actual collider polygon boundary outside the filled skin.

Eight samples per edge; 1,328 samples for the default four-segment strip/collider topology. Paths and collider offsets are read from the actual colliders and transformed by the same interpolated render poses used for skin. Internal overlap boundaries are not incorrectly interpreted as an exposed union boundary; point-to-solid distance is zero inside. These sampled values are NOT an exact continuous Hausdorff bound, not a native contact/solver measurement, and not proof of equality. Angular smoothing and taper can create small measurable differences; overlays remain available. A value above .25 times the smallest anatomical radius raises a visible Inspector/Console coverage warning. Invalid/self-intersecting geometry hides only its own skin and reports the reason; it does not alter or stop physics.

## Automated evidence / limits
- Baseline commit c6412a6a266a8e90b45793c652628733be7f9c68: exact-HEAD CI run 45450749392 completed/success before skin replacement.
- Local Python reference/structure suite: 46 tests pass, including 91 independent straight/C/S/extreme reference configurations, unchanged physical blobs, exact preserved EV-CP1 tree and both source mirrors.
- Actual C# tests add 107 pose configurations, exact repeatability, stable strip topology/manifold edge counts, positive triangles, independent proper-intersection checks, widths/axis following, changed-pose geometry, float conversion, invalid inputs, excessive gaps/turns and a self-intersecting loop. Default-anatomy sampled discrepancy is gated at < .025 m in both directions. New exact-HEAD C# CI results must be read after publication.
- Two additional PlayMode tests are PREPARED, NOT RUN: synchronous skin refresh must preserve native positions/velocities/masses/collider paths/joints/actuator state; real muscle-driven hinge deflection must change the actual published skin Mesh. Existing nine CP2 native cases remain pending too.
- No packages/Unity/ProjectSettings changes, no CP1 modification, no PEE work. No force push or rewritten history.
- No native screenshots, scene serialization, Unity API binding, rendering, physics, resource profiling or Build was executed here. Arbitrary self-contact/closed-loop poses are not supported by this single simple strip and are rejected rather than geometrically untangled. The metrics sampler and diagnostic allocations need local profiling; 20x throughput is not promised.

EV-CP3 remains NOT ACCEPTED until target-Editor straight/bent shell-only visual acceptance and actual physical articulation are observed.
