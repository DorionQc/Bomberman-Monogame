
using System.Collections.Generic;

namespace TopDownGridBasedEngine
{
    public class BlockCollisionEventArgs : CancellableEventArgs
    {
        public int X;
        public int Y;
        public List<CollisionInfo> Info;

        public BlockCollisionEventArgs(int x, int y, List<CollisionInfo> colInfo, bool cancelled) : base(cancelled)
        {
            X = x;
            Y = y;
            Info = colInfo;
        }
    }
}
