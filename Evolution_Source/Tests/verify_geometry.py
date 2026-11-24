"""Independent reference math + source wiring checks; NOT a C#/Unity execution."""
import itertools
import math
from pathlib import Path
import random
import unittest

ROOT = Path(__file__).resolve().parents[1]

def contour(shape, n):
    w, h, f, d, l = shape
    result = []
    for i in range(n):
        a = 2 * math.pi * i / n
        c, s = math.cos(a), math.sin(a)
        r = 1 + f*c + d*s + l*math.cos(2*a)
        result.append((w*r*c, h*r*s))
    return result

def cross(a, b, c):
    return (b[0]-a[0])*(c[1]-a[1])-(b[1]-a[1])*(c[0]-a[0])

def intersects(a, b, c, d):
    def on(a, b, p):
        return abs(cross(a,b,p)) <= 1e-12 and all(min(a[k], b[k])-1e-12 <= p[k] <= max(a[k], b[k])+1e-12 for k in (0,1))
    proper = cross(a,b,c)*cross(a,b,d)<0 and cross(c,d,a)*cross(c,d,b)<0
    return proper or on(a,b,c) or on(a,b,d) or on(c,d,a) or on(c,d,b)

def approach(start, target, response, dt):
    a = 1-math.exp(-response*dt)
    return tuple(x+(y-x)*a for x,y in zip(start,target))

class GeometryTests(unittest.TestCase):
    def assert_geometry(self, shape, n):
        points=contour(shape,n)
        area=0
        for i,a in enumerate(points):
            b=points[(i+1)%n]
            wedge=a[0]*b[1]-a[1]*b[0]
            self.assertGreater(wedge, 1e-12)
            self.assertTrue(all(math.isfinite(v) for v in a))
            self.assertLessEqual(abs(a[0]),shape[0]*1.46+1e-12)
            self.assertLessEqual(abs(a[1]),shape[1]*1.46+1e-12)
            area+=wedge/2
            for j in range(i+2,n):
                if i==0 and j==n-1: continue
                self.assertFalse(intersects(a,b,points[j],points[(j+1)%n]))
        self.assertGreater(area,0)
        triangles=[(0,i+1,(i+1)%n+1) for i in range(n)]
        self.assertEqual(len(triangles),n)
        self.assertEqual(triangles[-1],(0,n,1))
        self.assertTrue(all(0<=v<=n for t in triangles for v in t))

    def test_extreme_shapes(self):
        for n in (24,32,64,128):
            for shape in itertools.product((.15,2.5),(.15,2),(-.16,.16),(-.16,.16),(-.14,.14)):
                self.assert_geometry(shape,n)

    def test_seeded_random_shapes(self):
        rng=random.Random(20261124)
        for _ in range(80):
            shape=tuple(rng.uniform(a,b) for a,b in ((.15,2.5),(.15,2),(-.16,.16),(-.16,.16),(-.14,.14)))
            self.assert_geometry(shape,rng.choice((24,32,64,128)))

    def test_transition_is_bounded_and_valid(self):
        start=(.15,2,.16,-.16,.14)
        target=(2.5,.15,-.16,.16,-.14)
        value=start
        for i in range(100):
            next_value=approach(value,target,.8,.02)
            for v,n,t in zip(value,next_value,target):
                self.assertLessEqual(abs(n-t),abs(v-t)+1e-14)
                self.assertLessEqual(abs(n-v),abs(t-v)*.8*.02+1e-14)
            self.assert_geometry(next_value,32)
            value=next_value

    def test_constant_target_timestep_independence(self):
        start=(.85,.58,.035,.045,.025)
        target=(2.5,.15,-.16,.16,-.14)
        expected=approach(start,target,.8,3)
        for hz in (30,50,60,144):
            value=start
            for _ in range(hz*3): value=approach(value,target,.8,1/hz)
            for a,b in zip(value,expected): self.assertAlmostEqual(a,b,places=12)

    def test_csharp_contracts_present(self):
        morphology=(ROOT/'Code/Core/Morphology.cs').read_text()
        geometry=(ROOT/'Code/Core/RadialGeometry.cs').read_text()
        validator=(ROOT/'Code/Core/ContourValidator.cs').read_text()
        self.assertIn('Math.Exp(-response * dt)',morphology)
        self.assertIn('!double.IsNaN(value) && !double.IsInfinity(value)',morphology)
        self.assertIn('(i + 1) % resolution + 1',geometry)
        self.assertIn('shape = shape.Validated()',geometry)
        self.assertIn('OnSegment(a, b, c, abC)',validator)
        for path in (ROOT/'Code/Core').glob('*.cs'):
            self.assertNotIn('UnityEngine',path.read_text())

if __name__=='__main__': unittest.main(verbosity=2)
