using System;
using Evolution.Articulated.Core;
using Evolution.Source.Core;
using UnityEngine;

namespace Evolution.Articulated.Unity
{
    // Owns only generated bodies, constraints, colliders and actuator state. No root motor.
    [DisallowMultipleComponent]
    public sealed class ArticulatedOrganism2D : MonoBehaviour
    {
        [SerializeField, Range(2,8)] private int segmentCount=4;
        [SerializeField, Range(.5f,1.5f)] private float segmentLength=.85f;
        [SerializeField, Range(.08f,.2f)] private float segmentRadius=.14f;
        [SerializeField, Range(.94f,1f)] private float taper=.96f;
        [SerializeField, Range(5f,60f)] private float jointLimit=40f;
        [SerializeField] private bool musclesEnabled=true;
        private bool isolatedJoint;
        private double[] lengths,lengthRates,targets;
        public long PhysicsSteps { get; private set; }
        // Creation-time test configuration only. Never changes velocities/transforms of live bodies.
        public void ConfigureIsolatedJoint()
        {
            if(ownedRoot!=null || IsInitialized) throw new InvalidOperationException("Configure before Start.");
            segmentCount=2;isolatedJoint=true;
        }
        public double GetCommand(int index) => commands[index];
        public double GetLength(int index) => lengths[index];
        public double GetTargetLength(int index) => targets[index];
        public double GetLengthRate(int index) => lengthRates[index];
        public Vector2 GetAppliedForce(int index) => forceA[index];
        public Vector2 GetMusclePointA(int index) => pointA[index];
        public Vector2 GetMusclePointB(int index) => pointB[index];
        public Vector2 CenterOfMass
        {
            get
            {
                Vector2 sum=Vector2.zero;float mass=0;
                if(!IsInitialized) return sum;
                foreach(var body in bodies) if(body!=null) { sum+=body.mass*body.worldCenterOfMass;mass+=body.mass; }
                return mass>0 ? sum/mass : Vector2.zero;
            }
        }
        private GameObject ownedRoot;
        private PhysicsMaterial2D contactMaterial;
        private Rigidbody2D[] bodies;
        private PolygonCollider2D[] colliders;
        private HingeJoint2D[] joints;
        private MuscleSpec[] muscles;
        private MuscleSettings[] settings;
        private double[] commands,activation,tension;
        private Vector2[] forceA,pointA,pointB;
        public bool IsInitialized { get; private set; }
        public ChainDefinition Definition { get; private set; }
        public int BoneCount => IsInitialized ? bodies.Length : 0;
        public int MuscleCount => IsInitialized ? muscles.Length : 0;
        public bool MusclesEnabled { get => musclesEnabled; set => musclesEnabled=value; }
        public Rigidbody2D GetBody(int index) => bodies[index];
        public PolygonCollider2D GetCollider(int index) => colliders[index];
        // Joint 0 joins segment 1 to segment 0.
        public HingeJoint2D GetJoint(int index) => joints[index];
        public MuscleSpec GetMuscle(int index) => muscles[index];
        public double GetTension(int index) => tension[index];
        public double GetActivation(int index) => activation[index];
        public bool HasContact(int index) => colliders[index].IsTouchingLayers();
        public void SetActivationCommand(int index,double value)
        {
            if(!IsInitialized) throw new InvalidOperationException("Organism not initialized.");
            if(!Morphology.Finite(value) || value<0 || value>1) throw new ArgumentOutOfRangeException(nameof(value));
            commands[index]=value;
        }
        public void ClearCommands()
        {
            if(commands!=null) Array.Clear(commands,0,commands.Length);
        }
        private void Start()
        {
            try
            {
                if(!Morphology.Finite(transform.position.x) || !Morphology.Finite(transform.position.y)
                    || !Morphology.Finite(transform.position.z) || !Morphology.Finite(transform.lossyScale.x)
                    || !Morphology.Finite(transform.lossyScale.y) || !Morphology.Finite(transform.lossyScale.z))
                    throw new InvalidOperationException("Non-finite organism transform.");
                if((transform.lossyScale-Vector3.one).sqrMagnitude>1e-8f
                    || Mathf.Abs(transform.eulerAngles.x)>1e-4f || Mathf.Abs(transform.eulerAngles.y)>1e-4f
                    || Mathf.Abs(transform.position.z)>1e-4f)
                    throw new InvalidOperationException("Use unit scale and an XY transform at Z=0.");
                Definition=ChainDefinition.Create(segmentCount,Math.Round(segmentLength,6),
                    Math.Round(segmentRadius,6),Math.Round(taper,6),Math.Round(jointLimit,6));
                ownedRoot=new GameObject("EV-CP2 physical chain");
                ownedRoot.transform.SetParent(transform,false);
                // Keep all bodies inactive until the complete topology has been constructed.
                ownedRoot.SetActive(false);
                contactMaterial=new PhysicsMaterial2D("EV-CP2 natural contact")
                    { friction=.8f,bounciness=0f,hideFlags=HideFlags.DontSave };
                bodies=new Rigidbody2D[Definition.Count];colliders=new PolygonCollider2D[Definition.Count];
                joints=new HingeJoint2D[Definition.Count-1];
                for(int i=0;i<Definition.Count;i++)
                {
                    SegmentSpec bone=Definition[i];
                    var go=new GameObject("Segment "+i);
                    go.transform.SetParent(ownedRoot.transform,false);
                    go.transform.localPosition=new Vector3((float)Definition.CenterX(i),0,0);
                    var polygon=go.AddComponent<PolygonCollider2D>();colliders[i]=polygon;
                    Point2[] contour=SegmentContour.Create(bone.Length,bone.Radius);
                    var path=new Vector2[contour.Length];
                    for(int p=0;p<path.Length;p++) path[p]=new Vector2((float)contour[p].X,(float)contour[p].Y);
                    polygon.SetPath(0,path);polygon.sharedMaterial=contactMaterial;
                    var body=go.AddComponent<Rigidbody2D>();bodies[i]=body;
                    body.bodyType=RigidbodyType2D.Dynamic;body.mass=(float)bone.Mass;
                    body.gravityScale=isolatedJoint ? 0 : 1;
                    body.linearDamping=isolatedJoint ? 0 : .05f;body.angularDamping=isolatedJoint ? 0 : .15f;
                    body.constraints=RigidbodyConstraints2D.None;
                    body.collisionDetectionMode=CollisionDetectionMode2D.Continuous;
                    body.interpolation=RigidbodyInterpolation2D.Interpolate;
                    if(i==0) continue;
                    var joint=go.AddComponent<HingeJoint2D>();joints[i-1]=joint;
                    joint.connectedBody=bodies[bone.Parent];joint.autoConfigureConnectedAnchor=false;
                    joint.useConnectedAnchor=true;
                    joint.anchor=new Vector2((float)(-bone.Length*.5),0);
                    joint.connectedAnchor=new Vector2((float)(Definition[bone.Parent].Length*.5),0);
                    joint.useMotor=false;joint.useLimits=true;joint.enableCollision=false;
                    joint.limits=new JointAngleLimits2D { min=-(float)bone.LimitDegrees,max=(float)bone.LimitDegrees };
                }
                int count=2*joints.Length;
                muscles=new MuscleSpec[count];settings=new MuscleSettings[count];
                commands=new double[count];activation=new double[count];tension=new double[count];
                lengths=new double[count];lengthRates=new double[count];targets=new double[count];
                forceA=new Vector2[count];pointA=new Vector2[count];pointB=new Vector2[count];
                for(int i=0;i<count;i++)
                {
                    muscles[i]=new MuscleSpec(Definition,i/2+1,i%2==0 ? 1 : -1);
                    settings[i]=MuscleMechanics.DemonstrationSettings(muscles[i].RestLength);
                }
                ownedRoot.SetActive(true);
                // Only overlapping neighbors are ignored; non-adjacent self-contact remains physical.
                for(int i=1;i<colliders.Length;i++) Physics2D.IgnoreCollision(colliders[i-1],colliders[i],true);
                IsInitialized=true;
            }
            catch(Exception error) { Fail(error); }
        }
        private void FixedUpdate()
        {
            if(!IsInitialized) return;
            try
            {
                for(int i=0;i<bodies.Length;i++)
                    if(bodies[i]==null || colliders[i]==null || !bodies[i].simulated
                        || !bodies[i].gameObject.activeInHierarchy || !colliders[i].enabled
                        || (i>0 && (joints[i-1]==null || !joints[i-1].enabled)))
                        throw new InvalidOperationException("Physical topology was removed; recreate organism.");
                PhysicsSteps++;
                // Evaluate the whole force batch before publishing it to native physics.
                for(int i=0;i<muscles.Length;i++)
                {
                    MuscleSpec m=muscles[i];Rigidbody2D a=bodies[m.Parent],b=bodies[m.Child];
                    // Unity GetRelativePoint maps local -> world; GetPoint is the inverse.
                    pointA[i]=a.GetRelativePoint(new Vector2((float)m.ParentAnchor.X,(float)m.ParentAnchor.Y));
                    pointB[i]=b.GetRelativePoint(new Vector2((float)m.ChildAnchor.X,(float)m.ChildAnchor.Y));
                    Vector2 delta=pointB[i]-pointA[i];float length=delta.magnitude;
                    float rate=length>1e-8f ? Vector2.Dot(b.GetPointVelocity(pointB[i])-a.GetPointVelocity(pointA[i]),delta/length) : 0f;
                    activation[i]=MuscleMechanics.Activate(activation[i],musclesEnabled ? commands[i] : 0,8,Time.fixedDeltaTime);
                    tension[i]=musclesEnabled ? MuscleMechanics.Tension(settings[i],length,rate,activation[i]) : 0;
                    lengths[i]=length;lengthRates[i]=rate;
                    targets[i]=MuscleMechanics.TargetLength(settings[i],activation[i]);
                    Point2 f=MuscleMechanics.ForceOnParent(new Point2(pointA[i].x,pointA[i].y),
                        new Point2(pointB[i].x,pointB[i].y),tension[i]);
                    forceA[i]=new Vector2((float)f.X,(float)f.Y);
                }
                for(int i=0;i<muscles.Length;i++)
                {
                    bodies[muscles[i].Parent].AddForceAtPosition(forceA[i],pointA[i],ForceMode2D.Force);
                    bodies[muscles[i].Child].AddForceAtPosition(-forceA[i],pointB[i],ForceMode2D.Force);
                }
            }
            catch(Exception error) { Fail(error); }
        }
        private void Fail(Exception error)
        {
            IsInitialized=false;Release();enabled=false;Debug.LogException(error,this);
        }
        private void OnDisable()
        {
            ClearCommands();
            if(activation!=null) Array.Clear(activation,0,activation.Length);
            if(tension!=null) Array.Clear(tension,0,tension.Length);
            if(forceA!=null) Array.Clear(forceA,0,forceA.Length);
            if(ownedRoot!=null) ownedRoot.SetActive(false);
        }
        private void OnEnable()
        {
            if(IsInitialized && ownedRoot!=null)
            {
                ownedRoot.SetActive(true);
                for(int i=1;i<colliders.Length;i++)
                    if(colliders[i-1]!=null && colliders[i]!=null) Physics2D.IgnoreCollision(colliders[i-1],colliders[i],true);
            }
        }
        private void OnDestroy() { IsInitialized=false;Release(); }
        private void Release()
        {
            if(ownedRoot!=null) { ownedRoot.SetActive(false);Destroy(ownedRoot); }
            if(contactMaterial!=null) Destroy(contactMaterial);
            ownedRoot=null;contactMaterial=null;
        }
    }
}
