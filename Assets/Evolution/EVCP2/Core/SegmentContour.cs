using System;
using Evolution.Source.Core;

namespace Evolution.Articulated.Core
{
    // Convex capsule about the origin. The straight centerline joins the skeletal anchors.
    // Rounded end disks overlap across a hinge. Quantized vertices are shared by mesh/collider.
    public static class SegmentContour
    {
        public const int Resolution=34;
        public static Point2[] Create(double length,double radius)
        {
            Checks.Range(length,.25,2,nameof(length));Checks.Range(radius,.04,.25,nameof(radius));
            if(radius>length*.45) throw new ArgumentOutOfRangeException(nameof(radius));
            var points=new Point2[Resolution];
            const int half=Resolution/2;
            for(int i=0;i<half;i++)
            {
                double angle=-Math.PI*.5+Math.PI*i/(half-1);
                points[i]=new Point2((float)(length*.5+radius*Math.Cos(angle)),(float)(radius*Math.Sin(angle)));
                points[i+half]=new Point2((float)(-length*.5-radius*Math.Cos(angle)),(float)(-radius*Math.Sin(angle)));
            }
            if(!ContourValidator.IsValidFan(points)) throw new InvalidOperationException("Invalid capsule contour.");
            return points;
        }
    }
}
