# EV-CP3 — baseline and longitudinal skin validation

Status: baseline prepared; native Unity acceptance pending. Exact historical approved rod geometry was NOT found in published main. The old radial prototype is not the target.

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
