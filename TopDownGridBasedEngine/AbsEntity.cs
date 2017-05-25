using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TopDownGridBasedEngine
{
    public delegate void OnMoveHandler(object sender, CancellableEventArgs e);
    public delegate void OnDieHandler(object sender, CancellableEventArgs e);
    public delegate void OnDropBombHandler(object sender, CaseEventArgs e);
    public delegate void OnPickBombHandler(object sender, CaseEventArgs e);
    public delegate void OnKickBombHandler(object sender, KickedBombEventArgs e);
    public delegate void OnGetBonusHandler(object sender, CaseEventArgs e);
    public delegate void OnChangeCaseHandler(object sender, MultiCaseEventArgs e);
    public delegate void OnCollideWithBlockHandler(object sender, BlockCollisionEventArgs e);
    public delegate void OnBombExplodeHandler(object sender, CaseEventArgs e);
    public delegate void OnShootBombHandler(object sender, ShootBombEventArgs e);
    public delegate void OnFireStopHandler(object sender, MultiFireEventArgs e);

    public delegate void OnGenericMultiblockEventHandler(object sender, MultiCaseEventArgs e);
    public delegate void OnGenericBlockEventHandler(object sender, CaseEventArgs e);

    public enum EntityType
    {
        Joueur = 0,
        Bomb,
        Fire
    };

    public abstract class AbsEntity
    {
        private int _x;
        private int _y;

        /// <summary>
        /// Lancé lorsque l'entité meurt
        /// </summary>
        public event OnDieHandler Died;
        
        protected void FireDied(object sender, CancellableEventArgs e)
        {
            Died?.Invoke(sender, e);
        }

        protected AbsEntity(int x, int y, Map m, bool registered, int id)
        {
            IsRegistered = registered;
            _x = x;
            _y = y;
            Map = m;
            Size = 15;
            IsDead = false;
            ID = id == 0 ? (int) DateTime.Now.Ticks ^ (x << 16) ^ y : id;
        }

        /// <summary>
        /// Indique si l'entité est morte
        /// </summary>
        public bool IsDead { get; set; }

        /// <summary>
        /// Identifiant pseudo-unique de l'entité
        /// </summary>
        public int ID { get; }

        /// <summary>
        /// Position en X de l'entité, en pixel
        /// </summary>
        public int X
        {
            get { return _x; }
            set { if (value >= 0 && value < Map.NoCase * Map.EntityPixelPerCase) _x = value; }
        }

        /// <summary>
        /// Position en Y de l'entité, en pixel
        /// </summary>
        public int Y
        {
            get { return _y; }
            set { if (value >= 0 && value < Map.NoCase * Map.EntityPixelPerCase) _y = value; }
        }

        /// <summary>
        /// Grosseur de l'entité
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        /// Carte contenant l'entité
        /// </summary>
        public Map Map { get; set; }

        /// <summary>
        /// Indique si l'entité est contenue dans l'EntityManager
        /// </summary>
        public bool IsRegistered { get; }

        /// <summary>
        /// Type de l'entité
        /// </summary>
        public abstract EntityType Type { get; }

        /// <summary>
        /// Calcule et donne un tableau contenant les cases dans lesquelles se trouve l'entité
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public AbsCase[] GetCasesIn(int x, int y)
        {
            int n = 1, i = 0;
            int x1, x2, y1, y2;
            bool bx, by;
            AbsCase[] ret;
            
            x1 = (x - Size) / Map.EntityPixelPerCase;
            x2 = (x + Size - 1) / Map.EntityPixelPerCase;
            y1 = (y - Size) / Map.EntityPixelPerCase;
            y2 = (y + Size - 1) / Map.EntityPixelPerCase;
            
            if (y2 >= Map.NoCase)
            {
                y2 = Map.NoCase - 1;
                if (y1 >= Map.NoCase)
                    y1 = Map.NoCase - 1;
            }
            
            if (x2 >= Map.NoCase)
            {
                x2 = Map.NoCase - 1;
                if (x1 >= Map.NoCase)
                    x1 = Map.NoCase - 1;
            }
            if (x1 < 0)
            {
                x1 = 0;
                if (x2 < 0)
                    x2 = 0;
            }
            if (y1 < 0)
            {
                y1 = 0;
                if (y2 < 0)
                    y2 = 0;
            }
            bx = (x1 != x2);
            by = (y1 != y2);
            if (bx) n <<= 1;
            if (by) n <<= 1;
            ret = new AbsCase[n];
            
            ret[i++] = Map[x1, y1];
            
            if (bx) ret[i++] = Map[x2, y1];
            if (by) ret[i++] = Map[x1, y2];
            if (bx && by) ret[i] = Map[x2, y2];
            return ret;
        }

        public abstract void Tick(long deltaTime);

        public abstract void Draw(SpriteBatch sb, float width);

        public abstract void Draw(SpriteBatch sb, float width, Color color);

    }
}
