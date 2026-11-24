using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Evolution.Articulated.Unity;

namespace Evolution.Articulated.Tests
{
    // Prepared only: no remote tool runs or compiles these against Unity.
    public sealed class ArticulatedIntegrationTests
    {
        private GameObject host,ground;
        private ArticulatedOrganism2D organism;
        [UnitySetUp] public IEnumerator SetUp()
        {
            host=new GameObject("EV-CP2 test organism");host.transform.position=new Vector3(-1.5f,1,0);
            organism=host.AddComponent<ArticulatedOrganism2D>();
            yield return null;
            Assert.IsTrue(organism.IsInitialized);
        }
        [UnityTearDown] public IEnumerator TearDown()
        {
            if(host!=null) Object.Destroy(host);if(ground!=null) Object.Destroy(ground);
            yield return null;yield return null;
        }
        [UnityTest] public IEnumerator TopologyAndCoincidentAnchors()
        {
            Assert.AreEqual(4,organism.BoneCount);Assert.AreEqual(6,organism.MuscleCount);
            for(int i=0;i<organism.BoneCount-1;i++)
            {
                var joint=organism.GetJoint(i);
                Assert.IsTrue(joint.useLimits);Assert.IsFalse(joint.useMotor);
                Assert.AreEqual(organism.GetBody(i),joint.connectedBody);
                Vector2 a=joint.transform.TransformPoint(joint.anchor);
                Vector2 b=joint.connectedBody.transform.TransformPoint(joint.connectedAnchor);
                Assert.Less(Vector2.Distance(a,b),.02f);
            }
            yield return null;
        }
        private Vector2 CenterOfMass()
        {
            Vector2 p=Vector2.zero;float mass=0;
            for(int i=0;i<organism.BoneCount;i++)
            { var body=organism.GetBody(i);p+=body.worldCenterOfMass*body.mass;mass+=body.mass; }
            return p/mass;
        }
        [UnityTest] public IEnumerator MusclesBendWithoutExternalPropulsion()
        {
            for(int i=0;i<organism.BoneCount;i++)
            { var body=organism.GetBody(i);body.gravityScale=0;body.linearDamping=0;body.angularDamping=0;body.linearVelocity=Vector2.zero;body.angularVelocity=0; }
            Vector2 before=CenterOfMass();float angle=organism.GetJoint(0).jointAngle;
            organism.SetActivationCommand(0,1);
            double peak=0;
            for(int i=0;i<30;i++)
            { yield return new WaitForFixedUpdate();peak=System.Math.Max(peak,organism.GetTension(0)); }
            Assert.Greater(peak,.1);Assert.Greater(Mathf.Abs(organism.GetJoint(0).jointAngle-angle),.5f);
            Assert.Less(Vector2.Distance(before,CenterOfMass()),.02f,"Internal actuators must not propel an isolated COM.");
        }
        [UnityTest] public IEnumerator NativeRenderAndColliderCoordinatesMatch()
        {
            host.AddComponent<ArticulatedOrganismView2D>();yield return null;yield return new WaitForEndOfFrame();
            for(int i=0;i<organism.BoneCount;i++)
            {
                var filter=organism.GetBody(i).transform.Find("Envelope patch "+i).GetComponent<MeshFilter>();
                Vector2[] polygon=organism.GetCollider(i).GetPath(0);Vector3[] vertices=filter.sharedMesh.vertices;
                Assert.AreEqual(polygon.Length+1,vertices.Length);
                for(int j=0;j<polygon.Length;j++)
                { Assert.AreEqual(polygon[j].x,vertices[j+1].x);Assert.AreEqual(polygon[j].y,vertices[j+1].y); }
            }
        }
        [UnityTest] public IEnumerator PassiveContactAndNoDuplicateOnReenable()
        {
            ground=new GameObject("Flat soil test surface");ground.transform.position=new Vector3(0,-.25f,0);
            ground.AddComponent<BoxCollider2D>().size=new Vector2(20,.5f);
            organism.MusclesEnabled=false;var original=organism.GetBody(0);
            for(int i=0;i<150;i++) yield return new WaitForFixedUpdate();
            bool contact=false;
            for(int i=0;i<organism.BoneCount;i++)
            { contact|=organism.HasContact(i);Assert.Greater(organism.GetBody(i).position.y,-.1f); }
            Assert.IsTrue(contact);
            organism.enabled=false;yield return null;organism.enabled=true;yield return new WaitForFixedUpdate();
            Assert.AreSame(original,organism.GetBody(0));Assert.AreEqual(4,host.GetComponentsInChildren<Rigidbody2D>().Length);
        }
    }
}
