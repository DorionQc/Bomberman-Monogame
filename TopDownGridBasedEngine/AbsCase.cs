using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Penumbra;

namespace TopDownGridBasedEngine
{

    public enum CaseType
    {
        Vide = 0,
        Wall,
        SolidWall,
        Bonus
    };

    public abstract class AbsCase
    {
        protected AbsCase(int x, int y, Map m, bool solid, bool breakable, bool letsFireThrough)
        {
            X = x;
            Y = y;
            Map = m;
            IsSolid = solid;
            IsBreakable = breakable;
            LetsFireThrough = letsFireThrough;

            if (solid)
            {
                Hull = Hull.CreateRectangle(new Vector2(x * Map.Width + Map.Width / 2, y * Map.Width + Map.Width / 2), new Vector2(Map.Width, Map.Width));
                Game1.Penumbra.Hulls.Add(Hull);
            }
        }
        
        public Hull Hull { get; }

        /// <summary>
        /// Indice vertical dans le tableau de la carte
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// Indice vertical dans le tableau de la carte
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// La carte qui contient cette case
        /// </summary>
        public Map Map { get; set; }

        /// <summary>
        /// Indique si la case est destructible ou non
        /// </summary>
        public bool IsBreakable { get; set; }

        /// <summary>
        /// Indique si la case est en processus de destruction
        /// </summary>
        public bool IsBreaking
        {
            get { return Fire != null; }
            set { throw new System.NotImplementedException(); }
        }

        /// <summary>
        /// Référence vers le feu contenu dans cette case, ou null
        /// </summary>
        public Fire Fire { get; set; }

        /// <summary>
        /// Indique si le feu peut traverser librement cette case
        /// </summary>
        public bool LetsFireThrough { get; set; }

        /// <summary>
        /// Indique si le joueur peut traverser librement cette case
        /// </summary>
        public virtual bool IsSolid { get; set; }

        public abstract Texture2D Texture { get; }

        public abstract CaseType Type { get; }

        public virtual void Draw(SpriteBatch sb, float width)
        {
            
            sb.Draw(Texture, new Rectangle((int)(X * width), (int)(Y * width), (int)width, (int)width), Color.White);
        }

        public override string ToString()
        {
            return $"{Type} at {X}, {Y}";
        }
    }
}
