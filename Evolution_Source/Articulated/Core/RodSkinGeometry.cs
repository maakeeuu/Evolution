using System;
using Evolution.Source.Core;

namespace Evolution.Articulated.Core
{
    /// <summary>One longitudinal strip, not a set of physical capsule outlines.</summary>
    public sealed class RodSkinGeometry
    {
        public const int CapSections=5;
        public const double MaxAnchorGap=.02;
        public const double MaxMiterRatio=1.2;
        private readonly ChainDefinition anatomy;
        private readonly Point2[] axis,directions;
        public Point2[] Vertices { get; }
        public Point2[] Perimeter { get; }
        public int[] Triangles { get; }
        public double LargestAnchorGap { get; private set; }
        public double LargestMiterRatio { get; private set; }
        public int SectionCount => Vertices.Length/2;
        public RodSkinGeometry(ChainDefinition definition)
        {
            anatomy=definition ?? throw new ArgumentNullException(nameof(definition));
            axis=new Point2[anatomy.Count+1];directions=new Point2[anatomy.Count];
            Vertices=new Point2[2*(axis.Length+2*CapSections)];
            Perimeter=new Point2[Vertices.Length];Triangles=new int[(SectionCount-1)*6];
            for(int i=0;i<SectionCount-1;i++)
            {
                int k=i*6,v=i*2;
                Triangles[k]=v;Triangles[k+1]=v+2;Triangles[k+2]=v+1;
                Triangles[k+3]=v+1;Triangles[k+4]=v+2;Triangles[k+5]=v+3;
            }
        }
        /// <summary>Read-only input snapshots of actual segment endpoints in one coordinate frame.</summary>
        public void Update(Point2[] starts,Point2[] ends)
        {
            if(starts==null || ends==null || starts.Length!=anatomy.Count || ends.Length!=anatomy.Count)
                throw new ArgumentException("One endpoint pair per physical segment required.");
            LargestAnchorGap=0;LargestMiterRatio=1;
            for(int i=0;i<anatomy.Count;i++)
            {
                Finite(starts[i]);Finite(ends[i]);
                double dx=ends[i].X-starts[i].X,dy=ends[i].Y-starts[i].Y;
                double length=Math.Sqrt(dx*dx+dy*dy);
                if(Math.Abs(length-anatomy[i].Length)>anatomy[i].Length*1e-4)
                    throw new ArgumentException("Segment pose length disagrees with anatomy.");
                directions[i]=new Point2(dx/length,dy/length);
                if(i==0) axis[0]=starts[0];
                else
                {
                    double gap=Distance(ends[i-1],starts[i]);LargestAnchorGap=Math.Max(LargestAnchorGap,gap);
                    if(gap>MaxAnchorGap) throw new ArgumentException("Joint anchor gap exceeds .02 m; not concealed by skin.");
                    axis[i]=new Point2((ends[i-1].X+starts[i].X)*.5,(ends[i-1].Y+starts[i].Y)*.5);
                    double dot=directions[i-1].X*directions[i].X+directions[i-1].Y*directions[i].Y;
                    double limit=Math.Min(62,anatomy[i].LimitDegrees+2)*Math.PI/180;
                    if(dot<Math.Cos(limit)-1e-10) throw new ArgumentException("Bend exceeds supported anatomical limit + 2 degrees.");
                }
            }
            axis[axis.Length-1]=ends[ends.Length-1];
            int section=0;
            // Nonzero terminal section at 5 degrees avoids duplicate tip vertices / zero-area triangles.
            // Extension never exceeds the existing radius; cap is sampled by transverse strip sections.
            for(int i=0;i<CapSections;i++) Cap(section++,0,-1,5+17*i);
            for(int i=0;i<axis.Length;i++)
            {
                Point2 d;
                double width;
                if(i==0) { d=directions[0];width=anatomy[0].Radius; }
                else if(i==axis.Length-1) { d=directions[i-1];width=anatomy[i-1].Radius; }
                else
                {
                    var a=directions[i-1];var b=directions[i];
                    double x=a.X+b.X,y=a.Y+b.Y,n=Math.Sqrt(x*x+y*y);
                    d=new Point2(x/n,y/n);
                    double ratio=1/Math.Sqrt((1+a.X*b.X+a.Y*b.Y)*.5);
                    if(ratio>MaxMiterRatio) throw new ArgumentException("Excessive joint expansion.");
                    LargestMiterRatio=Math.Max(LargestMiterRatio,ratio);
                    width=Math.Max(anatomy[i-1].Radius,anatomy[i].Radius)*ratio;
                }
                Section(section++,axis[i],new Point2(-d.Y*width,d.X*width));
            }
            for(int i=CapSections-1;i>=0;i--) Cap(section++,axis.Length-1,1,5+17*i);
            for(int i=0;i<SectionCount;i++)
            { Perimeter[i]=Vertices[2*i];Perimeter[Perimeter.Length-1-i]=Vertices[2*i+1]; }
            ValidateSurface();
        }
        private void Cap(int section,int end,int sign,double degrees)
        {
            int bone=end==0 ? 0 : anatomy.Count-1;
            Point2 d=directions[bone];double r=anatomy[bone].Radius,angle=degrees*Math.PI/180;
            var c=new Point2(axis[end].X+sign*d.X*r*Math.Cos(angle),axis[end].Y+sign*d.Y*r*Math.Cos(angle));
            double width=r*Math.Sin(angle);
            Section(section,c,new Point2(-d.Y*width,d.X*width));
        }
        private void Section(int index,Point2 center,Point2 offset)
        {
            // Validate the exact float-quantized geometry that will be published to a Mesh.
            Vertices[2*index]=new Point2((float)(center.X-offset.X),(float)(center.Y-offset.Y));
            Vertices[2*index+1]=new Point2((float)(center.X+offset.X),(float)(center.Y+offset.Y));
        }
        private void ValidateSurface()
        {
            for(int i=0;i<Triangles.Length;i+=3)
                if(Cross(Vertices[Triangles[i]],Vertices[Triangles[i+1]],Vertices[Triangles[i+2]])<=1e-12)
                    throw new ArgumentException("Skin triangle is degenerate or inverted after float conversion.");
            for(int i=0;i<Perimeter.Length;i++)
            for(int j=i+1;j<Perimeter.Length;j++)
            {
                if(j==i+1 || (i==0 && j==Perimeter.Length-1)) continue;
                if(Intersects(Perimeter[i],Perimeter[(i+1)%Perimeter.Length],Perimeter[j],Perimeter[(j+1)%Perimeter.Length]))
                    throw new ArgumentException("Skin self-intersection; unsupported folded pose, physics unchanged.");
            }
        }
        public static double Cross(Point2 a,Point2 b,Point2 c) => (b.X-a.X)*(c.Y-a.Y)-(b.Y-a.Y)*(c.X-a.X);
        public static double Distance(Point2 a,Point2 b)
        { double x=a.X-b.X,y=a.Y-b.Y;return Math.Sqrt(x*x+y*y); }
        private static void Finite(Point2 p)
        { Checks.Range(p.X,-100000,100000,nameof(p));Checks.Range(p.Y,-100000,100000,nameof(p)); }
        private static bool Intersects(Point2 a,Point2 b,Point2 c,Point2 d)
        {
            double abC=Cross(a,b,c),abD=Cross(a,b,d),cdA=Cross(c,d,a),cdB=Cross(c,d,b);
            if(((abC>0 && abD<0)||(abC<0 && abD>0)) && ((cdA>0 && cdB<0)||(cdA<0 && cdB>0))) return true;
            return (Math.Abs(abC)<1e-12 && OnSegment(c,a,b)) || (Math.Abs(abD)<1e-12 && OnSegment(d,a,b))
                || (Math.Abs(cdA)<1e-12 && OnSegment(a,c,d)) || (Math.Abs(cdB)<1e-12 && OnSegment(b,c,d));
        }
        private static bool OnSegment(Point2 p,Point2 a,Point2 b) =>
            p.X>=Math.Min(a.X,b.X)-1e-12 && p.X<=Math.Max(a.X,b.X)+1e-12
            && p.Y>=Math.Min(a.Y,b.Y)-1e-12 && p.Y<=Math.Max(a.Y,b.Y)+1e-12;

        /// <summary>Distance to the filled polygon: zero inside, not distance to hidden internal boundaries.</summary>
        public static double DistanceToSolid(Point2 p,Point2[] polygon)
        {
            bool inside=false;double minimum=double.MaxValue;
            for(int i=0,j=polygon.Length-1;i<polygon.Length;j=i++)
            {
                Point2 a=polygon[j],b=polygon[i];
                if((a.Y>p.Y)!=(b.Y>p.Y) && p.X<(b.X-a.X)*(p.Y-a.Y)/(b.Y-a.Y)+a.X) inside=!inside;
                double dx=b.X-a.X,dy=b.Y-a.Y,n=dx*dx+dy*dy;
                double t=n==0 ? 0 : Math.Max(0,Math.Min(1,((p.X-a.X)*dx+(p.Y-a.Y)*dy)/n));
                minimum=Math.Min(minimum,Distance(p,new Point2(a.X+t*dx,a.Y+t*dy)));
            }
            return inside ? 0 : minimum;
        }
        /// <summary>Sampled bidirectional solid-coverage discrepancy, NOT exact Hausdorff or native contact validation.</summary>
        public CoverageMeasure MeasureCoverage(Point2[][] colliderPaths,int samplesPerEdge=8)
        {
            if(colliderPaths==null || colliderPaths.Length==0 || samplesPerEdge<1) throw new ArgumentException("Collider paths and positive sampling required.");
            foreach(var path in colliderPaths)
            {
                if(path==null || path.Length<3) throw new ArgumentException("Closed collider path required.");
                foreach(var point in path) Finite(point);
            }
            double outside=0,uncovered=0;int samples=0;
            for(int i=0;i<Perimeter.Length;i++)
            for(int s=0;s<samplesPerEdge;s++)
            {
                var p=Lerp(Perimeter[i],Perimeter[(i+1)%Perimeter.Length],s/(double)samplesPerEdge);
                double distance=double.MaxValue;
                foreach(var path in colliderPaths) distance=Math.Min(distance,DistanceToSolid(p,path));
                outside=Math.Max(outside,distance);samples++;
            }
            foreach(var path in colliderPaths)
            for(int i=0;i<path.Length;i++)
            for(int s=0;s<samplesPerEdge;s++)
            {
                uncovered=Math.Max(uncovered,DistanceToSolid(Lerp(path[i],path[(i+1)%path.Length],s/(double)samplesPerEdge),Perimeter));samples++;
            }
            return new CoverageMeasure(outside,uncovered,samples);
        }
        private static Point2 Lerp(Point2 a,Point2 b,double t) => new Point2(a.X+(b.X-a.X)*t,a.Y+(b.Y-a.Y)*t);
    }
    public readonly struct CoverageMeasure
    {
        public readonly double SkinOutsidePhysics,PhysicsOutsideSkin;
        public readonly int Samples;
        public CoverageMeasure(double outside,double uncovered,int samples)
        { SkinOutsidePhysics=outside;PhysicsOutsideSkin=uncovered;Samples=samples; }
    }
}
