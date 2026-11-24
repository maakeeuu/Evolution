using UnityEngine;

namespace Evolution.Creatures
{
    /// <summary>
    /// Minimal procedural 2D creature.
    /// One closed contour drives both the filled mesh and PolygonCollider2D.
    /// The automatic morph is a geometry demonstration, not a learning algorithm.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer),
        typeof(PolygonCollider2D), typeof(LineRenderer))]
    public sealed class ProceduralCreature2D : MonoBehaviour
    {
        [System.Serializable]
        public struct Morphology
        {
            [Min(0.1f)] public float width;
            [Min(0.1f)] public float height;
            [Range(-0.16f, 0.16f)] public float forwardBias;
            [Range(-0.16f, 0.16f)] public float dorsalBias;
            [Range(-0.14f, 0.14f)] public float lateralLobes;

            public static Morphology Default => new Morphology
            {
                width = 0.85f,
                height = 0.58f,
                forwardBias = 0.035f,
                dorsalBias = 0.045f,
                lateralLobes = 0.025f
            };
        }

        [Header("Geometry")]
        [SerializeField, Range(24, 128)] private int contourResolution = 64;
        [SerializeField] private Morphology initialMorphology = new Morphology
        {
            width = 0.85f, height = 0.58f,
            forwardBias = 0.035f, dorsalBias = 0.045f, lateralLobes = 0.025f
        };

        [Header("Continuous Morphology")]
        [SerializeField, Min(0.01f)] private float morphRate = 0.24f;
        [SerializeField] private bool runMorphologyDemo = true;
        [SerializeField] private Color bodyColor = new Color(0.20f, 0.47f, 0.43f, 1f);
        [SerializeField] private Color outlineColor = new Color(0.075f, 0.16f, 0.16f, 1f);
        [SerializeField, Range(0.005f, 0.08f)] private float outlineWidth = 0.025f;

        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        private PolygonCollider2D _polygonCollider;
        private LineRenderer _lineRenderer;
        private Mesh _mesh;
        private Material _material;
        private Vector3[] _vertices;
        private Vector2[] _contour;
        private int[] _triangles;
        private Morphology _current;
        private Morphology _target;
        private Morphology _demoBase;
        private bool _initialized;

        public Morphology CurrentMorphology => _current;

        private void OnEnable()
        {
            EnsureComponents();
            _current = Clamp(initialMorphology);
            _target = _current;
            _demoBase = _current;
            _initialized = true;
            RebuildGeometry();
        }

        private void Update()
        {
            if (!_initialized)
                return;

            if (Application.isPlaying && runMorphologyDemo)
                UpdateDemoTarget();

            float dt = Application.isPlaying ? Time.deltaTime : 0f;
            if (dt <= 0f)
                return;

            _current = MoveTowards(_current, _target, morphRate * dt);
            RebuildGeometry();
        }

        private void OnValidate()
        {
            contourResolution = Mathf.Clamp(contourResolution, 24, 128);
            morphRate = Mathf.Max(0.01f, morphRate);
            initialMorphology = Clamp(initialMorphology);

            if (!isActiveAndEnabled)
                return;

            EnsureComponents();
            _current = Clamp(initialMorphology);
            _target = _current;
            _demoBase = _current;
            _initialized = true;
            RebuildGeometry();
        }

        /// <summary>Sets a new long-term morphology target. The body approaches it gradually.</summary>
        public void SetMorphologyTarget(Morphology target)
        {
            _target = Clamp(target);
        }

        private void UpdateDemoTarget()
        {
            // A slow parameter wave demonstrates topology stability.
            // This intentionally does not represent biological adaptation.
            float t = Time.time * 0.22f;
            Morphology next = _demoBase;
            next.width = _demoBase.width * (1f + 0.13f * Mathf.Sin(t));
            next.height = _demoBase.height * (1f + 0.12f * Mathf.Sin(t * 0.83f + 1.4f));
            next.forwardBias = Mathf.Clamp(_demoBase.forwardBias + 0.075f * Mathf.Sin(t * 0.61f), -0.16f, 0.16f);
            next.dorsalBias = Mathf.Clamp(_demoBase.dorsalBias + 0.065f * Mathf.Sin(t * 0.74f + 0.9f), -0.16f, 0.16f);
            next.lateralLobes = Mathf.Clamp(_demoBase.lateralLobes + 0.055f * Mathf.Sin(t * 0.49f + 2.1f), -0.14f, 0.14f);
            _target = Clamp(next);
        }

        private void EnsureComponents()
        {
            if (_meshFilter == null) _meshFilter = GetComponent<MeshFilter>();
            if (_meshRenderer == null) _meshRenderer = GetComponent<MeshRenderer>();
            if (_polygonCollider == null) _polygonCollider = GetComponent<PolygonCollider2D>();
            if (_lineRenderer == null) _lineRenderer = GetComponent<LineRenderer>();

            if (_mesh == null)
            {
                _mesh = new Mesh { name = "Evolution Creature Vector Mesh" };
                _mesh.MarkDynamic();
                _mesh.hideFlags = HideFlags.DontSave;
                _meshFilter.sharedMesh = _mesh;
            }

            if (_material == null)
            {
                Shader shader = Shader.Find("Sprites/Default");
                if (shader == null) shader = Shader.Find("Unlit/Color");
                if (shader != null)
                {
                    _material = new Material(shader) { name = "Evolution Creature Runtime Material" };
                    _material.hideFlags = HideFlags.DontSave;
                    _meshRenderer.sharedMaterial = _material;
                    _lineRenderer.sharedMaterial = _material;
                }
            }

            _meshRenderer.sortingOrder = 10;
            _lineRenderer.useWorldSpace = false;
            _lineRenderer.loop = false;
            _lineRenderer.numCornerVertices = 4;
            _lineRenderer.numCapVertices = 4;
            _lineRenderer.textureMode = LineTextureMode.Stretch;
            _lineRenderer.sortingOrder = 11;
        }

        private void RebuildGeometry()
        {
            if (_mesh == null || _polygonCollider == null || _lineRenderer == null)
                return;

            int count = Mathf.Clamp(contourResolution, 24, 128);
            bool topologyChanged = _vertices == null || _vertices.Length != count + 1;

            if (topologyChanged)
            {
                _vertices = new Vector3[count + 1];
                _contour = new Vector2[count];
                _triangles = new int[count * 3];

                for (int i = 0; i < count; i++)
                {
                    int next = (i + 1) % count;
                    _triangles[i * 3] = 0;
                    _triangles[i * 3 + 1] = i + 1;
                    _triangles[i * 3 + 2] = next + 1;
                }

                _mesh.Clear();
            }

            _vertices[0] = Vector3.zero;
            float angleStep = Mathf.PI * 2f / count;

            for (int i = 0; i < count; i++)
            {
                float angle = i * angleStep;
                float c = Mathf.Cos(angle);
                float s = Mathf.Sin(angle);

                // Restrained harmonics keep the radial function positive.
                float radial = 1f
                    + _current.forwardBias * c
                    + _current.dorsalBias * s
                    + _current.lateralLobes * Mathf.Cos(2f * angle);
                radial = Mathf.Clamp(radial, 0.54f, 1.46f);

                Vector2 point = new Vector2(
                    c * _current.width * radial,
                    s * _current.height * radial);

                _contour[i] = point;
                _vertices[i + 1] = new Vector3(point.x, point.y, 0f);
            }

            _mesh.vertices = _vertices;
            if (topologyChanged)
                _mesh.triangles = _triangles;
            _mesh.RecalculateBounds();
            _mesh.RecalculateNormals();

            _polygonCollider.pathCount = 1;
            _polygonCollider.SetPath(0, _contour);

            _lineRenderer.startWidth = outlineWidth;
            _lineRenderer.endWidth = outlineWidth;
            _lineRenderer.startColor = outlineColor;
            _lineRenderer.endColor = outlineColor;
            _lineRenderer.positionCount = count + 1;
            for (int i = 0; i < count; i++)
                _lineRenderer.SetPosition(i, _vertices[i + 1]);
            _lineRenderer.SetPosition(count, _vertices[1]);

            if (_material != null)
                _material.color = bodyColor;
        }

        private static Morphology Clamp(Morphology value)
        {
            value.width = Mathf.Clamp(value.width, 0.15f, 2.5f);
            value.height = Mathf.Clamp(value.height, 0.15f, 2.0f);
            value.forwardBias = Mathf.Clamp(value.forwardBias, -0.16f, 0.16f);
            value.dorsalBias = Mathf.Clamp(value.dorsalBias, -0.16f, 0.16f);
            value.lateralLobes = Mathf.Clamp(value.lateralLobes, -0.14f, 0.14f);
            return value;
        }

        private static Morphology MoveTowards(Morphology from, Morphology to, float maxDelta)
        {
            from.width = Mathf.MoveTowards(from.width, to.width, maxDelta);
            from.height = Mathf.MoveTowards(from.height, to.height, maxDelta);
            from.forwardBias = Mathf.MoveTowards(from.forwardBias, to.forwardBias, maxDelta * 0.35f);
            from.dorsalBias = Mathf.MoveTowards(from.dorsalBias, to.dorsalBias, maxDelta * 0.35f);
            from.lateralLobes = Mathf.MoveTowards(from.lateralLobes, to.lateralLobes, maxDelta * 0.35f);
            return from;
        }

        private void OnDestroy()
        {
            if (_mesh != null) DestroyGeneratedObject(_mesh);
            if (_material != null) DestroyGeneratedObject(_material);
        }

        private static void DestroyGeneratedObject(Object obj)
        {
            if (Application.isPlaying)
                Destroy(obj);
            else
                DestroyImmediate(obj);
        }
    }
}
