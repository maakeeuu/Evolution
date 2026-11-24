using System.Collections;
using UnityEngine;

namespace Evolution.Articulated.Unity
{
    // Isolated native A/B/C/D acceptance harness. Nothing is declared passed before it runs.
    // Fresh bodies for each antagonist, never a live-body pose/velocity reset.
    [DisallowMultipleComponent]
    public sealed class SingleJointProbe2D : MonoBehaviour
    {
        [SerializeField,TextArea(3,12)] private string result="NOT RUN: enter Play in an empty probe scene.";
        private GameObject specimen,cameraObject;
        private ArticulatedOrganism2D organism;
        public bool Complete { get; private set; }
        public bool Passed { get; private set; }
        public string Result => result;
        private float[] angles=new float[2];
        private double peakTension;
        private float maxComDrift,maxMomentum,maxAnchorGap;
        private bool checks=true;
        private void Start()
        {
            cameraObject=new GameObject("Isolated joint camera");cameraObject.transform.SetParent(transform,false);
            cameraObject.transform.localPosition=new Vector3(.85f,0,-10);
            var camera=cameraObject.AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=1.5f;
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.93f,.92f,.87f,1);
            StartCoroutine(Run());
        }
        private void CreateSpecimen()
        {
            specimen=new GameObject("Isolated two-segment specimen");specimen.transform.SetParent(transform,false);
            organism=specimen.AddComponent<ArticulatedOrganism2D>();organism.ConfigureIsolatedJoint();
            specimen.AddComponent<ArticulatedOrganismView2D>();
            specimen.AddComponent<ArticulationDiagnostics2D>().CaptureDiagnostics=true;
        }
        private void Observe(Vector2 initialCom)
        {
            if(organism==null || !organism.IsInitialized) { checks=false;return; }
            maxComDrift=Mathf.Max(maxComDrift,Vector2.Distance(initialCom,organism.CenterOfMass));
            Vector2 momentum=Vector2.zero;
            for(int i=0;i<organism.BoneCount;i++)
            { var b=organism.GetBody(i);momentum+=b.mass*b.linearVelocity;checks &= !organism.HasContact(i); }
            maxMomentum=Mathf.Max(maxMomentum,momentum.magnitude);
            var joint=organism.GetJoint(0);
            Vector2 a=organism.GetBody(1).GetRelativePoint(joint.anchor);
            Vector2 b=organism.GetBody(0).GetRelativePoint(joint.connectedAnchor);
            maxAnchorGap=Mathf.Max(maxAnchorGap,Vector2.Distance(a,b));
            checks &= !joint.useMotor && joint.useLimits && joint.connectedBody==organism.GetBody(0);
        }
        private IEnumerator Run()
        {
            Passed=false;Complete=false;
            for(int side=0;side<2;side++)
            {
                CreateSpecimen();yield return null;yield return new WaitForFixedUpdate();
                if(!organism.IsInitialized) { result="FAIL: organism initialization. Inspect Console.";Complete=true;yield break; }
                Vector2 com=organism.CenterOfMass;float baseline=organism.GetJoint(0).jointAngle;
                result="A: passive, zero gravity/damping, no ground; observing native angles and COM.";
                for(int step=0;step<50;step++)
                {
                    yield return new WaitForFixedUpdate();Observe(com);
                    checks &= Mathf.Abs(Mathf.DeltaAngle(baseline,organism.GetJoint(0).jointAngle))<.1f;
                    for(int i=0;i<2;i++) checks &= organism.GetBody(i).linearVelocity.magnitude<.001f
                        && Mathf.Abs(organism.GetBody(i).angularVelocity)<.1f;
                }
                result=side==0 ? "B: only muscle 0 active; measuring actual joint deflection." : "C: fresh passive specimen, only muscle 1 active; measuring opposite deflection.";
                peakTension=0;organism.SetActivationCommand(side,1);
                for(int step=0;step<25;step++)
                {
                    yield return new WaitForFixedUpdate();Observe(com);
                    peakTension=System.Math.Max(peakTension,organism.GetTension(side));
                    float angle=Mathf.DeltaAngle(baseline,organism.GetJoint(0).jointAngle);
                    if(Mathf.Abs(angle)>Mathf.Abs(angles[side])) angles[side]=angle;
                }
                checks &= peakTension>.1 && Mathf.Abs(angles[side])>5;
                organism.ClearCommands();result="D: commands zero; observing activation decay, not forcing a return pose.";
                for(int step=0;step<50;step++) { yield return new WaitForFixedUpdate();Observe(com); }
                checks &= organism.GetActivation(side)<.001;
                if(side==0) { Destroy(specimen);yield return null;specimen=null; }
            }
            checks &= angles[0]*angles[1]<0 && maxComDrift<.002f && maxMomentum<.002f && maxAnchorGap<.01f;
            Complete=true;Passed=checks;
            result=$"{(Passed ? "PASS" : "FAIL")}: native A/B/C/D. Angles={angles[0]:F3}, {angles[1]:F3} deg; max COM drift={maxComDrift:F6} m; max |momentum|={maxMomentum:F6} kg m/s; max anchor gap={maxAnchorGap:F6} m. Activation released <0.001. Opposite signs and >5 deg required. See diagnostics if FAIL.";
            Debug.Log(result,this);
        }
        private void OnDisable()
        {
            StopAllCoroutines();
            if(!Complete) { result="ABORTED: probe disabled; restart Play to run again.";Passed=false; }
            if(specimen!=null) specimen.SetActive(false);
            if(cameraObject!=null) cameraObject.SetActive(false);
        }
        private void OnDestroy()
        { if(specimen!=null) Destroy(specimen);if(cameraObject!=null) Destroy(cameraObject); }
    }
}
