using System;

namespace Evolution.Source.Core
{
    public readonly struct Point2
    {
        public readonly double X, Y;
        public Point2(double x, double y) { X = x; Y = y; }
    }

    // One implicit closed CCW contour; do not duplicate the first point at the end.
    // Positive radial function + increasing angles + positive axis scale =>
    // a simple star-shaped polygon containing the origin, so a fan is valid.
    public static class RadialGeometry
    {
        public const int MinResolution = 24;
        public const int MaxResolution = 128;

        public static void Fill(Morphology shape, Point2[] contour)
        {
            if (contour == null) throw new ArgumentNullException(nameof(contour));
            CheckResolution(contour.Length);
            shape = shape.Validated();
            for (int i = 0; i < contour.Length; i++)
            {
                double angle = i * (2.0 * Math.PI / contour.Length);
                double c = Math.Cos(angle), s = Math.Sin(angle);
                // Lower bound 1 - .16 - .16 - .14 = .54 > 0.
                double radius = 1 + shape.ForwardBias * c + shape.DorsalBias * s
                    + shape.LateralLobes * Math.Cos(2 * angle);
                contour[i] = new Point2(shape.Width * radius * c, shape.Height * radius * s);
            }
        }

        // Vertex 0 is origin; contour[i] corresponds to mesh vertex i + 1.
        public static int[] CreateFan(int resolution)
        {
            CheckResolution(resolution);
            var result = new int[resolution * 3];
            for (int i = 0; i < resolution; i++)
            {
                result[3 * i] = 0;
                result[3 * i + 1] = i + 1;
                result[3 * i + 2] = (i + 1) % resolution + 1;
            }
            return result;
        }

        private static void CheckResolution(int resolution)
        {
            if (resolution < MinResolution || resolution > MaxResolution)
                throw new ArgumentOutOfRangeException(nameof(resolution));
        }
    }
}
