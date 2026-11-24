using System;

namespace Evolution.Source.Core
{
    /// <summary>Acceleration-limited velocity control. No decisions, learning or collision solver.</summary>
    public static class PlanarMotion
    {
        public static Point2 ApproachVelocity(Point2 current, Point2 intent,
            double speed, double acceleration, double dt)
        {
            if (!Morphology.Finite(current.X) || !Morphology.Finite(current.Y)
                || !Morphology.Finite(intent.X) || !Morphology.Finite(intent.Y))
                throw new ArgumentOutOfRangeException(nameof(intent));
            if (!Morphology.Finite(speed) || speed < 0) throw new ArgumentOutOfRangeException(nameof(speed));
            if (!Morphology.Finite(acceleration) || acceleration < 0)
                throw new ArgumentOutOfRangeException(nameof(acceleration));
            if (!Morphology.Finite(dt) || dt < 0) throw new ArgumentOutOfRangeException(nameof(dt));
            double x = Math.Max(-1, Math.Min(1, intent.X));
            double y = Math.Max(-1, Math.Min(1, intent.Y));
            double length = Math.Sqrt(x * x + y * y);
            if (length > 1) { x /= length; y /= length; }
            double targetX = x * speed, targetY = y * speed;
            double dx = targetX - current.X, dy = targetY - current.Y;
            if (!Morphology.Finite(dx) || !Morphology.Finite(dy))
                throw new ArgumentOutOfRangeException(nameof(current), "Velocity difference overflow.");
            double distance = Hypot(dx, dy);
            if (!Morphology.Finite(distance))
                throw new ArgumentOutOfRangeException(nameof(current), "Velocity magnitude overflow.");
            double maxDelta = acceleration * dt;
            if (distance == 0 || distance <= maxDelta) return new Point2(targetX, targetY);
            // Divide before multiplication to avoid squaring large finite velocities.
            return new Point2(current.X + (dx / distance) * maxDelta,
                current.Y + (dy / distance) * maxDelta);
        }

        private static double Hypot(double x, double y)
        {
            double max = Math.Max(Math.Abs(x), Math.Abs(y));
            if (max == 0) return 0;
            return max * Math.Sqrt((x / max) * (x / max) + (y / max) * (y / max));
        }
    }
}
