using System;
using System.Text;
using UnityEngine;

namespace Evolution.Articulated.Unity
{
    // Explicit EV-CP3 fixture. Geometry is never used to drive the physics.
    [DisallowMultipleComponent]
    public sealed class RodDemo2D : MonoBehaviour
    {
        [SerializeField] private ArticulatedOrganismView2D.InspectionMode mode;
        [SerializeField] private bool waveDrive;
        [SerializeField,TextArea(6,22)] private string baseline="Waiting for actual runtime anatomy.";
        private ArticulatedDemo2D demo;
        private ArticulatedOrganismView2D view;
        private MuscleWaveDriver2D driver;
        private ArticulatedRodSkin2D skin;
        private bool previousDrive,logged;
        private float nextReport;
        public string Baseline => baseline;
        private void Awake()
        {
            // Inspect this scene, including inactive objects; never delete a conflicting user object.
            foreach(var root in gameObject.scene.GetRootGameObjects())
            foreach(var component in root.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if(component==null) continue;
                string name=component.GetType().FullName;
                if(name=="Evolution.Creatures.ProceduralCreature2D" || name=="Evolution.Source.Unity.CreatureBody2D"
                    || name=="Evolution.Source.Unity.EvolutionPrototype2D")
                { baseline="BLOCKED: radial component in this scene: "+name;Debug.LogError(baseline,this);var time=GetComponent<SimulationTimeControl2D>();if(time!=null) time.enabled=false;enabled=false;return; }
            }
            demo=gameObject.AddComponent<ArticulatedDemo2D>();demo.InitialWaveDrive=false;
        }
        private void Update()
        {
            if(demo==null || demo.Organism==null || !demo.Organism.IsInitialized) return;
            var organism=demo.Organism;
            if(view==null)
            {
                view=organism.GetComponent<ArticulatedOrganismView2D>();
                driver=organism.GetComponent<MuscleWaveDriver2D>();
                skin=organism.GetComponent<ArticulatedRodSkin2D>();
            }
            view.SetInspection(mode);
            if(demo.TerrainRenderer!=null) demo.TerrainRenderer.enabled=mode!=ArticulatedOrganismView2D.InspectionMode.ShellOnly;
            if(driver!=null) driver.enabled=waveDrive;
            if(previousDrive && !waveDrive) organism.ClearCommands();
            previousDrive=waveDrive;
            if(Time.unscaledTime<nextReport) return;
            nextReport=Time.unscaledTime+.5f;
            var text=new StringBuilder("EV-CP3 runtime baseline; source base bf1e333; no historical exact restoration\n");
            text.Append("Shell: ArticulatedRodSkin2D / RodSkinGeometry longitudinal strip; diagnostics: ").Append(view.GetType().FullName).Append("; mode: ").Append(mode);
            text.Append("\nPhysics: ").Append(organism.GetType().FullName).Append("; segments: ").Append(organism.BoneCount);
            for(int i=0;i<organism.BoneCount;i++)
            {
                var b=organism.Definition[i];var body=organism.GetBody(i);
                text.AppendFormat("\n{0}: L={1:F6}, R={2:F6}, mass={3:F6}, gravity={4}, collider vertices={5}",i,b.Length,b.Radius,body.mass,body.gravityScale,organism.GetCollider(i).GetPath(0).Length);
                if(i>0) text.AppendFormat("; hinge={0:F2} deg, limits={1}/{2}, motor={3}",organism.GetJoint(i-1).jointAngle,organism.GetJoint(i-1).limits.min,organism.GetJoint(i-1).limits.max,organism.GetJoint(i-1).useMotor);
            }
            text.Append("\nMuscles: 2 per hinge; demo settings 60 N / 600 N/m / 6 Ns/m / contraction .3; arm = radius*.72, longitudinal offset = length*.25");
            text.Append("\nWave = ").Append(waveDrive).Append(" (force commands only; not learning). Restart Play for fresh rest pose.");
            text.Append("\nCamera: ArticulatedDemo2D, orthographic size 3.5, position (0,1,-10). Fixed step = ").Append(Time.fixedDeltaTime);
            if(skin!=null) text.Append("\n").Append(skin.Status);
            baseline=text.ToString();
            if(!logged) { Debug.Log(baseline,this);logged=true; }
        }
        private void OnGUI()
        {
            if(!enabled) return;
            string[] labels={"Normal","Shell only","Bones/colliders","Joints/muscles"};
            for(int i=0;i<labels.Length;i++)
                if(GUI.Button(new Rect(8+(i%2)*116,8+(i/2)*28,112,25),labels[i])) mode=(ArticulatedOrganismView2D.InspectionMode)i;
            if(skin!=null && !skin.ValidPose) GUI.Label(new Rect(8,94,620,38),"SKIN INVALID — see Inspector / Console; physics unchanged.");
            waveDrive=GUI.Toggle(new Rect(8,66,340,24),waveDrive,"Wave drive (real forces); restart Play for rest pose");
        }
        private void OnDisable() { if(demo!=null) demo.enabled=false; }
        private void OnEnable() { if(demo!=null) demo.enabled=true; }
        private void OnDestroy() { if(demo!=null) Destroy(demo); }
    }
}
