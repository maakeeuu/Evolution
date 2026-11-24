using System.Collections;
using System.Collections.Generic;
using Evolution.Source.Core;
using Evolution.Source.Unity;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Evolution.Source.Tests
{
    // Opt-in future tests. Not executed by standalone CI or imported into Assets yet.
    public sealed class CreatureIntegrationTests
    {
        private readonly List<GameObject> owned = new List<GameObject>();

        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            foreach (GameObject item in owned) if (item != null) Object.Destroy(item);
            owned.Clear();
            yield return null;
        }

        private GameObject Create(string name, Vector3 position)
        {
            var item = new GameObject(name);
            owned.Add(item);
            item.transform.position = position;
            return item;
        }

        private static void AssertGeometry(GameObject item)
        {
            Mesh mesh = item.GetComponent<MeshFilter>().sharedMesh;
            PolygonCollider2D polygon = item.GetComponent<PolygonCollider2D>();
            Assert.That(polygon.pathCount, Is.EqualTo(1));
            Assert.That(polygon.offset, Is.EqualTo(Vector2.zero));
            Vector2[] path = polygon.GetPath(0);
            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;
            Assert.That(vertices.Length, Is.EqualTo(path.Length + 1));
            Assert.That(triangles.Length, Is.EqualTo(path.Length * 3));
            Assert.That(vertices[0], Is.EqualTo(Vector3.zero));
            var points = new Point2[path.Length];
            for (int i = 0; i < path.Length; i++)
            {
                Assert.That(vertices[i + 1].x, Is.EqualTo(path[i].x));
                Assert.That(vertices[i + 1].y, Is.EqualTo(path[i].y));
                Assert.That(vertices[i + 1].z, Is.Zero);
                Assert.That(triangles[3*i], Is.Zero);
                Assert.That(triangles[3*i+1], Is.EqualTo(i+1));
                Assert.That(triangles[3*i+2], Is.EqualTo((i+1)%path.Length+1));
                points[i] = new Point2(path[i].x, path[i].y);
            }
            Assert.That(ContourValidator.IsValidFan(points), Is.True);
        }

        [UnityTest]
        public IEnumerator MorphingPublishesExactMeshAndColliderCoordinates()
        {
            GameObject item = Create("Morphology equality test", Vector3.zero);
            CreatureBody2D body = item.AddComponent<CreatureBody2D>();
            body.SetMorphologyTarget(new Morphology(2.5, .15, -.16, .16, -.14));
            for (int frame = 0; frame < 120; frame++)
            {
                yield return new WaitForFixedUpdate();
                AssertGeometry(item);
            }
        }

        [UnityTest]
        public IEnumerator StationaryShapeDoesNotRepublishAndReenableDoesNotReset()
        {
            GameObject item = Create("Publication lifecycle test", Vector3.zero);
            CreatureBody2D body = item.AddComponent<CreatureBody2D>();
            body.SetMorphologyTarget(body.CurrentMorphology);
            uint revision = body.GeometryRevision;
            for (int frame=0; frame<4; frame++) yield return new WaitForFixedUpdate();
            Assert.That(body.GeometryRevision, Is.EqualTo(revision));
            body.SetMorphologyTarget(new Morphology(2.5, .15, .16, -.16, .14));
            for (int frame=0; frame<10; frame++) yield return new WaitForFixedUpdate();
            Morphology before = body.CurrentMorphology;
            revision = body.GeometryRevision;
            body.enabled = false;
            yield return new WaitForFixedUpdate();
            body.enabled = true;
            Assert.That(body.CurrentMorphology.Width, Is.EqualTo(before.Width));
            Assert.That(body.CurrentMorphology.Height, Is.EqualTo(before.Height));
            Assert.That(body.GeometryRevision, Is.EqualTo(revision));
            AssertGeometry(item);
        }

        [UnityTest]
        public IEnumerator MovingAndMorphingBodyDoesNotCrossTallObstacle()
        {
            GameObject wall = Create("Tall test obstacle", new Vector3(.8f,0,0));
            wall.transform.localScale = new Vector3(1,10,1);
            wall.AddComponent<VectorObstacle2D>();
            PolygonCollider2D wallCollider = wall.GetComponent<PolygonCollider2D>();
            GameObject item = Create("Moving test creature", new Vector3(-3,0,0));
            CreatureBody2D shape = item.AddComponent<CreatureBody2D>();
            CreatureMotor2D motor = item.AddComponent<CreatureMotor2D>();
            Rigidbody2D rb = item.GetComponent<Rigidbody2D>();
            PolygonCollider2D collider = item.GetComponent<PolygonCollider2D>();
            shape.SetMorphologyTarget(new Morphology(1.2,.8,.1,-.1,.08));
            motor.SetMovementIntent(Vector2.right);
            for(int frame=0;frame<200;frame++)
            {
                yield return new WaitForFixedUpdate();
                AssertGeometry(item);
                Assert.That(collider.bounds.max.x, Is.LessThanOrEqualTo(wallCollider.bounds.min.x + .06f),
                    "Body crossed or penetrated obstacle beyond contact tolerance");
            }
            Assert.That(rb.position.x, Is.GreaterThan(-2f), "Motor did not approach obstacle");
            Assert.That(Physics2D.Distance(collider,wallCollider).distance, Is.GreaterThan(-.06f));
            shape.SetMorphologyTarget(shape.CurrentMorphology);
            motor.SetMovementIntent(Vector2.zero);
            for(int frame=0;frame<30;frame++) yield return new WaitForFixedUpdate();
            Assert.That(rb.linearVelocity.magnitude, Is.LessThan(.02f));
        }
    }
}
