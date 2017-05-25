using Microsoft.Xna.Framework.Graphics;

namespace TopDownGridBasedEngine
{
    public class CaseSolidWall : AbsCase
    {
        public CaseSolidWall(int x, int y, Map parent) : base(x, y, parent, true, false, false)
        { }

        public override CaseType Type => CaseType.SolidWall;

        public override Texture2D Texture { get; } = TextureManager.Instance.TextureCaseSolidWall;
    }
}
