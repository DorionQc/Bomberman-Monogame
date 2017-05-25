

namespace TopDownGridBasedEngine
{
    public class CancellableEventArgs
    {
        public bool Cancelled;

        public CancellableEventArgs(bool cancelled)
        {
            this.Cancelled = cancelled;
        }
    }
}
