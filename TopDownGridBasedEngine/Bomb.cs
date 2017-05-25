using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Penumbra;

namespace TopDownGridBasedEngine
{
    public class Bomb : AbsMoveableEntity, ITexturable
    {
        private int _textureVariant;
        private readonly int _power;

        public event OnBombExplodeHandler Explode;
        public event OnGenericMultiblockEventHandler BreakBlocks;
        public event OnGenericBlockEventHandler PlaceBonus;

        public Bomb(int x, int y, Map m, Joueur owner, int lifeTime, int power, bool registered, int ID = 0) : base(x, y, m, registered, ID)
        {
            Owner = owner;
            LifeTime = lifeTime;
            _power = power;
            Size = 10;
            _textureVariant = 0;
            Z = 0;
            Carried = false;
            Flying = false;

            Collided += Bomb_Collided;
            ChangedCase += Bomb_ChangedCase;
            Moved += Bomb_Moved;
            
            Light = new PointLight();
            Light.Color = Color.OrangeRed;
            Light.Intensity = 0.3f;
            Light.Scale = new Vector2(100, 100);
            Light.ShadowType = ShadowType.Solid;
            Game1.Penumbra.Lights.Add(Light);

        }

        public Light Light { get; }

        public void FireBreakBlocks(object sender, MultiCaseEventArgs e)
        {
            BreakBlocks?.Invoke(sender, e);
        }

        public void FirePlacedBonus(object sender, CaseEventArgs e)
        {
            PlaceBonus?.Invoke(sender, e);
        }

        public void FireExplode(object sender, CaseEventArgs e)
        {
            Explode?.Invoke(sender, e);
        }

        public void Bomb_Moved(object sender, CancellableEventArgs e)
        {
            Light.Position = new Vector2(X * Map.Width / Map.EntityPixelPerCase, Y * Map.Width / Map.EntityPixelPerCase);
        }

        public void Bomb_ChangedCase(object sender, MultiCaseEventArgs e)
        {
            CaseVide cv = e.Source as CaseVide;
            if (cv != null)
            {
                cv.Bomb = null;
            }
            if (e.Cases.First() != null)
            {
                cv = e.Cases.First() as CaseVide;
                if (cv != null)
                    cv.Bomb = this;
            }
        }

        /// <summary>
        /// La bombe se fait botter
        /// </summary>
        /// <param name="side"></param>
        /// <returns></returns>
        public bool Kick(CollisionSide side)
        {
            bool ret = false;
            int mx = X / 30, my = Y / 30;
            
            switch (side)
            {
                case CollisionSide.Left:
                    if (!Map[mx + 1, my].IsSolid)
                    {
                        ret = true;
                        VelX += 1;
                    }
                    break;
                case CollisionSide.Right:
                    if (!Map[mx - 1, my].IsSolid)
                    {
                        ret = true;
                        VelX -= 1;
                    }
                    break;
                case CollisionSide.Up:
                    if (!Map[mx, my + 1].IsSolid)
                    {
                        ret = true;
                        VelY += 1;
                    }
                    break;
                case CollisionSide.Down:
                    if (!Map[mx, my - 1].IsSolid)
                    {
                        ret = true;
                        VelY -= 1;
                    }
                    break;
            }
            
            CaseVide c = Map[mx, my] as CaseVide;
            
            if (ret && c != null)
            {
                c.Bomb = null;
            }
            
            return ret;
        }

        /// <summary>
        /// La bombe entre en collision avec quelque chose
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Bomb_Collided(object sender, BlockCollisionEventArgs e)
        {
            int mx, my;
            if (Flying)
            {
                mx = e.Info[0].Case.X;
                my = e.Info[0].Case.Y;
            }
            else
            {
                mx = X / Map.EntityPixelPerCase;
                my = Y / Map.EntityPixelPerCase;
            }
            if (!(e.Info[0].Case is CaseVide))
                Map[mx, my] = new CaseVide(mx, my, Map);
            
            SettleAt(mx, my);
        }
        
        

        /// <summary>
        /// La bombe se place quelque part
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void SettleAt(int x, int y)
        {
            CaseVide cv = Map[x, y] as CaseVide;
            if (cv == null)
                Map[x, y] = cv = new CaseVide(x, y, Map);
            
            if (cv.IsBreaking)
                LifeTime = 150;
            
            ((CaseVide)Map[x, y]).Bomb = this;
            X = x * Map.EntityPixelPerCase + Size;
            Y = y * Map.EntityPixelPerCase + Size;
            VelX = 0;
            VelY = 0;
            Z = 0;
            Owner.FireDroppedBomb(Owner, new CaseEventArgs(Map[x, y], false));
            Flying = false;
            Carried = false;
        }

        public bool Carried { get; set; }

        public bool Flying { get; set; }

        public int Z { get; set; }

        public Joueur Owner { get; set; }

        public int LifeTime { get; set; }

        public override EntityType Type => EntityType.Bomb;

        /// <summary>
        /// Méthode de dessin
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="width"></param>
        public override void Draw(SpriteBatch sb, float width)
        {
            Draw(sb, width, Color.White);

        }

        public override void Draw(SpriteBatch sb, float width, Color color)
        {
            int rad = Size + 5;
            Texture2D texture = TextureManager.Instance.TextureBomb[(_textureVariant / 20) % 4];
            //g.DrawImage(bit, _x * Width, _y * Width, Width, Width);
            if (/*VelX == 0 && VelY == 0 && */!Carried)
                sb.Draw(texture, new Rectangle((int)((X - X % 30) * width), (int)((Y - Y % 30) * width), (int)(rad * 2 * width), (int)(rad * 2 * width)), color);
            else
                sb.Draw(texture, new Rectangle((int)((X - 15) * width), (int)((Y - 15 - Z) * width), (int)(rad * 2 * width), (int)(rad * 2 * width)), color);
            /*if (_velx == 0 && _vely == 0 && !_carried)
                g.DrawImage(bit, new Rectangle((int)((_x - _x % 30) * Width), (int)((_y - _y % 30) * Width), (int)(rad * 2 * Width), (int)(rad * 2 * Width)),
                    0, 0, bit.Width, bit.Height, GraphicsUnit.Pixel, TextureManager.PlayerColorAttribute[0]);
            else
                g.DrawImage(bit, new Rectangle((int)((_x - 15) * Width), (int)((_y - 15 - _z) * Width), (int)(rad * 2 * Width), (int)(rad * 2 * Width)),
                    0, 0, bit.Width, bit.Height, GraphicsUnit.Pixel, TextureManager.PlayerColorAttribute[0]);*/
        }

        /// <summary>
        /// Appelée par l'EntityManager. Fait ce qui est nécessaire
        /// </summary>
        /// <param name="deltaTime"></param>
        public override void Tick(long deltaTime)
        {
            if (!Carried && !Flying)
            {
                base.Tick(deltaTime);
            }
            else if (Flying)
            {
                TickFlying(deltaTime);
            }
            if (Z == 0 && Flying == false && IsRegistered)
            {
                // Décrémente sa durée de vie
                LifeTime -= (int)deltaTime;
                if (LifeTime < 0)
                    Update();
            }
        }

        /// <summary>
        /// Change la texture de la bombe
        /// </summary>
        /// <param name="deltaTime"></param>
        public void UpdateTexture(long deltaTime)
        {
            _textureVariant += (int)deltaTime / 5;
            if (_textureVariant > 79)
                _textureVariant %= 80;
        }

        /// <summary>
        /// Explosion! BOOM
        /// </summary>
        public void Update()
        {
            
            int i = 1, x, y, mx = X / Map.EntityPixelPerCase, my = Y / Map.EntityPixelPerCase;
            CaseEventArgs e = new CaseEventArgs(Map[mx, my], false);
            FireExplode(this, e);
            Game1.Penumbra.Lights.Remove(Light);
            if (e.Cancelled)
                return;
            
            Owner.BombsLeft++;
            if (IsRegistered)
            {
                bool bLeft = true, bRight = true, bUp = true, bDown = true;

                List<AbsCase> BrokenBlocks = new List<AbsCase>();
                while (i <= _power && (bRight || bUp || bLeft || bDown))
                {
                    if (bRight)
                    {
                        x = mx + i;
                        if (x < Map.NoCase && Map[x, my].IsBreakable)
                        {
                            if (!SetFire(x, my, BrokenBlocks, IsRegistered))
                                bRight = false;
                        }
                        else
                            bRight = false;
                    }
                    if (bLeft)
                    {
                        x = mx - i;
                        if (x >= 0 && Map[x, my].IsBreakable)
                        {
                            if (!SetFire(x, my, BrokenBlocks, IsRegistered))
                                bLeft = false;
                        }
                        else
                            bLeft = false;
                    }
                    if (bUp)
                    {
                        y = my + i;
                        if (y < Map.NoCase && Map[mx, y].IsBreakable)
                        {
                            if (!SetFire(mx, y, BrokenBlocks, IsRegistered))
                                bUp = false;
                        }
                        else
                            bUp = false;
                    }
                    if (bDown)
                    {
                        y = my - i;
                        if (y >= 0 && Map[mx, y].IsBreakable)
                        {
                            if (!SetFire(mx, y, BrokenBlocks, IsRegistered))
                                bDown = false;
                        }
                        else
                            bDown = false;
                    }
                    i++;
                }

                SetFire(mx, my, BrokenBlocks, IsRegistered);
                FireBreakBlocks(this, new MultiCaseEventArgs(Map[mx, my], BrokenBlocks.ToArray(), false));
            }
            ((CaseVide)Map[mx, my]).Bomb = null;
            EntityManager.Instance.Remove(this);


        }

        /// <summary>
        /// Changes a block to fire
        /// </summary>
        /// <param name="x">X coord (in map coords) of the block</param>
        /// <param name="y">Y coord (in map coords) of the block</param>
        /// <returns>True if the block was changed and you can keep updating other blocks.
        /// False if you must stop updating blocks.</returns>
        private bool SetFire(int x, int y, List<AbsCase> BrokenBlocks, bool Registered)
        {
            if (x < 0 || y < 0 || x >= Map.NoCase || y >= Map.NoCase)
                return false;
            bool ret = false;
            AbsCase c = Map[x, y];
            if (!c.IsBreakable)
                return false;

            CaseVide cv = c as CaseVide;
            
            if (cv != null)
            {
                if (cv.ContainsBomb)
                    cv.Bomb.LifeTime = 150;
            }

            if (c is CaseWall)
            {
                if (!Map.MakeRandomBonus(x, y))
                    Ignite(x, y, Map, 600, BrokenBlocks, Registered);
                else
                    FirePlacedBonus(this, new CaseEventArgs(Map[x, y], false));
            }
            else
            {
                Ignite(x, y, Map, 600, BrokenBlocks, Registered);
                if (c.LetsFireThrough)
                {
                    ret = true;
                }
            }

            return ret;
        }

        /// <summary>
        /// Brise une case
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="m"></param>
        /// <param name="LifeTime"></param>
        /// <param name="BrokenBlocks"></param>
        /// <param name="Register"></param>
        /// <param name="EntityID"></param>
        public static void Ignite(int x, int y, Map m, int LifeTime, List<AbsCase> BrokenBlocks, bool Register, int EntityID = 0)
        {
            Fire f = new Fire(x * Map.EntityPixelPerCase + Map.EntityPixelPerCase / 2, y * Map.EntityPixelPerCase + Map.EntityPixelPerCase / 2, m, 600, Register, EntityID);
            m[x, y].Fire = f;
            BrokenBlocks?.Add(m[x, y]);
            EntityManager.Instance.Add(f);
        }








        // Collisions dans les airs
        #region Flying Collisions

        private void TickFlying(long DeltaTime)
        {
            float dt = DeltaTime / 2;
            int vx = (int)(VelX * dt);
            int vy = (int)(VelY * dt);
            List<CollisionInfo> CollisionInfoReturned = CheckFlyingCollision(X, Y, vx, vy);
            CollisionSide Side = 0;
            CollisionInfo Target = new CollisionInfo(CollisionSide.None, null);
            foreach (CollisionInfo c in CollisionInfoReturned)
            {
                Side |= c.Side;
                if (c.Case != null)
                {
                    if (c.Case.IsBreaking)
                    {
                        FireDied(this, new CancellableEventArgs(false));
                    }
                    Target = c;
                }
                else if (c.Case == null && c.Side != 0)
                {
                    switch (Side)
                    {
                        case CollisionSide.Up:
                            Y = Map.NoCase * Map.EntityPixelPerCase - Size;
                            break;
                        case CollisionSide.Down:
                            Y = Size;
                            break;
                        case CollisionSide.Left:
                            X = Map.NoCase * Map.EntityPixelPerCase - Size;
                            break;
                        case CollisionSide.Right:
                            X = Size;
                            break;
                    }
                }
            }

            if (Side == CollisionSide.None)
            {
                FireMoved(this, new CancellableEventArgs(false));
            }
            else
            {
                if (Target.Case != null && !Target.Case.IsSolid)
                {
                    BlockCollisionEventArgs e = new BlockCollisionEventArgs(Target.Case.X, Target.Case.Y, new List<CollisionInfo> { new CollisionInfo(Target.Side, Target.Case)}, false);
                    FireCollided(this, e);
                }
                if ((Side & (CollisionSide.Left | CollisionSide.Right)) > 0)
                {
                    //_velx = 0;
                    vx = 0;
                }
                if ((Side & (CollisionSide.Up | CollisionSide.Down)) > 0)
                {
                    //_vely = 0;
                    vy = 0;
                }
                if (vx != 0 || vy != 0)
                    FireMoved(this, new CancellableEventArgs(false));

            }

            // Détection de mouvement
            if (X / Map.EntityPixelPerCase != (X + vx) / Map.EntityPixelPerCase) // Side only
            {
                FireChangedCase(this, new MultiCaseEventArgs(Map[X / Map.EntityPixelPerCase, Y / Map.EntityPixelPerCase], new AbsCase[] {
                    Map[(X + vx) / Map.EntityPixelPerCase, (Y + vy) / Map.EntityPixelPerCase]
                }, false));
                if (Y / Map.EntityPixelPerCase != (Y + vy) / Map.EntityPixelPerCase) // Both side and up/down
                {
                    FireChangedCase(this, new MultiCaseEventArgs(Map[X / Map.EntityPixelPerCase, Y / Map.EntityPixelPerCase], new AbsCase[] {
                        Map[(X + vx) / Map.EntityPixelPerCase, (Y + vy) / Map.EntityPixelPerCase],
                        Map[(X + vx) / Map.EntityPixelPerCase, Y / Map.EntityPixelPerCase], Map[X / Map.EntityPixelPerCase, (Y + vy) / Map.EntityPixelPerCase]
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

            if (Math.Abs(VelX) < 0.01f)
                VelX = 0;
            if (Math.Abs(VelY) < 0.01f)
                VelY = 0;
        }

        private List<CollisionInfo> CheckFlyingCollision(int x, int y, float velx, float vely)
        {

            List<CollisionInfo> ret = new List<CollisionInfo>(2);
            int i = 0, n = 0;
            int xm = x + (int)velx - Size; // top left
            int ym = y + (int)vely - Size;
            int rad2 = Size + Size;
            int oldx = x - Size;
            int oldy = y - Size;
            AbsCase[] cases = GetCasesIn(x + (int)velx, y + (int)vely);


            if (xm < 0) // TODO - Faire que la bombe loop autour de la map
                ret.Add(new CollisionInfo(CollisionSide.Left, null));
            else if (xm + rad2 >= Map.NoCase * Map.EntityPixelPerCase)
                ret.Add(new CollisionInfo(CollisionSide.Right, null));
            if (ym < 0)
                ret.Add(new CollisionInfo(CollisionSide.Up, null));
            else if (ym + rad2 >= Map.NoCase * Map.EntityPixelPerCase)
                ret.Add(new CollisionInfo(CollisionSide.Down, null));

            for (; i < cases.Length; i++)
            {
                if (!cases[i].IsSolid) // Vérifier les cases pas solides
                    n++;
                //if (Cases[i] is CaseVide && Cases[i].IsBreaking) // Exploser la bombe
                //    Update();
            }
            if (n == 0)
            {
                if (ret.Count > 0)
                    i = 0;
                return ret;
            }
            cases = cases.Where((ca) => ca.IsSolid == false).OrderBy((a) => Math.Abs(xm - a.X * Map.EntityPixelPerCase) + Math.Abs(ym - a.Y * Map.EntityPixelPerCase)).ToArray();
            // Ignorer tout cela et trouver la case dans laquelle on tombe

            float dxm, dym, dx, dy;
            for (i = 0; i < n; i++)
            {
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
            // Retourner la case dans laquelle on tombe.
            return ret;
        }

        #endregion
    }
}
