using System;
using Evolution.Source.Core;

namespace Evolution.Articulated.Core
{
    // Pure ownership policy; Unity adapter alone performs Time API writes.
    public sealed class TimeScaleLease
    {
        public double Previous { get; private set; }
        public double LastWritten { get; private set; }
        public bool Active { get; private set; }
        public static bool Supported(double scale) => scale==0 || scale==1 || scale==2 || scale==5 || scale==20;
        public void Acquire(double previous)
        {
            if(Active) throw new InvalidOperationException("Lease already active.");
            if(!Morphology.Finite(previous) || previous<0) throw new ArgumentOutOfRangeException(nameof(previous));
            Previous=previous;LastWritten=1;Active=true;
        }
        public bool Owns(double observed) => Active && observed==LastWritten;
        public bool TrySelect(double observed,double requested)
        {
            if(!Supported(requested)) throw new ArgumentOutOfRangeException(nameof(requested));
            if(!Owns(observed)) return false;
            LastWritten=requested;return true;
        }
        public bool TryRelease(double observed,out double restore)
        {
            bool owned=Owns(observed);restore=Previous;Active=false;return owned;
        }
    }
}
