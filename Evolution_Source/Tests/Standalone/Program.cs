using System;
using Evolution.Source.Core;

internal static class Program
{
    private static int checks;
    private static void Check(bool ok, string name)
    {
        checks++;
        if (!ok) throw new Exception(name);
    }
    private static void Reject(Action action)
    {
        bool rejected = false;
        try { action(); } catch (ArgumentOutOfRangeException) { rejected = true; }
        Check(rejected, "Invalid input accepted");
    }
    public static void Main()
    {
        var random = new Random(20261124);
        foreach (int n in new[] {24, 32, 64, 128})
        {
            var contour = new Point2[n];
            int[] fan = RadialGeometry.CreateFan(n);
            Check(fan.Length == n * 3, "Triangle count");
            for (int i = 0; i < n; i++)
                Check(fan[3*i] == 0 && fan[3*i+1] == i+1
                    && fan[3*i+2] == (i+1)%n+1, "Stable fan indices");
            for (int mask = 0; mask < 32; mask++)
            {
                var m = new Morphology((mask & 1)==0 ? .15 : 2.5, (mask & 2)==0 ? .15 : 2,
                    (mask & 4)==0 ? -.16 : .16, (mask & 8)==0 ? -.16 : .16,
                    (mask & 16)==0 ? -.14 : .14);
                RadialGeometry.Fill(m, contour);
                Check(ContourValidator.IsValidFan(contour), "Extreme contour");
            }
            for (int k = 0; k < 100; k++)
            {
                var m = new Morphology(.15+random.NextDouble()*2.35, .15+random.NextDouble()*1.85,
                    random.NextDouble()*.32-.16, random.NextDouble()*.32-.16, random.NextDouble()*.28-.14);
                RadialGeometry.Fill(m, contour);
                Check(ContourValidator.IsValidFan(contour), "Seeded random contour");
            }
            RadialGeometry.Fill(Morphology.Default, contour);
            contour[1] = contour[0];
            Check(!ContourValidator.IsValidFan(contour), "Duplicate edge accepted");
        }
        var start = Morphology.Default;
        var target = new Morphology(2.5, .15, -.16, -.16, .14);
        var direct = Morphology.Approach(start, target, .8, 3);
        foreach (int hz in new[] {30, 50, 60, 144})
        {
            var current = start;
            for (int i = 0; i < hz*3; i++) current = Morphology.Approach(current, target, .8, 1.0/hz);
            Check(Math.Abs(current.Width-direct.Width)<1e-12
                && Math.Abs(current.Height-direct.Height)<1e-12
                && Math.Abs(current.ForwardBias-direct.ForwardBias)<1e-12
                && Math.Abs(current.DorsalBias-direct.DorsalBias)<1e-12
                && Math.Abs(current.LateralLobes-direct.LateralLobes)<1e-12, "Timestep dependence");
        }
        Check(Morphology.Approach(start, target, .8, 0).Width == start.Width, "Zero dt");
        Reject(() => new Morphology(double.NaN, 1, 0, 0, 0));
        Reject(() => new Morphology(1, double.PositiveInfinity, 0, 0, 0));
        Reject(() => Morphology.Approach(start, target, .8, -1));
        Reject(() => Morphology.Approach(start, target, double.NaN, 1));
        Reject(() => RadialGeometry.CreateFan(23));
        Reject(() => RadialGeometry.CreateFan(129));
        Console.WriteLine("PASS: " + checks + " C# core assertions");
    }
}
