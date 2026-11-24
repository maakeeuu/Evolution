using System.Text;
using UnityEngine;

namespace Evolution.Articulated.Unity
{
    // Optional Inspector snapshot only: no permanent HUD, no animation or solver mutations.
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ArticulatedOrganism2D))]
    public sealed class ArticulationDiagnostics2D : MonoBehaviour
    {
        [SerializeField] private bool captureDiagnostics;
        [SerializeField,TextArea(3,24)] private string snapshot="Enable Capture Diagnostics during Play.";
        private ArticulatedOrganism2D organism;
        private float nextCapture;
        public string Snapshot => snapshot;
        public bool CaptureDiagnostics { get => captureDiagnostics;set => captureDiagnostics=value; }
        private void Awake() => organism=GetComponent<ArticulatedOrganism2D>();
        private void Update()
        {
            if(!captureDiagnostics || Time.unscaledTime<nextCapture) return;
            nextCapture=Time.unscaledTime+.25f;
            if(organism==null || !organism.IsInitialized) { snapshot="Organism not initialized / stopped.";return; }
            var text=new StringBuilder(2048);int activeBodies=0,activeJoints=0;
            for(int i=0;i<organism.BoneCount;i++)
            {
                var b=organism.GetBody(i);
                if(b!=null && b.simulated && b.gameObject.activeInHierarchy) activeBodies++;
                if(b==null) continue;
                text.AppendLine($"B{i}: p={b.position:F4} omega={b.angularVelocity:F3} deg/s v={b.linearVelocity:F4} mass={b.mass:F3} simulated={b.simulated} constraints={b.constraints} awake={b.IsAwake()}");
            }
            for(int i=0;i<organism.BoneCount-1;i++)
            {
                var j=organism.GetJoint(i);if(j==null) { text.AppendLine($"J{i}: MISSING");continue; }
                bool active=j.isActiveAndEnabled && j.connectedBody!=null;if(active) activeJoints++;
                Vector2 a=organism.GetBody(i+1).GetRelativePoint(j.anchor);
                Vector2 b=j.connectedBody!=null ? j.connectedBody.GetRelativePoint(j.connectedAnchor) : j.connectedAnchor;
                text.AppendLine($"J{i}: angle={j.jointAngle:F3} speed={j.jointSpeed:F3} deg/s limits={j.useLimits} [{j.limits.min:F1},{j.limits.max:F1}] motor={j.useMotor} active={active} enabled={j.enabled} A={a:F4} B={b:F4} gap={Vector2.Distance(a,b):F6}");
            }
            for(int i=0;i<organism.MuscleCount;i++)
                text.AppendLine($"M{i}: command={organism.GetCommand(i):F3} activation={organism.GetActivation(i):F3} tension={organism.GetTension(i):F3} N L={organism.GetLength(i):F5} target={organism.GetTargetLength(i):F5} m rate={organism.GetLengthRate(i):F4} m/s |F|={organism.GetAppliedForce(i).magnitude:F3} N A={organism.GetMusclePointA(i):F4} B={organism.GetMusclePointB(i):F4}");
            snapshot=$"Steps={organism.PhysicsSteps} bodies={activeBodies}/{organism.BoneCount} joints={activeJoints}/{organism.BoneCount-1} COM={organism.CenterOfMass:F5} timeScale={Time.timeScale} fixedDt={Time.fixedDeltaTime} mode={Physics2D.simulationMode}\n"+text;
        }
    }
}
