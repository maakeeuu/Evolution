using System;
using Evolution.Source.Core;
using Evolution.Articulated.Core;

internal static class Program
{
    private static int assertions;
    private static void Check(bool condition,string message)
    { assertions++;if(!condition) throw new Exception(message); }
    private static void Near(double a,double b,double tolerance,string message) => Check(Math.Abs(a-b)<=tolerance,message);
    private static void Reject(Action action)
    { bool rejected=false;try{action();}catch(ArgumentException){rejected=true;}Check(rejected,"invalid input accepted"); }
    public static int Main()
    {
        try { Run();Console.WriteLine("PASS: "+assertions+" EV-CP2 C# assertions (no Unity physics).");return 0; }
        catch(Exception e) { Console.Error.WriteLine(e);return 1; }
    }
    private static void Run()
    {
        for(int count=2;count<=8;count++)
        for(int test=0;test<10;test++)
        {
            var chain=ChainDefinition.Create(count,.5+test/9.0,.08+test/9.0*.12,.94+test/9.0*.06);
            var repeat=ChainDefinition.Create(count,.5+test/9.0,.08+test/9.0*.12,.94+test/9.0*.06);
            Check(chain.Count==count,"segment count");
            for(int i=0;i<count;i++)
            {
                var bone=chain[i];Check(bone.Parent==i-1,"parent order");
                Near(bone.Length,repeat[i].Length,0,"deterministic generation");
                if(i>0) Near(chain.CenterX(i)-bone.Length*.5,
                    chain.CenterX(i-1)+chain[i-1].Length*.5,1e-12,"hinge coincidence");
                var polygon=SegmentContour.Create(bone.Length,bone.Radius);
                Check(ContourValidator.IsValidFan(polygon),"convex shared float contour");
                var triangles=RadialGeometry.CreateFan(polygon.Length);
                double area=0;
                for(int j=0;j<polygon.Length;j++)
                {
                    var a=polygon[j];var b=polygon[(j+1)%polygon.Length];
                    Check(a.X==(float)a.X && a.Y==(float)a.Y,"float publication");
                    area+=(a.X*b.Y-a.Y*b.X)*.5;
                    Check(triangles[j*3]==0 && triangles[j*3+1]==j+1,"fan indices");
                }
                double exact=2*bone.Radius*bone.Length+Math.PI*bone.Radius*bone.Radius;
                Check(Math.Abs(area-exact)/exact<.01,"capsule area");
                if(i==0)continue;
                var upper=new MuscleSpec(chain,i,1);var lower=new MuscleSpec(chain,i,-1);
                Near(upper.RestLength,lower.RestLength,0,"antagonist rest symmetry");
                var settings=new MuscleSettings(upper.RestLength);
                Near(MuscleMechanics.Tension(settings,upper.RestLength,0,0),0,0,"rest is passive");
                Check(MuscleMechanics.Tension(settings,upper.RestLength,0,1)>0,"activation creates tension");
                // Rotated child shortens the upper path, lengthens the lower path.
                double angle=.2,c=Math.Cos(angle),s=Math.Sin(angle);
                double hinge=chain[i-1].Length*.5;
                double ax=upper.ParentAnchor.X,ay=upper.ParentAnchor.Y;
                double bx=hinge+(upper.ChildAnchor.X+bone.Length*.5)*c-upper.ChildAnchor.Y*s;
                double by=(upper.ChildAnchor.X+bone.Length*.5)*s+upper.ChildAnchor.Y*c;
                Point2 force=MuscleMechanics.ForceOnParent(new Point2(ax,ay),new Point2(bx,by),5);
                double childTorque=(bx-hinge)*(-force.Y)-by*(-force.X);
                double parentTorque=(ax-hinge)*force.Y-ay*force.X;
                Near(childTorque+parentTorque,0,1e-12,"internal angular momentum");
                Check(parentTorque<0,"upper muscle has meaningful moment arm");
            }
        }
        var m=new MuscleSettings(.4);
        for(int i=0;i<=1000;i++)
        {
            double length=i*.001;
            double tension=MuscleMechanics.Tension(m,length,0,.7);
            Check(tension>=0 && tension<=m.MaxForce,"bounded pull only");
            var f=MuscleMechanics.ForceOnParent(new Point2(1,2),new Point2(4,6),tension);
            Near(Math.Sqrt(f.X*f.X+f.Y*f.Y),tension,1e-12,"force magnitude");
        }
        Near(MuscleMechanics.Tension(m,0,-10000,1),0,0,"no pushing");
        Near(MuscleMechanics.Tension(m,100,10000,1),m.MaxForce,0,"force cap");
        Near(MuscleMechanics.ForceOnParent(new Point2(0,0),new Point2(0,0),10).X,0,0,"coincident anchors");
        double reference=1-Math.Exp(-8);
        foreach(int hz in new[]{30,50,60,144})
        {
            double activation=0;for(int i=0;i<hz;i++) activation=MuscleMechanics.Activate(activation,1,8,1.0/hz);
            Near(activation,reference,1e-12,"activation is dt invariant");
        }
        Check(MuscleMechanics.Activate(0,1,1,1e-20)>0,"tiny dt not cancelled");
        foreach(double bad in new[]{double.NaN,double.PositiveInfinity,double.NegativeInfinity})
        {
            Reject(()=>ChainDefinition.Create(4,bad));Reject(()=>new MuscleSettings(bad));
            Reject(()=>MuscleMechanics.Activate(0,bad,8,.02));
            Reject(()=>MuscleMechanics.Tension(m,.4,bad,1));
            Reject(()=>MuscleMechanics.ForceOnParent(new Point2(bad,0),new Point2(0,0),1));
        }
        Reject(()=>ChainDefinition.Create(1));Reject(()=>ChainDefinition.Create(9));
        Reject(()=>new MuscleSpec(ChainDefinition.Create(),0,1));
        Reject(()=>MuscleMechanics.Tension(default,.4,0,1));
        Reject(()=>MuscleMechanics.Activate(0,1,8,-.02));
        Reject(()=>SegmentContour.Create(.25,.25));
    }
}
