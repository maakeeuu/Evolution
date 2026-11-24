"""Float-geometry reference and numerical regression checks, not Unity execution."""
import itertools
import math
from pathlib import Path
import struct
import unittest
from verify_geometry import contour

ROOT=Path(__file__).resolve().parents[1]

def f32(value): return struct.unpack('f',struct.pack('f',value))[0]

class PrecisionTests(unittest.TestCase):
    def test_quantized_extremes_all_resolutions(self):
        for n in range(24,129):
            for shape in itertools.product((.15,2.5),(.15,2),(-.16,.16),(-.16,.16),(-.14,.14)):
                raw=contour(shape,n)
                points=[tuple(map(f32,p)) for p in raw]
                polygon_area=0
                for a,b in zip(points,points[1:]+points[:1]):
                    wedge=a[0]*b[1]-a[1]*b[0]
                    self.assertGreater(wedge,1e-12)
                    self.assertGreater(math.dist(a,b),1e-5)
                    polygon_area+=wedge/2
                w,h,f,d,l=shape
                exact=math.pi*w*h*(1+(f*f+d*d+l*l)/2)
                self.assertLess(abs(polygon_area-exact)/exact,.04)

    def test_tiny_step_cancellation_regression(self):
        decay=1e-17
        old=1-math.exp(-decay)
        stable=decay*(1-decay*.5+decay*decay/6)
        self.assertEqual(old,0)
        self.assertEqual(stable,-math.expm1(-decay))
        self.assertGreater(.15+(2.5-.15)*stable,.15)

    def test_core_guards_and_quantization(self):
        radial=(ROOT/'Code/Core/RadialGeometry.cs').read_text()
        validator=(ROOT/'Code/Core/ContourValidator.cs').read_text()
        motion=(ROOT/'Code/Core/PlanarMotion.cs').read_text()
        self.assertIn('FillFloatCompatible',radial)
        self.assertIn('(float)contour[i].X',radial)
        self.assertIn('!Morphology.Finite(wedge)',validator)
        self.assertIn('if (dt == 0 || acceleration == 0) return current;',motion)

if __name__=='__main__': unittest.main(verbosity=2)
