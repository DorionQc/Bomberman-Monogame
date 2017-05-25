
namespace TopDownGridBasedEngine
{
    public class KickedBombEventArgs : CancellableEventArgs
    {
        public Bomb Bomb;
        public CollisionSide Side;
        public KickedBombEventArgs(Bomb b, CollisionSide side, bool cancelled) : base(cancelled)
        {
            Bomb = b;
            Side = side;
        }
    }
}
