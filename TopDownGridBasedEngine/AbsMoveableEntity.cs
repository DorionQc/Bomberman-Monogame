using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace TopDownGridBasedEngine
{

    [Flags]
    public enum CollisionSide
    {
        None = 0,
        Up = 1,
        Left = 2,
        Down = 4,
        Right = 8
    };

    public struct CollisionInfo
    {
        public CollisionSide Side;
        public AbsCase Case;

        public CollisionInfo(CollisionSide side, AbsCase Case)
        {
            this.Side = side;
            this.Case = Case;
        }
    }

    public abstract class AbsMoveableEntity : AbsEntity
    {
        public event OnChangeCaseHandler ChangedCase;
        public event OnMoveHandler Moved;
        public event OnCollideWithBlockHandler Collided;

        protected AbsMoveableEntity(int x, int y, Map m, bool registered) : this(x, y, m, registered, 0)
        {
            
        }

        protected AbsMoveableEntity(int x, int y, Map m, bool registered, int ID) : base(x, y, m, registered, ID)
        {
            VelX = 0;
            VelY = 0;
        }

        public void FireMoved(object sender, CancellableEventArgs e)
        {
            Moved?.Invoke(sender, e);
        }

        public void FireChangedCase(object sender, MultiCaseEventArgs e)
        {
            ChangedCase?.Invoke(sender, e);
        }
        public void FireCollided(object sender, BlockCollisionEventArgs e)
        {
            Collided?.Invoke(sender, e);
        }


        public float VelX { get; set; }

        public float VelY { get; set; }

        /// <summary>
        /// Vérifie les collisions avec les cases solides
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="velx"></param>
        /// <param name="vely"></param>
        /// <returns></returns>
        protected virtual List<CollisionInfo> CheckCollision(int x, int y, float velx, float vely)
        {

            List<CollisionInfo> ret = new List<CollisionInfo>(2);
            int i = 0, n = 0;
            
            // Top Left
            int xm = x + (int)velx - Size;
            int ym = y + (int)vely - Size;
            
            int rad2 = Size + Size;
            int oldx = x - Size;
            int oldy = y - Size;
            
            AbsCase[] cases = GetCasesIn(x + (int)velx, y + (int)vely);


            if (xm < 0)
                ret.Add(new CollisionInfo(CollisionSide.Left, null));
            else if (xm + rad2 >= Map.NoCase * Map.EntityPixelPerCase)
                ret.Add(new CollisionInfo(CollisionSide.Right, null));
            if (ym < 0)
                ret.Add(new CollisionInfo(CollisionSide.Up, null));
            else if (ym + rad2 >= Map.NoCase * Map.EntityPixelPerCase)
                ret.Add(new CollisionInfo(CollisionSide.Down, null));

            for (; i < cases.Length; i++)
            {
                if (cases[i].IsSolid)
                    n++;
                if (cases[i].IsBreaking)
                    FireDied(this, new CancellableEventArgs(false));
            }
            
            if (n == 0)
            {
                if (ret.Count > 0)
                    i = 0;
                return ret;
            }
            
            cases = cases.Where((ca) => ca.IsSolid == true).OrderBy((a) => Math.Abs(xm - a.X * Map.EntityPixelPerCase) + Math.Abs(ym - a.Y * Map.EntityPixelPerCase)).ToArray();

            
            for (i = 0; i < n; i++)
            {
                float dxm, dym, dx, dy;
                dxm = xm - cases[i].X * Map.EntityPixelPerCase;
                dym = ym - cases[i].Y * Map.EntityPixelPerCase;
                dx = x - cases[i].X * Map.EntityPixelPerCase - Size;
                dy = y - cases[i].Y * Map.EntityPixelPerCase - Size;
                if (dx <= -rad2)
                {
                    if (-dxm <= rad2 - 1 && dym > -rad2 && dym < Map.EntityPixelPerCase)
                    {
                        ret.Add(new CollisionInfo(CollisionSide.Left, cases[i]));
                        xm = x - Size;
                    }
                }
                else if (dx >= Map.EntityPixelPerCase)
                {
                    if (dxm <= Map.EntityPixelPerCase && dym > -rad2 && dym < Map.EntityPixelPerCase)
                    {
                        ret.Add(new CollisionInfo(CollisionSide.Right, cases[i]));
                        xm = x - Size;
                    }
                }
                if (dy <= -rad2)
                {
                    if (-dym <= rad2 - 1 && dxm > -rad2 && dxm < Map.EntityPixelPerCase)
                    {
                        ret.Add(new CollisionInfo(CollisionSide.Up, cases[i]));
                        ym = y - Size;
                    }
                }
                else if (dy >= Map.EntityPixelPerCase)
                {
                    if (dym <= Map.EntityPixelPerCase && dxm > -rad2 && dxm < Map.EntityPixelPerCase)
                    {
                        ret.Add(new CollisionInfo(CollisionSide.Down, cases[i]));
                        ym = y - Size;
                    }
                }
            }

            return ret;
        }

        /// <summary>
        /// Calcule le mouvement de l'entité
        /// </summary>
        /// <param name="DeltaTime"></param>
        public override void Tick(long deltaTime)
        {
            float dt = deltaTime / 2;
            //VelY += 0.2f;
            int vx = (int)(VelX * dt);
            int vy = (int)(VelY * dt);
            
            List<CollisionInfo> res = CheckCollision(X, Y, vx, vy);
            CollisionSide rs = 0;
            
            foreach (CollisionInfo c in res)
            {
                rs |= c.Side;
                if (c.Case != null && c.Case.IsBreaking)
                    FireDied(this, new CancellableEventArgs(false));
            }
            
            bool Or = false;
            if (Math.Abs(VelX) < 0.01f)
            {
                VelX = 0;
                Or = true;
            }
            if (Math.Abs(VelY) < 0.01f)
            {
                VelY = 0;
                Or = true;
            }
            if (Or)
                FireMoved(this, new CancellableEventArgs(false));

            if (vx == 0 && vy == 0)
                return; 

            
            if (rs != CollisionSide.None)
            {
                BlockCollisionEventArgs e = new BlockCollisionEventArgs(X / Map.EntityPixelPerCase, Y / Map.EntityPixelPerCase, res, false);
                FireCollided(this, e);
                if ((rs & (CollisionSide.Left | CollisionSide.Right)) > 0)
                {
                    //_velx = 0;
                    vx = 0;
                }
                if ((rs & (CollisionSide.Up | CollisionSide.Down)) > 0)
                {
                    //_vely = 0;
                    vy = 0;
                }
            }

            
            if (X / Map.EntityPixelPerCase != (X + vx) / Map.EntityPixelPerCase) // Side only
            {
                FireChangedCase(this, new MultiCaseEventArgs(Map[X / Map.EntityPixelPerCase, Y / Map.EntityPixelPerCase], new AbsCase[] {
                    Map[(X + vx) / Map.EntityPixelPerCase, (Y + vy) / Map.EntityPixelPerCase]
                }, false));
                if (Y / Map.EntityPixelPerCase != (Y + vy) / Map.EntityPixelPerCase) // Both side and up/down
                {
                    FireChangedCase(this, new MultiCaseEventArgs(Map[X / Map.EntityPixelPerCase, Y / Map.EntityPixelPerCase], new AbsCase[] {
                        Map[(X + vx) / Map.EntityPixelPerCase, (Y + vy) / Map.EntityPixelPerCase],
                        Map[(X + vx) / Map.EntityPixelPerCase, Y / Map.EntityPixelPerCase],
                        Map[X / Map.EntityPixelPerCase, (Y + vy) / Map.EntityPixelPerCase]
                    }, false));
                }
            }
            else if (Y / Map.EntityPixelPerCase != (Y + vy) / Map.EntityPixelPerCase) // Up/Down only
            {
                FireChangedCase(this, new MultiCaseEventArgs(Map[X / Map.EntityPixelPerCase, Y / Map.EntityPixelPerCase], new AbsCase[] {
                    Map[(X + vx) / Map.EntityPixelPerCase, (Y + vy) / Map.EntityPixelPerCase]
                }, false));
            }
            X += vx;
            Y += vy;
            FireMoved(this, new CancellableEventArgs(false));
        }
    }
}
