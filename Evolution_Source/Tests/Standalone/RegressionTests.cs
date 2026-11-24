using System;
using Evolution.Source.Core;

internal static class RegressionTests
{
    public static void Run(Action<bool, string> check)
    {
        NumericRegressions(check);
        QuantizedGeometry(check);
        MotionProperties(check);
    }

    private static void NumericRegressions(Action<bool, string> check)
    {
        var low = new Morphology(.15, .15, -.16, -.16, -.14);
        var high = new Morphology(2.5, 2, .16, .16, .14);
        check(Morphology.Approach(low, high, 1, 1e-17).Width > low.Width,
            "Tiny positive timestep lost to cancellation");
        var huge = new Point2(double.MaxValue, double.MaxValue);
        var same = PlanarMotion.ApproachVelocity(huge, new Point2(-1, -1), double.MaxValue, 1, 0);
        check(same.X == huge.X && same.Y == huge.Y, "Zero dt must preserve finite velocity");
        same = PlanarMotion.ApproachVelocity(huge, new Point2(-1, -1), double.MaxValue, 0, 1);
        check(same.X == huge.X && same.Y == huge.Y, "Zero acceleration must preserve velocity");
        foreach (double invalid in new[] { double.NaN, double.PositiveInfinity, double.NegativeInfinity })
        {
            Reject(() => new Morphology(invalid, 1, 0, 0, 0), check);
            Reject(() => new Morphology(1, invalid, 0, 0, 0), check);
            Reject(() => new Morphology(1, 1, invalid, 0, 0), check);
            Reject(() => new Morphology(1, 1, 0, invalid, 0), check);
            Reject(() => new Morphology(1, 1, 0, 0, invalid), check);
            Reject(() => Morphology.Approach(low, high, invalid, 1), check);
            Reject(() => Morphology.Approach(low, high, 1, invalid), check);
            Reject(() => PlanarMotion.ApproachVelocity(new Point2(0, 0), new Point2(invalid, 0), 2, 8, 1), check);
            Reject(() => PlanarMotion.ApproachVelocity(new Point2(0, invalid), new Point2(0, 0), 2, 8, 1), check);
        }
        var clamped = new Morphology(-100, 100, -100, 100, 100);
        check(clamped.Width == .15 && clamped.Height == 2 && clamped.ForwardBias == -.16
            && clamped.DorsalBias == .16 && clamped.LateralLobes == .14, "Morphology clamp bounds");
        check(default(Morphology).Validated().Width == .15, "Default struct normalization");
    }

    private static void QuantizedGeometry(Action<bool, string> check)
    {
        var random = new Random(6000610);
        for (int n = 24; n <= 128; n++)
        {
            var points = new Point2[n];
            var raw = new Point2[n];
            int[] triangles = RadialGeometry.CreateFan(n);
            for (int sample = 0; sample < 36; sample++)
            {
                Morphology shape = sample < 32
                    ? new Morphology((sample&1)==0?.15:2.5, (sample&2)==0?.15:2,
                        (sample&4)==0?-.16:.16, (sample&8)==0?-.16:.16, (sample&16)==0?-.14:.14)
                    : new Morphology(.15+2.35*random.NextDouble(), .15+1.85*random.NextDouble(),
                        -.16+.32*random.NextDouble(), -.16+.32*random.NextDouble(), -.14+.28*random.NextDouble());
                RadialGeometry.Fill(shape, raw);
                RadialGeometry.FillFloatCompatible(shape, points);
                check(ContourValidator.IsValidFan(points), "Float contour invalid at n="+n);
                double polygonArea = 0, fanArea = 0;
                for (int i = 0; i < n; i++)
                {
                    Point2 p = points[i], q = points[(i+1)%n];
                    check(p.X == (double)(float)raw[i].X && p.Y == (double)(float)raw[i].Y,
                        "Float sampling mismatch");
                    check(Math.Abs(p.X) <= shape.Width*1.46+1e-6 && Math.Abs(p.Y) <= shape.Height*1.46+1e-6,
                        "Quantized contour escaped bounds");
                    double dx = p.X-q.X, dy = p.Y-q.Y;
                    check(dx*dx+dy*dy > 1e-10, "Float edge collapsed");
                    polygonArea += (p.X*q.Y-p.Y*q.X)*.5;
                    Point2 a = Vertex(points, triangles[3*i]);
                    Point2 b = Vertex(points, triangles[3*i+1]);
                    Point2 c = Vertex(points, triangles[3*i+2]);
                    double triangleArea = ((b.X-a.X)*(c.Y-a.Y)-(b.Y-a.Y)*(c.X-a.X))*.5;
                    check(triangleArea > 0, "Triangle winding/degeneracy");
                    fanArea += triangleArea;
                }
                check(Math.Abs(fanArea-polygonArea) < 1e-11, "Fan does not cover polygon area exactly");
                double analytic = Math.PI*shape.Width*shape.Height*(1+
                    (shape.ForwardBias*shape.ForwardBias+shape.DorsalBias*shape.DorsalBias
                        +shape.LateralLobes*shape.LateralLobes)*.5);
                check(Math.Abs(polygonArea-analytic)/analytic < .04, "Radial area discretization bound");
            }
            RadialGeometry.FillFloatCompatible(Morphology.Default, points);
            Array.Reverse(points);
            check(!ContourValidator.IsValidFan(points), "Clockwise contour accepted");
            RadialGeometry.FillFloatCompatible(Morphology.Default, points);
            points[1] = points[0];
            check(!ContourValidator.IsValidFan(points), "Duplicate point accepted");
            RadialGeometry.FillFloatCompatible(Morphology.Default, points);
            Point2 swap=points[2]; points[2]=points[n/2]; points[n/2]=swap;
            check(!ContourValidator.IsValidFan(points), "Self-intersection accepted");
            RadialGeometry.FillFloatCompatible(Morphology.Default, points);
            points[3]=new Point2(double.NaN,0);
            check(!ContourValidator.IsValidFan(points), "NaN point accepted");
            for(int i=0;i<n;i++) points[i]=new Point2(1e300*Math.Cos(i*2*Math.PI/n),1e300*Math.Sin(i*2*Math.PI/n));
            check(!ContourValidator.IsValidFan(points), "Overflowing area accepted");
        }
        var trajectory=new Point2[64];
        var current=Morphology.Default;
        for(int frame=0;frame<2000;frame++)
        {
            var target=new Morphology(.15+2.35*random.NextDouble(),.15+1.85*random.NextDouble(),
                -.16+.32*random.NextDouble(),-.16+.32*random.NextDouble(),-.14+.28*random.NextDouble());
            current=Morphology.Approach(current,target,.8,.02);
            RadialGeometry.FillFloatCompatible(current,trajectory);
            check(ContourValidator.IsValidFan(trajectory),"Random target trajectory invalid");
        }
        check(!ContourValidator.IsValidFan(null), "Null contour accepted");
        check(!ContourValidator.IsValidFan(new Point2[23]), "Unsupported contour count accepted");
    }

    private static void MotionProperties(Action<bool, string> check)
    {
        var random=new Random(20261124);
        for(int i=0;i<10000;i++)
        {
            var start=new Point2(random.NextDouble()*10-5,random.NextDouble()*10-5);
            var intent=new Point2(random.NextDouble()*2-1,random.NextDouble()*2-1);
            double speed=random.NextDouble()*6, acceleration=random.NextDouble()*30, dt=random.NextDouble()*.1;
            Point2 next=PlanarMotion.ApproachVelocity(start,intent,speed,acceleration,dt);
            double delta=Math.Sqrt((next.X-start.X)*(next.X-start.X)+(next.Y-start.Y)*(next.Y-start.Y));
            check(Morphology.Finite(next.X)&&Morphology.Finite(next.Y),"Nonfinite velocity");
            check(delta<=acceleration*dt+1e-12,"Acceleration bound violated");
            Point2 target=PlanarMotion.ApproachVelocity(start,intent,speed,acceleration,1000000);
            double before=Math.Sqrt((target.X-start.X)*(target.X-start.X)+(target.Y-start.Y)*(target.Y-start.Y));
            double after=Math.Sqrt((target.X-next.X)*(target.X-next.X)+(target.Y-next.Y)*(target.Y-next.Y));
            check(after<=before+1e-12,"Velocity overshot target");
        }
    }

    private static Point2 Vertex(Point2[] points,int index) => index==0 ? new Point2(0,0) : points[index-1];
    private static void Reject(Action action,Action<bool,string> check)
    {
        bool rejected=false;
        try { action(); } catch(ArgumentOutOfRangeException) { rejected=true; }
        check(rejected,"Nonfinite input was accepted");
    }
}
