using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Evolution.Articulated.Unity;

namespace Evolution.Articulated.Tests
{
    // Prepared native tests only. Must run in a clean target-Editor test scene.
    public sealed class TimeControlIntegrationTests
    {
        private GameObject host,falling;
        private float previous,step;
        [UnitySetUp] public IEnumerator SetUp()
        { previous=Time.timeScale;step=Time.fixedDeltaTime;Time.timeScale=.75f;yield return null; }
        [UnityTearDown] public IEnumerator TearDown()
        {
            if(host!=null) Object.Destroy(host);if(falling!=null) Object.Destroy(falling);
            yield return null;Time.timeScale=previous;
        }
        [UnityTest] public IEnumerator ModesPauseResumeAndCleanup()
        {
            host=new GameObject("Time lease test");var time=host.AddComponent<SimulationTimeControl2D>();time.ShowControls=false;
            Assert.AreEqual(1,Time.timeScale);
            foreach(float mode in new[]{1f,2f,5f,20f})
            { Assert.IsTrue(time.SelectScale(mode));yield return null;Assert.AreEqual(mode,Time.timeScale);Assert.AreEqual(step,Time.fixedDeltaTime); }
            time.SelectScale(1);falling=new GameObject("Physical pause witness");var body=falling.AddComponent<Rigidbody2D>();
            yield return new WaitForSecondsRealtime(.1f);
            time.SelectScale(0);yield return null;
            long ticks=time.PhysicsSteps;Vector2 position=body.position;
            yield return new WaitForSecondsRealtime(.15f);
            Assert.AreEqual(ticks,time.PhysicsSteps);Assert.AreEqual(position,body.position);
            time.TogglePause();Assert.AreEqual(1,Time.timeScale);
            yield return new WaitForSecondsRealtime(.15f);Assert.Greater(time.PhysicsSteps,ticks);Assert.Less(body.position.y,position.y);
            time.SelectScale(20);time.enabled=false;Assert.AreEqual(.75f,Time.timeScale);
            time.enabled=true;Assert.AreEqual(1,Time.timeScale);
            time.SelectScale(0);Object.Destroy(time);yield return null;Assert.AreEqual(.75f,Time.timeScale);Assert.AreEqual(step,Time.fixedDeltaTime);
        }
        [UnityTest] public IEnumerator ExternalTimeWriterIsNotOverriddenOnRelease()
        {
            host=new GameObject("External time test");var time=host.AddComponent<SimulationTimeControl2D>();
            time.SelectScale(20);Time.timeScale=.5f;time.enabled=false;
            Assert.AreEqual(.5f,Time.timeScale);yield return null;
        }
        [UnityTest] public IEnumerator SurfaceSixtyGameSecondsAt20x()
        {
            host=new GameObject("20x surface fixture");host.AddComponent<SimulationTimeControl2D>();var demo=host.AddComponent<ArticulatedDemo2D>();
            yield return null;yield return null;
            Assert.IsNotNull(demo.Organism);Assert.IsTrue(demo.Organism.IsInitialized);
            var time=host.GetComponent<SimulationTimeControl2D>();time.ShowControls=false;time.SelectScale(20);
            long start=time.PhysicsSteps;float deadline=Time.realtimeSinceStartup+15;
            float maxGap=0;bool touched=false;
            while((time.PhysicsSteps-start)*step<60 && Time.realtimeSinceStartup<deadline)
            {
                yield return null;Assert.IsTrue(demo.Organism.IsInitialized,"Physical topology failed.");
                for(int i=0;i<demo.Organism.BoneCount;i++)
                {
                    var body=demo.Organism.GetBody(i);Assert.IsFalse(float.IsNaN(body.position.x)||float.IsInfinity(body.position.x)||float.IsNaN(body.position.y)||float.IsInfinity(body.position.y));
                    touched|=demo.Organism.HasContact(i);Assert.Less(Mathf.Abs(body.position.x),15);Assert.Greater(body.position.y,-2);
                    if(i==0) continue;
                    var j=demo.Organism.GetJoint(i-1);
                    maxGap=Mathf.Max(maxGap,Vector2.Distance(body.GetRelativePoint(j.anchor),j.connectedBody.GetRelativePoint(j.connectedAnchor)));
                    Assert.Less(Mathf.Abs(j.jointAngle),50);
                }
            }
            Assert.GreaterOrEqual((time.PhysicsSteps-start)*step,60,"60 game seconds not reached; profile real throughput.");
            Assert.IsTrue(touched);Assert.Less(maxGap,.02f);Assert.AreEqual(step,Time.fixedDeltaTime);
            Debug.Log($"Native 20x sample: measured {time.MeasuredSpeed:F2}x, {time.MeasuredStepsPerSecond:F1} steps/s; max render-sampled anchor gap {maxGap:F6} m. Profile sub-frame peaks separately.");
        }
    }
}
