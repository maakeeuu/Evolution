using System;
using Evolution.Source.Core;

namespace Evolution.Articulated.Core
{
    public readonly struct SegmentSpec
    {
        public readonly int Parent;
        public readonly double Length, Radius, Mass, LimitDegrees;
        public SegmentSpec(int parent, double length, double radius, double mass, double limitDegrees)
        {
            Checks.Range(length, .25, 2, nameof(length));
            Checks.Range(radius, .04, .25, nameof(radius));
            Checks.Range(mass, .05, 5, nameof(mass));
            Checks.Range(limitDegrees, 5, 60, nameof(limitDegrees));
            if (parent < -1 || radius > length * .45) throw new ArgumentOutOfRangeException(nameof(parent));
            Parent=parent; Length=length; Radius=radius; Mass=mass; LimitDegrees=limitDegrees;
        }
    }

    // Initial developmental rule: serial repetition with a bounded proportional taper.
    // No animal-specific parts, random body selection, or lifetime adaptation policy.
    public sealed class ChainDefinition
    {
        private readonly SegmentSpec[] segments;
        public int Count => segments.Length;
        public SegmentSpec this[int index] => segments[index];
        private ChainDefinition(SegmentSpec[] values) { segments=values; }
        public static ChainDefinition Create(int count=4, double length=.85, double radius=.14,
            double taper=.96, double limitDegrees=40)
        {
            if(count<2 || count>8) throw new ArgumentOutOfRangeException(nameof(count));
            Checks.Range(length,.5,1.5,nameof(length));
            Checks.Range(radius,.08,.2,nameof(radius));
            Checks.Range(taper,.94,1,nameof(taper));
            Checks.Range(limitDegrees,5,60,nameof(limitDegrees));
            var result=new SegmentSpec[count];
            for(int i=0;i<count;i++)
            {
                double scale=Math.Pow(taper,i);
                result[i]=new SegmentSpec(i-1,length*scale,radius*scale,.35*scale*scale,limitDegrees);
            }
            return new ChainDefinition(result);
        }
        public double CenterX(int index)
        {
            if(index<0 || index>=Count) throw new ArgumentOutOfRangeException(nameof(index));
            double x=0;
            for(int i=0;i<index;i++) x+=segments[i].Length;
            return x+segments[index].Length*.5;
        }
    }

    internal static class Checks
    {
        internal static void Range(double x,double min,double max,string name)
        {
            if(!Morphology.Finite(x) || x<min || x>max) throw new ArgumentOutOfRangeException(name);
        }
    }
}
