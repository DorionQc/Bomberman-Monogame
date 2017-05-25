using System;

using Microsoft.Xna.Framework.Graphics;

namespace TopDownGridBasedEngine
{
    public enum BonusType
    {
        ExtraBomb,
        FirePower,
        RollerSkates,
        Glove,
        Kick,
        MaximumPower
    };

    public class CaseBonus : AbsCase
    {
        private byte _textureVariant;

        public CaseBonus(int x, int y, Map parent, Random r) : base(x, y, parent, false, true, false)
        {
            _textureVariant = 0;
            int ran = r.Next() % 100;
            if (ran < 30)
                BonusType = BonusType.ExtraBomb;
            else if (ran < 50)
                BonusType = BonusType.FirePower;
            else if (ran < 70)
                BonusType = BonusType.RollerSkates;
            else if (ran < 80)
                BonusType = BonusType.Glove;
            else if (ran < 95)
                BonusType = BonusType.Kick;
            else
                BonusType = BonusType.MaximumPower;
        }

        public CaseBonus(int x, int y, Map parent, BonusType t) : base(x, y, parent, false, true, false)
        {
            _textureVariant = 0;
            BonusType = t;
        }



        public override CaseType Type => CaseType.Bonus;

        public BonusType BonusType { get; }

        public override Texture2D Texture
        {
            get
            {
                if (_textureVariant >= 19)
                    _textureVariant = 0;
                else
                    _textureVariant++;
                return TextureManager.Instance.TextureCaseBonus[(int)BonusType, _textureVariant / 10];
            }
        }
    }
}
