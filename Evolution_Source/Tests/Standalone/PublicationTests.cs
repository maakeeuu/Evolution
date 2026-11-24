using System;
using Evolution.Source.Core;

internal static class PublicationTests
{
    public static void Run(Action<bool,string> check)
    {
        var shape=Morphology.Default;
        var first=new Point2[128];
        var second=new Point2[128];
        RadialGeometry.FillFloatCompatible(shape,first);
        int[] indices=RadialGeometry.CreateFan(128);
        for(int repeat=0;repeat<128;repeat++)
        {
            RadialGeometry.FillFloatCompatible(shape,second);
            for(int i=0;i<128;i++)
            {
                check(BitConverter.SingleToInt32Bits((float)first[i].X)==BitConverter.SingleToInt32Bits((float)second[i].X)
                    && BitConverter.SingleToInt32Bits((float)first[i].Y)==BitConverter.SingleToInt32Bits((float)second[i].Y),
                    "Unchanged morphology changed float publication bytes");
                check(indices[3*i]==0 && indices[3*i+1]==i+1 && indices[3*i+2]==(i+1)%128+1,
                    "Geometry filling mutated stable topology");
            }
        }
        var target=new Morphology(2.5,.15,-.16,.16,-.14);
        var current=shape;
        var velocity=new Point2(0,0);
        bool valid=true;
        for(int i=0;i<64;i++) Step(ref current,ref velocity,target,second,ref valid);
        long before=GC.GetAllocatedBytesForCurrentThread();
        for(int i=0;i<512;i++) Step(ref current,ref velocity,target,second,ref valid);
        long allocated=GC.GetAllocatedBytesForCurrentThread()-before;
        check(valid,"Allocation probe produced invalid geometry");
        check(allocated==0,"Core hot path allocated "+allocated+" managed bytes");
        var unchanged=Morphology.Approach(shape,target,0,double.MaxValue);
        check(unchanged.Width==shape.Width && unchanged.Height==shape.Height,"Zero response changed shape");
        var converged=Morphology.Approach(shape,target,double.MaxValue,double.MaxValue);
        check(converged.Width==target.Width && converged.Height==target.Height,"Large finite response/time failed to converge");
        bool nullRejected=false,shortRejected=false;
        try { RadialGeometry.FillFloatCompatible(shape,null); } catch(ArgumentNullException) { nullRejected=true; }
        try { RadialGeometry.FillFloatCompatible(shape,new Point2[23]); } catch(ArgumentOutOfRangeException) { shortRejected=true; }
        check(nullRejected && shortRejected,"Invalid float geometry buffer accepted");
    }

    private static void Step(ref Morphology current,ref Point2 velocity,Morphology target,Point2[] buffer,ref bool valid)
    {
        current=Morphology.Approach(current,target,.8,.02);
        velocity=PlanarMotion.ApproachVelocity(velocity,new Point2(1,.25),2,8,.02);
        RadialGeometry.FillFloatCompatible(current,buffer);
        valid &= ContourValidator.IsValidFan(buffer) && Morphology.Finite(velocity.X) && Morphology.Finite(velocity.Y);
    }
}
