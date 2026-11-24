using System;
using UnityEngine;

namespace Evolution.Source.Unity
{
    /// <summary>One static solid rectangle; mesh and polygon use identical local points.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(PolygonCollider2D))]
    public sealed class VectorObstacle2D : MonoBehaviour
    {
        [SerializeField] private Vector2 size = new Vector2(0.6f, 2.8f);
        [SerializeField] private Material vectorMaterial;
        // Temporary prototype color.
        [SerializeField] private Color color = new Color(0.30f, 0.32f, 0.34f, 1f);
        private Mesh mesh;
        private Material ownedMaterial;
        private MeshFilter meshFilter;

        private void Awake()
        {
            NormalizeSize();
            meshFilter = GetComponent<MeshFilter>();
            MeshRenderer renderer = GetComponent<MeshRenderer>();
            var points = new[]
            {
                new Vector2(-size.x/2, -size.y/2), new Vector2(size.x/2, -size.y/2),
                new Vector2(size.x/2, size.y/2), new Vector2(-size.x/2, size.y/2)
            };
            var vertices = new Vector3[points.Length];
            for (int i = 0; i < points.Length; i++)
                vertices[i] = new Vector3(points[i].x, points[i].y, 0f);
            mesh = new Mesh { name = "Evolution Source Obstacle", hideFlags = HideFlags.DontSave };
            mesh.vertices = vertices;
            mesh.triangles = new[] { 0, 1, 2, 0, 2, 3 };
            mesh.RecalculateBounds();
            meshFilter.sharedMesh = mesh;
            if (vectorMaterial == null)
            {
                Shader shader = Resources.Load<Shader>("EvolutionVector2D");
                if (shader == null) throw new InvalidOperationException("EvolutionVector2D shader missing.");
                ownedMaterial = new Material(shader) { hideFlags = HideFlags.DontSave };
            }
            renderer.sharedMaterial = vectorMaterial != null ? vectorMaterial : ownedMaterial;
            var properties = new MaterialPropertyBlock();
            properties.SetColor("_Color", color);
            renderer.SetPropertyBlock(properties);
            PolygonCollider2D polygon = GetComponent<PolygonCollider2D>();
            polygon.offset = Vector2.zero;
            polygon.isTrigger = false;
            polygon.pathCount = 1;
            polygon.SetPath(0, points);
        }

        private void OnValidate() => NormalizeSize();
        private void NormalizeSize()
        {
            size.x = float.IsNaN(size.x) || float.IsInfinity(size.x) ? .6f : Mathf.Clamp(size.x, .1f, 10f);
            size.y = float.IsNaN(size.y) || float.IsInfinity(size.y) ? 2.8f : Mathf.Clamp(size.y, .1f, 10f);
        }

        private void OnDestroy()
        {
            if (meshFilter != null && meshFilter.sharedMesh == mesh) meshFilter.sharedMesh = null;
            if (mesh != null) Destroy(mesh);
            if (ownedMaterial != null) Destroy(ownedMaterial);
        }
    }
}
