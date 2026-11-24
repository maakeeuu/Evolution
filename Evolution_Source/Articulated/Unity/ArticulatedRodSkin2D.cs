using System;
using Evolution.Articulated.Core;
using Evolution.Source.Core;
using UnityEngine;

namespace Evolution.Articulated.Unity
{
    /// <summary>Read-only render adapter. No physics writes, no static shape animation.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ArticulatedOrganism2D))]
    public sealed class ArticulatedRodSkin2D : MonoBehaviour
    {
        private ArticulatedOrganism2D organism;
        private RodSkinGeometry surface;
        private Point2[] starts,ends;
        private Vector3[] vertices;
        private GameObject owned;
        private Mesh mesh;
        private MeshRenderer renderer2D;
        private Material material;
        private bool visible=true,warnedCoverage,buildFailed;
        private float nextMeasure;
        [SerializeField,TextArea(3,8)] private string status="Waiting for initialized physical anatomy.";
        public string Status => status;
        public bool ValidPose { get; private set; }
        public CoverageMeasure Coverage { get; private set; }
        public RodSkinGeometry Surface => surface;
        public bool Visible
        {
            get => visible;
            set { visible=value;ApplyVisibility(); }
        }
        private void Awake() { organism=GetComponent<ArticulatedOrganism2D>(); }
        private void LateUpdate() => RefreshSkin();
        public void RefreshSkin()
        {
            if(organism==null || !organism.IsInitialized || !organism.isActiveAndEnabled)
            { ValidPose=false;ApplyVisibility();return; }
            try
            {
                if(buildFailed) return;
                if(surface==null)
                {
                    try { Build(); }
                    catch { buildFailed=true;Release();throw; }
                }
                for(int i=0;i<organism.BoneCount;i++)
                {
                    Transform body=organism.GetBody(i).transform;
                    float half=(float)(organism.Definition[i].Length*.5);
                    Vector2 a=i==0 ? new Vector2(-half,0) : organism.GetJoint(i-1).anchor;
                    Vector2 b=i==organism.BoneCount-1 ? new Vector2(half,0) : organism.GetJoint(i).connectedAnchor;
                    // Both skin and diagnostic collider outlines use the same interpolated render poses.
                    starts[i]=LocalPoint(body.TransformPoint(new Vector3(a.x,a.y,0)));
                    ends[i]=LocalPoint(body.TransformPoint(new Vector3(b.x,b.y,0)));
                }
                surface.Update(starts,ends);
                for(int i=0;i<vertices.Length;i++) vertices[i]=new Vector3((float)surface.Vertices[i].X,(float)surface.Vertices[i].Y,0);
                mesh.vertices=vertices;mesh.RecalculateBounds();ValidPose=true;
                if(Time.unscaledTime>=nextMeasure)
                {
                    nextMeasure=Time.unscaledTime+.5f;
                    CaptureCoverage();
                }
            }
            catch(Exception error)
            {
                ValidPose=false;
                string message="SKIN INVALID (physics unchanged): "+error.Message;
                if(status!=message) Debug.LogWarning(message,this);
                status=message;
            }
            ApplyVisibility();
        }
        private Point2 LocalPoint(Vector3 world)
        { Vector3 p=transform.InverseTransformPoint(world);return new Point2(p.x,p.y); }
        private void Build()
        {
            var geometry=new RodSkinGeometry(organism.Definition);
            starts=new Point2[organism.BoneCount];ends=new Point2[starts.Length];
            vertices=new Vector3[geometry.Vertices.Length];
            Shader shader=Resources.Load<Shader>("EvolutionVector2D");
            if(shader==null || !shader.isSupported) throw new InvalidOperationException("Supported EvolutionVector2D required.");
            material=new Material(shader) { name="EV-CP3 rod skin",hideFlags=HideFlags.DontSave };
            material.SetColor("_Color",new Color(.23f,.48f,.4f,1));
            mesh=new Mesh { name="EV-CP3 longitudinal strip",hideFlags=HideFlags.DontSave };
            mesh.MarkDynamic();mesh.vertices=vertices;mesh.triangles=geometry.Triangles;
            owned=new GameObject("EV-CP3 continuous rod skin");owned.transform.SetParent(transform,false);
            owned.AddComponent<MeshFilter>().sharedMesh=mesh;
            renderer2D=owned.AddComponent<MeshRenderer>();renderer2D.enabled=false;
            renderer2D.sharedMaterial=material;renderer2D.sortingOrder=10;surface=geometry;
        }
        public void CaptureCoverage()
        {
            if(!ValidPose || surface==null) return;
            var paths=new Point2[organism.BoneCount][];
            for(int i=0;i<paths.Length;i++)
            {
                var collider=organism.GetCollider(i);
                if(collider.pathCount!=1) throw new InvalidOperationException("Expected one physical path per segment.");
                Vector2[] path=collider.GetPath(0);paths[i]=new Point2[path.Length];
                for(int p=0;p<path.Length;p++)
                {
                    Vector2 q=path[p]+collider.offset;
                    paths[i][p]=LocalPoint(collider.transform.TransformPoint(new Vector3(q.x,q.y,0)));
                }
            }
            Coverage=surface.MeasureCoverage(paths,8);
            double minRadius=organism.Definition[organism.BoneCount-1].Radius;
            bool excessive=Math.Max(Coverage.SkinOutsidePhysics,Coverage.PhysicsOutsideSkin)>.25*minRadius;
            status=string.Format("{0}; sections={1}, triangles={2}; gap={3:F6} m; max miter={4:F6}\nSampled solid coverage: skin outside colliders={5:F6} m; colliders outside skin={6:F6} m; samples={7}\nActual collider paths transformed with render poses; not exact Hausdorff / native solver validation.",
                excessive ? "COVERAGE WARNING" : "Skin valid",surface.SectionCount,surface.Triangles.Length/3,surface.LargestAnchorGap,surface.LargestMiterRatio,Coverage.SkinOutsidePhysics,Coverage.PhysicsOutsideSkin,Coverage.Samples);
            if(excessive && !warnedCoverage) Debug.LogWarning(status,this);
            warnedCoverage=excessive;
        }
        private void ApplyVisibility()
        { if(renderer2D!=null) renderer2D.enabled=visible && ValidPose && isActiveAndEnabled && organism!=null && organism.isActiveAndEnabled; }
        private void OnDisable() { if(renderer2D!=null) renderer2D.enabled=false; }
        private void OnDestroy() => Release();
        private void Release()
        {
            if(owned!=null) { owned.SetActive(false);Destroy(owned); }
            if(mesh!=null) Destroy(mesh);
            if(material!=null) Destroy(material);
        }
    }
}
