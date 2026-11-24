using System;

namespace Evolution.Source.Core
{
    public static class ContourValidator
    {
        private const double Epsilon = 1e-12;
        public static bool IsValidFan(Point2[] contour)
        {
            if (contour == null || contour.Length < RadialGeometry.MinResolution
                || contour.Length > RadialGeometry.MaxResolution) return false;
            for (int i = 0; i < contour.Length; i++)
            {
                Point2 a = contour[i], b = contour[(i + 1) % contour.Length];
                if (!Morphology.Finite(a.X) || !Morphology.Finite(a.Y)) return false;
                if (a.X * b.Y - a.Y * b.X <= Epsilon) return false;
                for (int j = i + 1; j < contour.Length; j++)
                {
                    if (j == i + 1 || (i == 0 && j == contour.Length - 1)) continue;
                    if (Intersects(a, b, contour[j], contour[(j + 1) % contour.Length])) return false;
                }
            }
            return true;
        }

        private static double Cross(Point2 a, Point2 b, Point2 c) =>
            (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X);

        // Also rejects touching and collinear overlapping non-adjacent edges.
        private static bool Intersects(Point2 a, Point2 b, Point2 c, Point2 d)
        {
            double abC = Cross(a, b, c), abD = Cross(a, b, d);
            double cdA = Cross(c, d, a), cdB = Cross(c, d, b);
            if (((abC > Epsilon && abD < -Epsilon) || (abC < -Epsilon && abD > Epsilon))
                && ((cdA > Epsilon && cdB < -Epsilon) || (cdA < -Epsilon && cdB > Epsilon))) return true;
            return OnSegment(a, b, c, abC) || OnSegment(a, b, d, abD)
                || OnSegment(c, d, a, cdA) || OnSegment(c, d, b, cdB);
        }

        private static bool OnSegment(Point2 a, Point2 b, Point2 p, double cross) =>
            Math.Abs(cross) <= Epsilon && p.X >= Math.Min(a.X, b.X) - Epsilon
            && p.X <= Math.Max(a.X, b.X) + Epsilon && p.Y >= Math.Min(a.Y, b.Y) - Epsilon
            && p.Y <= Math.Max(a.Y, b.Y) + Epsilon;
    }
}
