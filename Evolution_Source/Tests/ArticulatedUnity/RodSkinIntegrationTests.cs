using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Evolution.Articulated.Unity;

namespace Evolution.Articulated.Tests
{
    // PREPARED ONLY: requires real target Editor import and Play Mode execution.
    public sealed class RodSkinIntegrationTests
    {
        private GameObject host;
        [UnityTearDown] public IEnumerator Cleanup()
        { if(host!=null) Object.Destroy(host);yield return null;yield return null; }
        [UnityTest] public IEnumerator SkinRefreshDoesNotMutateNativePhysics()
        {
            host=new GameObject("CP3 native read-only gate");
            var organism=host.AddComponent<ArticulatedOrganism2D>();organism.ConfigureIsolatedJoint();
            var skin=host.AddComponent<ArticulatedRodSkin2D>();
            yield return null;yield return null;skin.RefreshSkin();Assert.IsTrue(skin.ValidPose,skin.Status);
            for(int i=0;i<organism.BoneCount;i++)
            {
                var body=organism.GetBody(i);var collider=organism.GetCollider(i);
                Vector2 position=body.position,velocity=body.linearVelocity;float rotation=body.rotation,omega=body.angularVelocity,mass=body.mass;
                Vector2[] path=collider.GetPath(0);Vector2 offset=collider.offset;
                var joint=organism.GetJoint(0);var limits=joint.limits;Vector2 anchor=joint.anchor,connected=joint.connectedAnchor;
                double command=organism.GetCommand(0),activation=organism.GetActivation(0),force=organism.GetTension(0);
                for(int j=0;j<10;j++) skin.RefreshSkin();skin.CaptureCoverage();
                Assert.AreEqual(position,body.position);Assert.AreEqual(velocity,body.linearVelocity);
                Assert.AreEqual(rotation,body.rotation);Assert.AreEqual(omega,body.angularVelocity);Assert.AreEqual(mass,body.mass);
                CollectionAssert.AreEqual(path,collider.GetPath(0));Assert.AreEqual(offset,collider.offset);
                Assert.AreEqual(anchor,joint.anchor);Assert.AreEqual(connected,joint.connectedAnchor);Assert.AreEqual(limits,joint.limits);
                Assert.AreEqual(command,organism.GetCommand(0));Assert.AreEqual(activation,organism.GetActivation(0));Assert.AreEqual(force,organism.GetTension(0));
            }
        }
        [UnityTest] public IEnumerator ActualMuscleBendingChangesPublishedSkin()
        {
            host=new GameObject("CP3 actual native bend gate");
            var organism=host.AddComponent<ArticulatedOrganism2D>();organism.ConfigureIsolatedJoint();
            var skin=host.AddComponent<ArticulatedRodSkin2D>();
            yield return null;yield return null;skin.RefreshSkin();Assert.IsTrue(skin.ValidPose,skin.Status);
            Vector3[] before=host.GetComponentInChildren<MeshFilter>().sharedMesh.vertices;
            float start=organism.GetJoint(0).jointAngle;organism.SetActivationCommand(0,1);
            for(int i=0;i<25;i++) yield return new WaitForFixedUpdate();
            yield return null;skin.RefreshSkin();
            Assert.Greater(Mathf.Abs(organism.GetJoint(0).jointAngle-start),5,"Color/force alone is not bending.");
            Assert.IsTrue(skin.ValidPose,skin.Status);
            Vector3[] after=host.GetComponentInChildren<MeshFilter>().sharedMesh.vertices;
            bool changed=false;for(int i=0;i<before.Length;i++) if((after[i]-before[i]).sqrMagnitude>1e-6f) changed=true;
            Assert.IsTrue(changed,"Published skin must follow actual native pose.");
            Assert.Less(skin.Coverage.PhysicsOutsideSkin,.025);Assert.Less(skin.Coverage.SkinOutsidePhysics,.025);
        }
    }
}
