

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Penumbra;

namespace TopDownGridBasedEngine
{
    public class Fire : AbsEntity, ITexturable
    {
        int _textureVariant;
        int _lifeTime;

        public Fire(int x, int y, Map m, int lifeTime, bool registered, int id = 0)
            : base(x, y, m, registered, id)
        {
            _textureVariant = 0;
            _lifeTime = lifeTime;
            Light = new PointLight();
            Light.Color = Color.OrangeRed;
            Light.Intensity = 1;
            Light.ShadowType = ShadowType.Solid;
            Light.Scale = new Vector2(250, 250);
            Light.Position = new Vector2(x * Map.Width / Map.EntityPixelPerCase, y * Map.Width / Map.EntityPixelPerCase);
            Game1.Penumbra.Lights.Add(Light);
        }
        
        public Light Light { get; }

        public override EntityType Type => EntityType.Fire;

        public override void Draw(SpriteBatch sb, float width)
        {
            Draw(sb, width, Color.White);
        }
        
        public override void Draw(SpriteBatch sb, float width, Color color)
        {
            int rad = Size;
            Texture2D bit = TextureManager.Instance.TextureFire[_textureVariant / 21];
            //g.DrawImage(bit, _x * Width, _y * Width, Width, Width);
            sb.Draw(bit, new Rectangle((int)((X - rad) * width), (int)((Y - rad) * width), (int)(rad * width * 2), (int)(rad * width * 2)), color);
        }
        
        public override void Tick(long deltaTime)
        {
            if (IsRegistered)
            {
                _lifeTime -= (int)deltaTime;
                if (_lifeTime < 0)
                    Update();
            }
        }


        public void UpdateTexture(long deltaTime)
        {
            _textureVariant += (int)deltaTime / 5;
            if (_textureVariant > 39)
                _textureVariant %= 40;
        }

        public void Update()
        {
            
            AbsCase c = Map[X / 30, Y / 30];
            CaseVide vide = c as CaseVide;
            
            if (vide != null)
                vide.Fire = null;
            else
                Map[X / 30, Y / 30] = new CaseVide(X / 30, Y / 30, Map);
            
            Game1.Penumbra.Lights.Remove(Light);
            
            EntityManager.Instance.Remove(this);
        }

    }
}
