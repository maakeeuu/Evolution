using System;
using Evolution.Source.Core;
using UnityEngine;

namespace Evolution.Source.Unity
{
    /// <summary>
    /// Source-only replacement, not an in-place modification of the legacy component.
    /// Fixed topology is captured in Awake. One float contour drives fill and collision.
    /// Not ExecuteAlways: validation must not allocate native objects in the editor.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(PolygonCollider2D))]
    public sealed class CreatureBody2D : MonoBehaviour
    {
        [SerializeField, Range(24, 128)] private int resolution = 64;
        [SerializeField, Range(0.15f, 2.5f)] private float initialWidth = 0.85f;
        [SerializeField, Range(0.15f, 2f)] private float initialHeight = 0.58f;
        [SerializeField, Range(-0.16f, 0.16f)] private float initialForwardBias = 0.035f;
        [SerializeField, Range(-0.16f, 0.16f)] private float initialDorsalBias = 0.045f;
        [SerializeField, Range(-0.14f, 0.14f)] private float initialLateralLobes = 0.025f;
        [SerializeField, Range(0.01f, 2f)] private float response = 0.8f;
        [SerializeField] private Material vectorMaterial;
        // Temporary prototype color, not a final palette.
        [SerializeField] private Color bodyColor = new Color(0.20f, 0.47f, 0.43f, 1f);

        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private PolygonCollider2D polygon;
        private Rigidbody2D physicsBody;
        private Mesh mesh;
        private Material ownedMaterial;
        private MaterialPropertyBlock properties;
        private Point2[] contour;
        private Vector2[] physicsContour;
        private Vector3[] vertices;
        private Morphology current;
        private Morphology target;
        private bool initialized;
        private bool hasPendingTarget;
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        public Morphology CurrentMorphology => current;
        public uint GeometryRevision { get; private set; }

        private void Start() => physicsBody = GetComponent<Rigidbody2D>();

        private void Awake()
        {
            NormalizeSettings();
            meshFilter = GetComponent<MeshFilter>();
            meshRenderer = GetComponent<MeshRenderer>();
            polygon = GetComponent<PolygonCollider2D>();
            current = new Morphology(initialWidth, initialHeight, initialForwardBias,
                initialDorsalBias, initialLateralLobes);
            if (!hasPendingTarget) target = current;
            contour = new Point2[resolution];
            physicsContour = new Vector2[resolution];
            vertices = new Vector3[resolution + 1];
            mesh = new Mesh { name = "Evolution Source Creature", hideFlags = HideFlags.DontSave };
            mesh.MarkDynamic();
            meshFilter.sharedMesh = mesh;
            properties = new MaterialPropertyBlock();
            meshRenderer.sharedMaterial = ResolveMaterial();
            meshRenderer.sortingOrder = 10;
            polygon.offset = Vector2.zero;
            polygon.isTrigger = false;
            polygon.pathCount = 1;
            Publish(current, true);
            initialized = true;
        }

        public void SetMorphologyTarget(Morphology value)
        {
            target = value.Validated();
            hasPendingTarget = true;
        }

        private void FixedUpdate()
        {
            if (!initialized) return;
            Morphology next = Morphology.Approach(current, target, response, Time.fixedDeltaTime);
            // Validate before publishing anything; keep the previous geometry on failure.
            Publish(next, false);
            current = next;
        }

        private void Publish(Morphology shape, bool first)
        {
            RadialGeometry.FillFloatCompatible(shape, contour);
            bool changed = first;
            for (int i = 0; i < contour.Length && !changed; i++)
                changed = contour[i].X != physicsContour[i].x || contour[i].Y != physicsContour[i].y;
            if (!changed)
            {
                ApplyColor();
                return;
            }
            if (!ContourValidator.IsValidFan(contour))
            {
                enabled = false;
                throw new InvalidOperationException("Invalid creature contour; previous geometry retained.");
            }
            vertices[0] = Vector3.zero;
            for (int i = 0; i < contour.Length; i++)
            {
                physicsContour[i] = new Vector2((float)contour[i].X, (float)contour[i].Y);
                // Exact same float coordinates, no independent render sampling or smoothing.
                vertices[i + 1] = new Vector3(physicsContour[i].x, physicsContour[i].y, 0f);
            }
            mesh.vertices = vertices;
            if (first) mesh.triangles = RadialGeometry.CreateFan(contour.Length);
            mesh.RecalculateBounds();
            polygon.SetPath(0, physicsContour);
            GeometryRevision++;
            if (physicsBody != null) physicsBody.WakeUp();
            ApplyColor();
        }

        private void ApplyColor()
        {
            meshRenderer.GetPropertyBlock(properties);
            properties.SetColor(ColorId, bodyColor);
            meshRenderer.SetPropertyBlock(properties);
            // Unlit shader needs no per-frame normals or UV generation.
        }

        private Material ResolveMaterial()
        {
            if (vectorMaterial != null) return vectorMaterial;
            Shader shader = Resources.Load<Shader>("EvolutionVector2D");
            if (shader == null)
                throw new InvalidOperationException("Include Art/Resources/EvolutionVector2D.shader at integration.");
            ownedMaterial = new Material(shader)
            {
                name = "Evolution Source Vector Material", hideFlags = HideFlags.DontSave
            };
            return ownedMaterial;
        }

        private void OnValidate()
        {
            // Data normalization only. Never reset live shape, resize topology or create objects here.
            NormalizeSettings();
        }

        private void NormalizeSettings()
        {
            resolution = Mathf.Clamp(resolution, 24, 128);
            response = FiniteClamp(response, 0.01f, 2f, 0.8f);
            initialWidth = FiniteClamp(initialWidth, 0.15f, 2.5f, 0.85f);
            initialHeight = FiniteClamp(initialHeight, 0.15f, 2f, 0.58f);
            initialForwardBias = FiniteClamp(initialForwardBias, -0.16f, 0.16f, 0.035f);
            initialDorsalBias = FiniteClamp(initialDorsalBias, -0.16f, 0.16f, 0.045f);
            initialLateralLobes = FiniteClamp(initialLateralLobes, -0.14f, 0.14f, 0.025f);
        }

        private static float FiniteClamp(float value, float min, float max, float fallback) =>
            float.IsNaN(value) || float.IsInfinity(value) ? fallback : Mathf.Clamp(value, min, max);

        private void OnDestroy()
        {
            if (meshFilter != null && meshFilter.sharedMesh == mesh) meshFilter.sharedMesh = null;
            if (polygon != null) polygon.enabled = false;
            if (mesh != null) Destroy(mesh);
            if (ownedMaterial != null) Destroy(ownedMaterial);
        }
    }
}
