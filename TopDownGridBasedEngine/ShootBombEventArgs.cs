

namespace TopDownGridBasedEngine
{
    public class ShootBombEventArgs : CancellableEventArgs
    {
        public Joueur Joueur;
        public Bomb Bomb;
        public CollisionSide Side;

        public ShootBombEventArgs(Joueur j, Bomb b, CollisionSide s, bool cancelled) : base(cancelled)
        {
            Joueur = j;
            Bomb = b;
            Side = s;
        }
    }
}
