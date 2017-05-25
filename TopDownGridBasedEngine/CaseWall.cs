
using Microsoft.Xna.Framework.Graphics;

namespace TopDownGridBasedEngine
{
    public class CaseWall : AbsCase
    {
        public CaseWall(int x, int y, Map parent) : base(x, y, parent, true, true, false)
        { }

        public override CaseType Type => CaseType.Wall;

        public override Texture2D Texture => TextureManager.Instance.TextureCaseWall;
    }
}
