using System;
using System.Collections.Generic;
using Evolution.Articulated.Core;
using Evolution.Source.Core;

internal static class RodSkinTests
{
    private static int checks,poses;
    private static double maxOutside,maxUncovered,maxMiter;
    private static void Check(bool value,string message)
    { checks++;if(!value) throw new Exception("EV-CP3: "+message); }
    private static void Near(double a,double b,double e,string label) => Check(Math.Abs(a-b)<=e,label);
    private static void Reject(Action action)
    { bool rejected=false;try{action();}catch(ArgumentException){rejected=true;}Check(rejected,"invalid pose accepted"); }
    private static void Pose(ChainDefinition chain,double[] degrees,out Point2[] starts,out Point2[] ends)
    {
        starts=new Point2[chain.Count];ends=new Point2[chain.Count];double angle=0,x=0,y=0;
        for(int i=0;i<chain.Count;i++)
        {
            if(i>0) angle+=degrees[i-1]*Math.PI/180;
            starts[i]=new Point2(x,y);x+=chain[i].Length*Math.Cos(angle);y+=chain[i].Length*Math.Sin(angle);
            ends[i]=new Point2(x,y);
        }
    }
    private static Point2[][] PhysicalPaths(ChainDefinition chain,Point2[] starts,Point2[] ends)
    {
        var paths=new Point2[chain.Count][];
        for(int i=0;i<chain.Count;i++)
        {
            var points=SegmentContour.Create(chain[i].Length,chain[i].Radius);
            double dx=(ends[i].X-starts[i].X)/chain[i].Length,dy=(ends[i].Y-starts[i].Y)/chain[i].Length;
            paths[i]=new Point2[points.Length];
            for(int p=0;p<points.Length;p++)
            {
                double x=points[p].X+chain[i].Length*.5,y=points[p].Y;
                paths[i][p]=new Point2(starts[i].X+x*dx-y*dy,starts[i].Y+x*dy+y*dx);
            }
        }
        return paths;
    }
    private static void Surface(ChainDefinition chain,double[] angles,bool measure)
    {
        Pose(chain,angles,out var starts,out var ends);
        var startsCopy=(Point2[])starts.Clone();var endsCopy=(Point2[])ends.Clone();
        var skin=new RodSkinGeometry(chain);skin.Update(starts,ends);poses++;
        var vertices=(Point2[])skin.Vertices.Clone();var triangles=(int[])skin.Triangles.Clone();
        skin.Update(starts,ends);
        Check(skin.SectionCount==chain.Count+11,"fixed longitudinal topology");
        for(int i=0;i<vertices.Length;i++)
        {
            Near(vertices[i].X,skin.Vertices[i].X,0,"deterministic vertex x");
            Near(vertices[i].Y,skin.Vertices[i].Y,0,"deterministic vertex y");
            Check(vertices[i].X==(float)vertices[i].X && vertices[i].Y==(float)vertices[i].Y,"float publication");
        }
        for(int i=0;i<triangles.Length;i++) Check(triangles[i]==skin.Triangles[i],"deterministic index");
        var edges=new Dictionary<string,int>();
        for(int i=0;i<triangles.Length;i+=3)
        {
            Point2 a=vertices[triangles[i]],b=vertices[triangles[i+1]],c=vertices[triangles[i+2]];
            Check((b.X-a.X)*(c.Y-a.Y)-(b.Y-a.Y)*(c.X-a.X)>1e-12,"positive CCW triangle");
            for(int k=0;k<3;k++)
            {
                int u=triangles[i+k],v=triangles[i+(k+1)%3];
                string edge=Math.Min(u,v)+":"+Math.Max(u,v);
                edges.TryGetValue(edge,out int count);edges[edge]=count+1;
            }
        }
        int boundary=0;
        foreach(var count in edges.Values) { Check(count==1 || count==2,"manifold edge");if(count==1)boundary++; }
        Check(boundary==vertices.Length,"one shared connected strip boundary, no seam cracks");
        Check(vertices.Length-edges.Count+triangles.Length/3==1,"disk topology");
        double area=0;
        for(int i=0;i<skin.Perimeter.Length;i++)
        {
            var a=skin.Perimeter[i];var b=skin.Perimeter[(i+1)%skin.Perimeter.Length];area+=a.X*b.Y-a.Y*b.X;
            for(int j=i+2;j<skin.Perimeter.Length;j++)
            {
                if(i==0 && j==skin.Perimeter.Length-1)continue;
                var c=skin.Perimeter[j];var d=skin.Perimeter[(j+1)%skin.Perimeter.Length];
                double p=RodSkinGeometry.Cross(a,b,c),q=RodSkinGeometry.Cross(a,b,d);
                double u=RodSkinGeometry.Cross(c,d,a),v=RodSkinGeometry.Cross(c,d,b);
                Check(!(p*q<0 && u*v<0),"independent proper intersection check");
            }
        }
        Check(area>0,"CCW perimeter");
        for(int i=0;i<chain.Count;i++)
        {
            Near(starts[i].X,startsCopy[i].X,0,"starts not mutated");Near(ends[i].Y,endsCopy[i].Y,0,"ends not mutated");
        }
        for(int i=0;i<=chain.Count;i++)
        {
            int section=i+RodSkinGeometry.CapSections;
            double width=RodSkinGeometry.Distance(vertices[section*2],vertices[section*2+1]);
            double radius=i==0 ? chain[0].Radius : i==chain.Count ? chain[i-1].Radius : Math.Max(chain[i-1].Radius,chain[i].Radius);
            double expectedRatio=i==0 || i==chain.Count ? 1 : 1/Math.Cos(angles[i-1]*Math.PI/360);
            Near(width,2*radius*expectedRatio,2e-6,"anatomical width, not global bounding box");
            Check(width<=2*radius*RodSkinGeometry.MaxMiterRatio+2e-6,"bounded joint width");
            var center=i==chain.Count ? ends[i-1] : starts[i];
            Near((vertices[section*2].X+vertices[section*2+1].X)*.5,center.X,2e-6,"axis-following section x");
            Near((vertices[section*2].Y+vertices[section*2+1].Y)*.5,center.Y,2e-6,"axis-following section y");
        }
        maxMiter=Math.Max(maxMiter,skin.LargestMiterRatio);
        if(measure)
        {
            CoverageMeasure m=skin.MeasureCoverage(PhysicalPaths(chain,starts,ends),8);
            maxOutside=Math.Max(maxOutside,m.SkinOutsidePhysics);maxUncovered=Math.Max(maxUncovered,m.PhysicsOutsideSkin);
            Check(m.Samples==8*(skin.Perimeter.Length+chain.Count*SegmentContour.Resolution),"coverage sample count");
            Check(m.SkinOutsidePhysics<.025 && m.PhysicsOutsideSkin<.025,"default geometry discrepancy bound .025 m");
        }
    }
    public static void Run()
    {
        // Straight/C/S poses across every supported count, plus parameter extremes and deterministic angle sequences.
        for(int count=2;count<=8;count++)
        for(int shape=0;shape<7;shape++)
        {
            var angles=new double[count-1];
            for(int i=0;i<angles.Length;i++) angles[i]=shape==0 ? 0 : shape==1 ? 12 : shape==2 ? -12 : shape==3 ? (i%2==0 ? 40 : -40) : shape==4 ? (i%2==0 ? -40 : 40) : Math.Sin(i*1.7+shape)*30;
            Surface(ChainDefinition.Create(count),angles,true);
        }
        for(int count=2;count<=8;count++)
        for(int extreme=0;extreme<8;extreme++)
        {
            var angles=new double[count-1];for(int i=0;i<angles.Length;i++)angles[i]=(i%2==0 ? 1 : -1)*60;
            Surface(ChainDefinition.Create(count,(extreme&1)==0 ? .5 : 1.5,(extreme&2)==0 ? .08 : .2,(extreme&4)==0 ? .94 : 1,60),angles,false);
        }
        var chain=ChainDefinition.Create();Pose(chain,new double[3],out var starts,out var ends);
        var skin=new RodSkinGeometry(chain);skin.Update(starts,ends);
        double length=0;for(int i=0;i<chain.Count;i++)length+=chain[i].Length;
        Check(length/(2*chain[0].Radius)>11,"rest centerline slenderness");
        var old=(Point2[])skin.Vertices.Clone();var oldIndices=skin.Triangles;
        Pose(chain,new[]{40.0,40.0,40.0},out starts,out ends);skin.Update(starts,ends);
        Check(RodSkinGeometry.Distance(old[16],skin.Vertices[16])>.1,"bent pose changes actual geometry");
        Check(ReferenceEquals(oldIndices,skin.Triangles),"stable topology across pose update");
        Surface(chain,new[]{40.0,40.0,40.0},true);Surface(chain,new[]{-40.0,-40.0,-40.0},true);
        Pose(chain,new double[3],out starts,out ends);
        var bad=(Point2[])starts.Clone();bad[1]=new Point2(double.NaN,0);Reject(()=>skin.Update(bad,ends));
        bad=(Point2[])starts.Clone();var badEnds=(Point2[])ends.Clone();bad[1]=new Point2(bad[1].X+.03,bad[1].Y);badEnds[1]=new Point2(badEnds[1].X+.03,badEnds[1].Y);Reject(()=>skin.Update(bad,badEnds));
        Reject(()=>skin.Update(new Point2[0],ends));
        Pose(chain,new[]{65.0,0,0},out starts,out ends);Reject(()=>skin.Update(starts,ends));
        var loop=ChainDefinition.Create(8,.85,.14,1,60);Pose(loop,new[]{60.0,60,60,60,60,60,60},out starts,out ends);
        Reject(()=>new RodSkinGeometry(loop).Update(starts,ends));
        Console.WriteLine("EV-CP3: checks="+checks+"; poses="+poses+"; max sampled skin outside="+maxOutside.ToString("F9")+" m; physics outside="+maxUncovered.ToString("F9")+" m; max miter="+maxMiter.ToString("F9")+" (no Unity).");
    }
}
