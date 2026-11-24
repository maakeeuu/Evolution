using System;
using Evolution.Source.Core;

namespace Evolution.Articulated.Core
{
    public readonly struct MuscleSpec
    {
        public readonly int Parent, Child;
        public readonly Point2 ParentAnchor, ChildAnchor;
        public readonly double RestLength;
        public MuscleSpec(ChainDefinition chain,int child,int side)
        {
            if(chain==null) throw new ArgumentNullException(nameof(chain));
            if(child<1 || child>=chain.Count || (side!=-1 && side!=1))
                throw new ArgumentOutOfRangeException(nameof(child));
            Parent=chain[child].Parent; Child=child;
            ParentAnchor=new Point2(chain[Parent].Length*.25,side*chain[Parent].Radius*.72);
            ChildAnchor=new Point2(-chain[Child].Length*.25,side*chain[Child].Radius*.72);
            double dx=chain[Parent].Length*.25+chain[Child].Length*.25;
            double dy=ChildAnchor.Y-ParentAnchor.Y;
            RestLength=Math.Sqrt(dx*dx+dy*dy);
        }
    }

    public readonly struct MuscleSettings
    {
        public readonly double RestLength, MaxForce, Stiffness, Damping, Contraction;
        public MuscleSettings(double restLength,double maxForce=12,double stiffness=90,
            double damping=1.2,double contraction=.3)
        {
            Checks.Range(restLength,.01,4,nameof(restLength));
            Checks.Range(maxForce,0,1000,nameof(maxForce));
            Checks.Range(stiffness,0,5000,nameof(stiffness));
            Checks.Range(damping,0,100,nameof(damping));
            Checks.Range(contraction,0,.6,nameof(contraction));
            RestLength=restLength;MaxForce=maxForce;Stiffness=stiffness;Damping=damping;Contraction=contraction;
        }
    }

    public static class MuscleMechanics
    {
        // Pull-only series spring/damper. Positive lengthRate means stretching.
        // Units: m, m/s, N/m, Ns/m, N. Never produces a compressive force.
        public static double Tension(MuscleSettings settings,double length,double lengthRate,double activation)
        {
            Checks.Range(length,0,100,nameof(length));
            Checks.Range(lengthRate,-10000,10000,nameof(lengthRate));
            Checks.Range(activation,0,1,nameof(activation));
            // A default(struct) settings value is not a valid muscle.
            Checks.Range(settings.RestLength,.01,4,nameof(settings));
            double target=settings.RestLength*(1-settings.Contraction*activation);
            return Math.Max(0,Math.Min(settings.MaxForce,
                settings.Stiffness*(length-target)+settings.Damping*lengthRate));
        }
        public static double Activate(double current,double command,double rate,double dt)
        {
            Checks.Range(current,0,1,nameof(current));Checks.Range(command,0,1,nameof(command));
            Checks.Range(rate,0,100,nameof(rate));Checks.Range(dt,0,1,nameof(dt));
            if(dt==0 || rate==0 || current==command) return current;
            double decay=rate*dt;
            double a=decay<1e-5 ? decay*(1-decay*.5+decay*decay/6) : 1-Math.Exp(-decay);
            return current+(command-current)*a;
        }
        // Pure force direction shared by tests and adapter; equal/opposite pair preserves net momentum.
        public static Point2 ForceOnParent(Point2 a,Point2 b,double tension)
        {
            Checks.Range(a.X,-100000,100000,nameof(a));Checks.Range(a.Y,-100000,100000,nameof(a));
            Checks.Range(b.X,-100000,100000,nameof(b));Checks.Range(b.Y,-100000,100000,nameof(b));
            Checks.Range(tension,0,1000,nameof(tension));
            double x=b.X-a.X,y=b.Y-a.Y,length=Math.Sqrt(x*x+y*y);
            return length<1e-8 ? new Point2(0,0) : new Point2(x/length*tension,y/length*tension);
        }
    }
}
