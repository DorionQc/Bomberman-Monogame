

namespace TopDownGridBasedEngine
{
    public class MultiFireEventArgs : CancellableEventArgs
    {
        public Fire[] Fire;
        public MultiFireEventArgs(Fire[] fire, bool cancelled)
            : base(cancelled)
        {
            Fire = fire;
        }
    }
}
