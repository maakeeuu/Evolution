using System;
using UnityEngine;

namespace Evolution.Articulated.Unity
{
    // Opt-in fixture: a small natural ground profile and a free, gravity-loaded chain.
    [DisallowMultipleComponent]
    public sealed class ArticulatedDemo2D : MonoBehaviour
    {
        public ArticulatedOrganism2D Organism { get; private set; }
        private GameObject ownedRoot;
        private Mesh terrainMesh;
        private Material terrainMaterial;
        private void Start()
        {
            try
            {
                ownedRoot=new GameObject("EV-CP2 Articulated Demonstration");
                var cameraObject=new GameObject("EV-CP2 camera");cameraObject.transform.SetParent(ownedRoot.transform,false);
                cameraObject.transform.position=new Vector3(0,1,-10);
                var camera=cameraObject.AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=3.5f;
                camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.93f,.92f,.87f,1);
                var ground=new GameObject("Natural soil profile");ground.transform.SetParent(ownedRoot.transform,false);
                var points=new[]{new Vector2(-12,-2),new Vector2(12,-2),new Vector2(12,-.1f),
                    new Vector2(5,-.1f),new Vector2(2,.15f),new Vector2(0,-.15f),new Vector2(-3,-.15f),new Vector2(-12,-.4f)};
                ground.AddComponent<PolygonCollider2D>().SetPath(0,points);
                // Monotone soil strip: explicit top/bottom pairs, not an invalid concave fan.
                var vertices=new Vector3[12];
                Vector2[] top={points[7],points[6],points[5],points[4],points[3],points[2]};
                for(int i=0;i<top.Length;i++) { vertices[i*2]=new Vector3(top[i].x,-2,0);vertices[i*2+1]=top[i]; }
                var triangles=new int[(top.Length-1)*6];
                for(int i=0;i<top.Length-1;i++)
                { int k=i*6,v=i*2;triangles[k]=v;triangles[k+1]=v+2;triangles[k+2]=v+1;
                  triangles[k+3]=v+1;triangles[k+4]=v+2;triangles[k+5]=v+3; }
                terrainMesh=new Mesh { name="EV-CP2 soil",hideFlags=HideFlags.DontSave };
                terrainMesh.vertices=vertices;terrainMesh.triangles=triangles;terrainMesh.RecalculateBounds();
                Shader shader=Resources.Load<Shader>("EvolutionVector2D");
                if(shader==null || !shader.isSupported) throw new InvalidOperationException("Supported shared vector shader required.");
                terrainMaterial=new Material(shader) { hideFlags=HideFlags.DontSave };
                terrainMaterial.SetColor("_Color",new Color(.4f,.38f,.3f,1));
                ground.AddComponent<MeshFilter>().sharedMesh=terrainMesh;
                ground.AddComponent<MeshRenderer>().sharedMaterial=terrainMaterial;
                var organism=new GameObject("Articulated organism");organism.transform.SetParent(ownedRoot.transform,false);
                organism.transform.position=new Vector3(-1.8f,1f,0);
                Organism=organism.AddComponent<ArticulatedOrganism2D>();
                organism.AddComponent<ArticulatedOrganismView2D>();
                organism.AddComponent<MuscleWaveDriver2D>();
                organism.AddComponent<ArticulationDiagnostics2D>();
                Debug.Log("EV-CP2: physical hinges and pull-only muscles. Wave excitation is NOT learning; net locomotion is not yet validated.",this);
            }
            catch(Exception error) { Release();enabled=false;Debug.LogException(error,this); }
        }
        private void OnDisable() { if(ownedRoot!=null) ownedRoot.SetActive(false); }
        private void OnEnable() { if(ownedRoot!=null) ownedRoot.SetActive(true); }
        private void OnDestroy() => Release();
        private void Release()
        {
            if(ownedRoot!=null) { ownedRoot.SetActive(false);Destroy(ownedRoot); }
            if(terrainMesh!=null) Destroy(terrainMesh);
            if(terrainMaterial!=null) Destroy(terrainMaterial);
            ownedRoot=null;terrainMesh=null;terrainMaterial=null;Organism=null;
        }
    }
}
