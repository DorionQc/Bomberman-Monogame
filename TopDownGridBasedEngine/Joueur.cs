using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Penumbra;


namespace TopDownGridBasedEngine
{
    public class Joueur : AbsMoveableEntity, ITexturable
    {

        private int _textureVariant;
        private int _bonusExtraBomb;
        private int _bonusFirePower;
        private int _bonusRollerSkates;
        private bool _bonusGlove;
        private bool _bonusKick;

        private bool _carryingBomb;

        private readonly byte _idJoueur;

        public event OnDropBombHandler DroppedBomb;
        public event OnGetBonusHandler PickedBonus;
        public event OnBombExplodeHandler BombExploded;
        public event OnKickBombHandler KickedBomb;
        public event OnPickBombHandler PickedBomb;
        public event OnShootBombHandler ShotBomb;
        public event OnGenericBlockEventHandler BombPlacedBonus;
        public event OnGenericMultiblockEventHandler BombBrokeBlocks;

        protected void FireBombBrokeBlocks(object sender, MultiCaseEventArgs e)
        {
            BombBrokeBlocks?.Invoke(sender, e);
        }
        protected void FireBombPlacedBonus(object sender, CaseEventArgs e)
        {
            BombPlacedBonus?.Invoke(sender, e);
        }
        protected void FireShotBomb(object sender, ShootBombEventArgs e)
        {
            ShotBomb?.Invoke(sender, e);
        }
        protected void FirePickedBomb(object sender, CaseEventArgs e)
        {
            PickedBomb?.Invoke(sender, e);
        }
        protected void FireKickedBomb(object sender, KickedBombEventArgs e)
        {
            KickedBomb?.Invoke(sender, e);
        }
        public void FireDroppedBomb(object sender, CaseEventArgs e)
        {
            DroppedBomb?.Invoke(sender, e);
        }
        protected void FirePickedBonus(object sender, CaseEventArgs e)
        {
            PickedBonus?.Invoke(sender, e);
        }

        public Joueur(int x, int y, Map m, byte id) : base(x, y, m, true)
        {
            Size = 10;
            _textureVariant = 0;

            _bonusRollerSkates = 1;
            _bonusKick = false;
            _bonusGlove = true;
            _bonusFirePower = 3;
            _bonusExtraBomb = 1;

            BombsLeft = 1;
            _carryingBomb = false;

            _idJoueur = id;
            
            Light = new Spotlight();
            Light.Color = Color.Yellow;
            Light.Scale = new Vector2(100, 300);
            Light.ShadowType = ShadowType.Solid;
            Game1.Penumbra.Lights.Add(Light);

            ChangedCase += Joueur_ChangedCase;
            Collided += Joueur_Collided;
            Moved += Joueur_Moved;
            Died += Die;
        }
        
        public Light Light { get; }

        public void Die(object sender, CancellableEventArgs e)
        {
            IsDead = true;
            Game1.Penumbra.Lights.Remove(Light);
            //MessageBox.Show("Ayyyyyyyy!! I'm dead!");
        }

        private void Joueur_Moved(object sender, CancellableEventArgs e)
        {
            if (_carryingBomb && Bomb != null)
            {
                Bomb.X = X;
                Bomb.Y = Y;
                Bomb.FireMoved(Bomb, new CancellableEventArgs(false));
            }

            Light.Position = new Vector2(X * Map.Width / Map.EntityPixelPerCase,
                Y * Map.Width / Map.EntityPixelPerCase);

            Light.Rotation = (float)Math.Atan2(-VelX, VelY) + MathHelper.PiOver2;
        }

        private void Joueur_Collided(object sender, BlockCollisionEventArgs e)
        {
            if (!_bonusKick)
                return;
            CollisionInfo inf;
            for (int i = 0; i < e.Info.Count; i++)
            {
                inf = e.Info[i];
                if (inf.Case is CaseVide && ((CaseVide)inf.Case).ContainsBomb)
                {
                    if (inf.Side != CollisionSide.None)
                    {
                        Bomb b = ((CaseVide)inf.Case).Bomb;
                        if (((CaseVide)inf.Case).Bomb.Kick(inf.Side))
                        {
                            FireKickedBomb(this, new KickedBombEventArgs(b, inf.Side, false));
                            e.Info.Remove(inf);
                        }
                    }
                }
            }
        }

        private void Joueur_ChangedCase(object sender, MultiCaseEventArgs e)
        {
            foreach (AbsCase c in e.Cases)
            {
                if (c.IsBreaking)
                {
                    FireDied(this, new CancellableEventArgs(false));
                }
                else if (c is CaseBonus)
                {
                    PickBonus((CaseBonus)c);
                }
            }
        }

        public void PickBonus(CaseBonus c)
        {
            switch (c.BonusType)
            {
                case BonusType.ExtraBomb:
                    if (_bonusExtraBomb < 9)
                    {
                        _bonusExtraBomb++;
                        BombsLeft++;
                    }
                    break;
                case BonusType.FirePower:
                    if (_bonusFirePower < 9)
                        _bonusFirePower++;
                    break;
                case BonusType.Glove:
                    _bonusGlove = true;
                    break;
                case BonusType.Kick:
                    _bonusKick = true;
                    break;
                case BonusType.MaximumPower:
                    _bonusFirePower = 9;
                    break;
                case BonusType.RollerSkates:
                    if (_bonusRollerSkates < 9)
                        _bonusRollerSkates++;
                    break;
            }
            FirePickedBonus(this, new CaseEventArgs(c, false));
            Map[c.X, c.Y] = new CaseVide(c.X, c.Y, Map);
        }

        public byte PlayerID => _idJoueur;

        public Bomb Bomb { get; set; }

        public int BombsLeft { get; set; }

        public override EntityType Type { get; } = EntityType.Joueur;

        public override void Tick(long deltaTime)
        {
            base.Tick(deltaTime);
            VelX *= (float)Math.Pow(0.9f, deltaTime / 8);
            VelY *= (float)Math.Pow(0.9f, deltaTime / 8);
            //UpdateTexture(DeltaTime);
        }

        public void UpdateTexture(long deltaTime)
        {
            _textureVariant += (int)deltaTime / 5;
            if (_textureVariant > 79)
                _textureVariant %= 80;
        }

        public void TickPlayer(long deltaTime, KeyWrapper wrapper)
        {
            if ((wrapper.State & KeyState.Up) > 0)
                VelY -= (1.1f + VelY + _bonusRollerSkates / 20.0f) / 15;
            if ((wrapper.State & KeyState.Down) > 0)
                VelY += (1.1f - VelY + _bonusRollerSkates / 20.0f) / 15;
            if ((wrapper.State & KeyState.Left) > 0)
                VelX -= (1.1f + VelX + _bonusRollerSkates / 20.0f) / 15;
            if ((wrapper.State & KeyState.Right) > 0)
                VelX += (1.1f - VelX + _bonusRollerSkates / 20.0f) / 15;
            if ((wrapper.State & KeyState.Space) > 0)
            {
                if (_carryingBomb)
                     ShootBomb(X / 30, Y / 30, VelX, VelY);
                else if (Map[X / 30, Y / 30] is CaseVide && ((CaseVide)Map[X / 30, Y / 30]).ContainsBomb)
                    PickupBomb(X / 30, Y / 30);
                else
                    DropBomb(X / 30, Y / 30, true);
            }
        }

        public bool ShootBomb(int x, int y, float Velx, float Vely)
        {
            CollisionSide Side = 0;
            if (VelX > 0)
            {
                Side = CollisionSide.Right;
            }
            else if (VelX < 0)
            {
                Side = CollisionSide.Left;
            }
            else if (VelY > 0)
            {
                Side = CollisionSide.Down;
            }
            else if (VelY < 0)
            {
                Side = CollisionSide.Up;
            }
            else
                return false;
            return ShootBomb(Side);

        }

        public bool ShootBomb(CollisionSide Side)
        {
            if (Bomb == null) return false;
            switch (Side)
            {
                case CollisionSide.Up: Bomb.VelY = -1; break;
                case CollisionSide.Down: Bomb.VelY = 1; break;
                case CollisionSide.Left: Bomb.VelX = -1; break;
                case CollisionSide.Right: Bomb.VelX = 1; break;
                default: return false;
            }
            FireShotBomb(this, new ShootBombEventArgs(this, Bomb, Side, false));
            _carryingBomb = false;
            Bomb.Carried = false;
            Bomb.Flying = true;
            Bomb.Z = 0;
            Bomb = null;
            return true;
        }

        public bool PickupBomb(int x, int y)
        {
            if (x >= Map.NoCase || y >= Map.NoCase)
                return false;
            AbsCase c = Map[x, y];
            CaseVide cv;
            if (!_bonusGlove)
                return false;
            if (!(c is CaseVide))
                return false;
            cv = (CaseVide)c;
            if (!cv.ContainsBomb)
                return false;
            if (cv.Bomb.Owner != this)
                return false;
            FirePickedBomb(this, new CaseEventArgs(Map[x, y], false));
            Bomb = cv.Bomb;
            Bomb.Z = 40;
            Map[x, y] = new CaseVide(x, y, Map);
            _carryingBomb = true;
            Bomb.Carried = true;
            
            return true;
        }

        public bool DropBomb(int x, int y, bool Register, int ID = 0)
        {
            if (x < 0 || y < 0 || x >= Map.NoCase || y >= Map.NoCase)
                return false;
            CaseVide cv;
            if (Register)
            {
                if (!(Map[x, y] is CaseVide))
                    return false;
                if (BombsLeft <= 0)
                    return false;
                cv = (CaseVide)Map[x, y];
                if (cv.ContainsBomb)
                    return false;
            }
            else
            {
                if (!(Map[x, y] is CaseVide))
                    Map[x, y] = new CaseVide(x, y, Map);
                cv = (CaseVide)Map[x, y];
            }
            cv.Bomb = new Bomb(x * 30 + 15, y * 30 + 15, Map, this, 3000, _bonusFirePower, Register, ID);
            if (cv.IsBreaking)
                cv.Bomb.LifeTime = 150;
            BombsLeft--;
            EntityManager.Instance.Add(cv.Bomb);
            FireDroppedBomb(this, new CaseEventArgs(Map[x, y], false));
            cv.Bomb.Explode += Bomb_Exploded;
            cv.Bomb.PlaceBonus += FireBombPlacedBonus;
            cv.Bomb.BreakBlocks += FireBombBrokeBlocks;
            return true;
        }

        private void Bomb_Exploded(object sender, CaseEventArgs e)
        {
            BombExploded?.Invoke(sender, e);
        }

        public override void Draw(SpriteBatch sb, float w)
        {
            Draw(sb, w, Color.White);
        }

        public override void Draw(SpriteBatch sb, float w, Color color)
        {
            //b.Color = Color.Gold;
            int rad = Size + 5;
            Texture2D bit;
            if (Math.Abs(VelX) > Math.Abs(VelY)) // Left or right
            {
                if (VelX > 0)
                    bit = TextureManager.Instance.TexturePlayerRight[_textureVariant / 20];
                else
                    bit = TextureManager.Instance.TexturePlayerLeft[_textureVariant / 20];
            }
            else // Up or down
            {
                if (VelY > 0)
                    bit = TextureManager.Instance.TexturePlayerDown[_textureVariant / 20];
                else
                    bit = TextureManager.Instance.TexturePlayerUp[_textureVariant / 20];
            }
            if (VelX == 0 && VelY == 0)
                bit = TextureManager.Instance.TexturePlayerDown[0];
            //g.FillRectangle(b, (X - m_Radius) * w, (Y - m_Radius) * w, m_Radius * 2 * w, m_Radius * 2 * w);
            //g.DrawImage(bit, (X - rad) * w, (Y - rad - 20) * w, rad * w * 2, rad * w * 3);
            sb.Draw(bit, new Rectangle((int)((X - rad) * w), (int)((Y - rad - 20) * w),(int)( rad * w * 2), (int)(rad * w * 3)), color);
            //Console.WriteLine($"{X}, {Y}\r\n{VelX}, {VelY}");
            //g.DrawString(string.Format("{0}, {1}\r\n{2}, {3}", X, Y, VelX, VelY), new Font("Arial", 12), b, 10, 45 * (_idJoueur));
        }
    }
}
