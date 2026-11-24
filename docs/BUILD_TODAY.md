# Build Today — first playable slice

Keep the scope narrow and deliver something visible.

1. Generate one closed vector contour with fixed resolution.
2. Interpolate morphology parameters gradually with bounded rates.
3. Validate contour closure, winding, triangle indices, and shape bounds.
4. Update PolygonCollider2D from the exact contour used by the render mesh.
5. Provide a minimal orthographic 2D scene.
6. Run it in Unity and inspect for visual jumps, gaps, flicker, self-intersection, and collider mismatch.
7. Only after this works, add basic movement and one obstacle.

Not in this slice: learning AI, large world generation, water/weather simulation, breeding, inheritance, save system, menus, or complex HUD.