using System;
using System.Collections.Generic;
using Evolution.Articulated.Core;
using Evolution.Source.Core;
using UnityEngine;

namespace Evolution.Articulated.Unity
{
    // First envelope: opaque union of overlapping convex segment patches, not a global manifold skin.
    // Every patch copies its physical polygon exactly and inherits that body's interpolated transform.
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ArticulatedOrganism2D))]
    public sealed class ArticulatedOrganismView2D : MonoBehaviour
    {
        [SerializeField] private bool showShell=true;
        [SerializeField] private bool showSkeleton=true;
        [SerializeField] private bool showMuscles=true;
        private bool showJoints=true,showColliders;
        public enum InspectionMode { Normal, ShellOnly, SkeletonColliders, JointsMuscles }
        public void SetInspection(InspectionMode mode)
        {
            showShell=mode==InspectionMode.Normal || mode==InspectionMode.ShellOnly;
            showSkeleton=mode==InspectionMode.SkeletonColliders;
            showColliders=showSkeleton;
            showJoints=showSkeleton || mode==InspectionMode.JointsMuscles;
            showMuscles=mode==InspectionMode.JointsMuscles;
        }
        private ArticulatedOrganism2D organism;
        private Material material;
        private readonly List<GameObject> ownedObjects=new List<GameObject>();
        private readonly List<Mesh> meshes=new List<Mesh>();
        private MeshRenderer[] shells,jointMarkers;
        private MaterialPropertyBlock properties;
        private LineRenderer[] bones,muscles,colliderLines;
        private bool built,failed;
        private void Awake()
        { organism=GetComponent<ArticulatedOrganism2D>();properties=new MaterialPropertyBlock(); }
        private void LateUpdate()
        {
            if(failed) return;
            if(organism==null || (!organism.IsInitialized && built)) { Release();enabled=false;return; }
            if(!organism.IsInitialized) return;
            try
            {
                if(!built) Build();
                for(int i=0;i<shells.Length;i++)
                { shells[i].enabled=showShell && organism.enabled;bones[i].enabled=showSkeleton && organism.enabled; }
                for(int i=0;i<jointMarkers.Length;i++) jointMarkers[i].enabled=showJoints && organism.enabled;
                for(int i=0;i<colliderLines.Length;i++) colliderLines[i].enabled=showColliders && organism.enabled;
                for(int i=0;i<muscles.Length;i++)
                {
                    MuscleSpec m=organism.GetMuscle(i);
                    Vector3 a=organism.GetBody(m.Parent).transform.TransformPoint(new Vector3((float)m.ParentAnchor.X,(float)m.ParentAnchor.Y,0));
                    Vector3 b=organism.GetBody(m.Child).transform.TransformPoint(new Vector3((float)m.ChildAnchor.X,(float)m.ChildAnchor.Y,0));
                    a.z=-.02f;b.z=-.02f;
                    muscles[i].SetPosition(0,a);muscles[i].SetPosition(1,b);
                    muscles[i].enabled=showMuscles && organism.enabled;
                    Color color=Color.Lerp(new Color(.45f,.35f,.3f,1),new Color(.95f,.25f,.1f,1),(float)organism.GetActivation(i));
                    SetColor(muscles[i],color);
                    muscles[i].startWidth=muscles[i].endWidth=.014f+.016f*(float)organism.GetActivation(i);
                }
            }
            catch(Exception error)
            {
                failed=true;Release();organism.enabled=false;enabled=false;Debug.LogException(error,this);
            }
        }
        private void Build()
        {
            Shader shader=Resources.Load<Shader>("EvolutionVector2D");
            if(shader==null || !shader.isSupported) throw new InvalidOperationException("Supported EvolutionVector2D resource required.");
            material=new Material(shader) { name="EV-CP2 vector view",hideFlags=HideFlags.DontSave };
            if(!material.HasProperty("_Color")) throw new InvalidOperationException("Vector shader requires _Color.");
            shells=new MeshRenderer[organism.BoneCount];bones=new LineRenderer[organism.BoneCount];
            colliderLines=new LineRenderer[organism.BoneCount];
            for(int i=0;i<organism.BoneCount;i++)
            {
                Transform bone=organism.GetBody(i).transform;
                GameObject patch=CreateObject("Envelope patch "+i,bone);
                Vector2[] contour=organism.GetCollider(i).GetPath(0);
                colliderLines[i]=CreateLine("Collider outline "+i,bone,.009f,22,false);
                colliderLines[i].positionCount=contour.Length;colliderLines[i].loop=true;
                for(int j=0;j<contour.Length;j++) colliderLines[i].SetPosition(j,new Vector3(contour[j].x,contour[j].y,-.03f));
                SetColor(colliderLines[i],new Color(.2f,.5f,.9f,1));
                var vertices=new Vector3[contour.Length+1];
                for(int j=0;j<contour.Length;j++) vertices[j+1]=new Vector3(contour[j].x,contour[j].y,0);
                var mesh=new Mesh { name="EV-CP2 shared contour patch "+i,hideFlags=HideFlags.DontSave };
                meshes.Add(mesh);mesh.vertices=vertices;mesh.triangles=RadialGeometry.CreateFan(contour.Length);mesh.RecalculateBounds();
                patch.AddComponent<MeshFilter>().sharedMesh=mesh;
                var renderer=patch.AddComponent<MeshRenderer>();shells[i]=renderer;
                renderer.sharedMaterial=material;renderer.sortingOrder=10;SetColor(renderer,new Color(.23f,.48f,.4f,1));
                bones[i]=CreateLine("Bone "+i,bone,.035f,20,false);
                float length=(float)organism.Definition[i].Length;
                bones[i].SetPosition(0,new Vector3(-length*.5f,0,-.01f));
                bones[i].SetPosition(1,new Vector3(length*.5f,0,-.01f));
                SetColor(bones[i],new Color(.9f,.85f,.65f,1));
            }
            jointMarkers=new MeshRenderer[organism.BoneCount-1];
            var diskVertices=new Vector3[33];
            for(int j=0;j<32;j++)
            { double angle=2*Math.PI*j/32;diskVertices[j+1]=new Vector3((float)Math.Cos(angle)*.048f,(float)Math.Sin(angle)*.048f,0); }
            var disk=new Mesh { name="EV-CP2 joint marker",hideFlags=HideFlags.DontSave };
            meshes.Add(disk);disk.vertices=diskVertices;disk.triangles=RadialGeometry.CreateFan(32);disk.RecalculateBounds();
            for(int i=0;i<jointMarkers.Length;i++)
            {
                GameObject marker=CreateObject("Joint "+i,organism.GetBody(i+1).transform);
                marker.transform.localPosition=new Vector3((float)(-organism.Definition[i+1].Length*.5),0,-.015f);
                marker.AddComponent<MeshFilter>().sharedMesh=disk;
                var renderer=marker.AddComponent<MeshRenderer>();jointMarkers[i]=renderer;
                renderer.sharedMaterial=material;renderer.sortingOrder=25;SetColor(renderer,new Color(.95f,.75f,.25f,1));
            }
            muscles=new LineRenderer[organism.MuscleCount];
            for(int i=0;i<muscles.Length;i++) muscles[i]=CreateLine("Muscle "+i,transform,.02f,30,true);
            built=true;
        }
        private GameObject CreateObject(string label,Transform parent)
        {
            var go=new GameObject(label);ownedObjects.Add(go);go.transform.SetParent(parent,false);return go;
        }
        private LineRenderer CreateLine(string label,Transform parent,float width,int order,bool world)
        {
            var line=CreateObject(label,parent).AddComponent<LineRenderer>();line.sharedMaterial=material;
            line.useWorldSpace=world;line.positionCount=2;line.startWidth=line.endWidth=width;
            line.numCapVertices=6;line.sortingOrder=order;return line;
        }
        private void SetColor(Renderer renderer,Color color)
        {
            properties.SetColor("_Color",color);renderer.SetPropertyBlock(properties);
        }
        private void OnDisable()
        {
            foreach(var go in ownedObjects) if(go!=null) go.SetActive(false);
        }
        private void OnEnable()
        {
            foreach(var go in ownedObjects) if(go!=null) go.SetActive(true);
        }
        private void OnDestroy() => Release();
        private void Release()
        {
            foreach(var go in ownedObjects) if(go!=null) { go.SetActive(false);Destroy(go); }
            foreach(var mesh in meshes) if(mesh!=null) Destroy(mesh);
            if(material!=null) Destroy(material);
            ownedObjects.Clear();meshes.Clear();material=null;built=false;
        }
    }
}
