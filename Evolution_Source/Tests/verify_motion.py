"""Independent velocity reference + wiring checks. Does not simulate Unity contacts."""
import math
from pathlib import Path
import random
import unittest

ROOT=Path(__file__).resolve().parents[1]

def velocity(current, intent, speed, acceleration, dt):
    x,y=(max(-1,min(1,v)) for v in intent)
    length=math.hypot(x,y)
    if length>1: x,y=x/length,y/length
    target=(x*speed,y*speed)
    difference=tuple(a-b for a,b in zip(target,current))
    distance=math.hypot(*difference)
    delta=acceleration*dt
    if distance==0 or distance<=delta: return target
    return tuple(v+d/distance*delta for v,d in zip(current,difference))

class MotionTests(unittest.TestCase):
    def test_diagonal_is_normalized(self):
        result=velocity((0,0),(1,1),2,8,1)
        self.assertAlmostEqual(math.hypot(*result),2)
        self.assertAlmostEqual(result[0],result[1])

    def test_acceleration_and_braking(self):
        self.assertEqual(velocity((0,0),(1,0),2,8,.02),(.16,0))
        self.assertEqual(velocity((2,0),(0,0),2,8,1),(0,0))
        self.assertEqual(velocity((2,0),(1,0),2,8,.02),(2,0))
        self.assertEqual(velocity((2,0),(-1,0),2,8,0),(2,0))

    def test_velocity_step_independence(self):
        # Velocity only: discrete physics positions/contacts are NOT claimed invariant.
        for hz in (30,50,60,144):
            v=(0,0)
            for _ in range(hz): v=velocity(v,(1,1),2,1,1/hz)
            self.assertAlmostEqual(v[0],1/math.sqrt(2),places=12)
            self.assertAlmostEqual(v[1],1/math.sqrt(2),places=12)

    def test_seeded_acceleration_bound(self):
        rng=random.Random(20261124)
        for _ in range(1000):
            current=(rng.uniform(-5,5),rng.uniform(-5,5))
            intent=(rng.uniform(-3,3),rng.uniform(-3,3))
            dt=rng.uniform(0,.04)
            result=velocity(current,intent,2,8,dt)
            self.assertLessEqual(math.dist(current,result),8*dt+1e-12)
            self.assertTrue(all(math.isfinite(v) for v in result))

    def test_motion_is_2d_fixed_step(self):
        text=(ROOT/'Code/Unity/CreatureMotor2D.cs').read_text()
        self.assertIn('Time.fixedDeltaTime',text)
        self.assertIn('CollisionDetectionMode2D.Continuous',text)
        self.assertIn('RigidbodyInterpolation2D.Interpolate',text)
        self.assertIn('body.linearVelocity = new Vector2',text)
        self.assertNotIn('transform.position',text)
        for path in (ROOT/'Code/Unity').glob('*.cs'):
            code=path.read_text()
            self.assertNotIn('using UnityEditor',code)
            self.assertNotIn('RuntimeInitializeOnLoadMethod',code)
        bootstrap=(ROOT/'Code/Unity/EvolutionPrototype2D.cs').read_text()
        self.assertEqual(bootstrap.count('AddComponent<VectorObstacle2D>()'),1)
        self.assertEqual(bootstrap.count('AddComponent<CreatureBody2D>()'),1)
        self.assertIn('motor.SetMovementIntent(Vector2.right)',bootstrap)

    def test_obstacle_mesh_polygon_correspondence(self):
        text=(ROOT/'Code/Unity/VectorObstacle2D.cs').read_text()
        self.assertIn('new Vector3(points[i].x, points[i].y, 0f)',text)
        self.assertIn('polygon.SetPath(0, points)',text)
        self.assertIn('polygon.isTrigger = false',text)
        points=((-0.3,-1.4),(.3,-1.4),(.3,1.4),(-.3,1.4))
        for a,b,c in ((0,1,2),(0,2,3)):
            p,q,r=points[a],points[b],points[c]
            self.assertGreater((q[0]-p[0])*(r[1]-p[1])-(q[1]-p[1])*(r[0]-p[0]),0)

if __name__=='__main__': unittest.main(verbosity=2)
