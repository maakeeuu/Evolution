using System;

namespace Evolution.Source.Core
{
    // Value data only: no learning state, environment pressure or inheritance here.
    public readonly struct Morphology
    {
        public readonly double Width, Height, ForwardBias, DorsalBias, LateralLobes;
        public static Morphology Default => new Morphology(0.85, 0.58, 0.035, 0.045, 0.025);

        public Morphology(double width, double height, double forwardBias,
            double dorsalBias, double lateralLobes)
        {
            Width = ClampFinite(width, 0.15, 2.5);
            Height = ClampFinite(height, 0.15, 2.0);
            ForwardBias = ClampFinite(forwardBias, -0.16, 0.16);
            DorsalBias = ClampFinite(dorsalBias, -0.16, 0.16);
            LateralLobes = ClampFinite(lateralLobes, -0.14, 0.14);
        }

        public Morphology Validated() => new Morphology(Width, Height,
            ForwardBias, DorsalBias, LateralLobes);

        // Exact exponential response for a constant target. Subdividing dt gives
        // the same result, unlike Euler interpolation. No overshoot or snapping.
        public static Morphology Approach(Morphology from, Morphology target,
            double response, double dt)
        {
            if (!Finite(response) || response < 0) throw new ArgumentOutOfRangeException(nameof(response));
            if (!Finite(dt) || dt < 0) throw new ArgumentOutOfRangeException(nameof(dt));
            from = from.Validated();
            target = target.Validated();
            double decay = response * dt;
            // Avoid cancellation of 1-exp(-x) for very small simulation steps.
            double a = decay < 1e-5
                ? decay * (1 - decay * .5 + decay * decay / 6)
                : 1.0 - Math.Exp(-decay);
            return new Morphology(
                from.Width + (target.Width - from.Width) * a,
                from.Height + (target.Height - from.Height) * a,
                from.ForwardBias + (target.ForwardBias - from.ForwardBias) * a,
                from.DorsalBias + (target.DorsalBias - from.DorsalBias) * a,
                from.LateralLobes + (target.LateralLobes - from.LateralLobes) * a);
        }

        public static bool Finite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
        private static double ClampFinite(double value, double min, double max)
        {
            if (!Finite(value)) throw new ArgumentOutOfRangeException(nameof(value), "Expected a finite value.");
            return Math.Max(min, Math.Min(max, value));
        }
    }
}
