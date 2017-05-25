

namespace TopDownGridBasedEngine
{
    public class MultiCaseEventArgs : CancellableEventArgs
    {
        public AbsCase Source;
        public AbsCase[] Cases;

        public MultiCaseEventArgs(AbsCase source, AbsCase[] cases, bool cancelled) : base(cancelled)
        {
            Source = source;
            Cases = cases;
        }
    }
}
