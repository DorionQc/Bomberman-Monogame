using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Penumbra;

namespace TopDownGridBasedEngine
{
    public class Map
    {
        private readonly AbsCase[,] _cases;
        private int _noCase;

        private readonly Random _random;

        // Nombre d'unités contenus dans une case, utilisées par les entités
        public const int EntityPixelPerCase = 30;

        public float Width;

        public int NoCase => _noCase;

        public Map(int size, Rectangle clientRect)
        {
            
            _noCase = size + (size % 2 - 1);
            _cases = new AbsCase[size, size];
            _random = new Random();
            
            Width = Math.Min((float)clientRect.Width / NoCase, (float)clientRect.Height / NoCase);
            
            // Remplissage aléatoire de la carte
            for (int x = 1; x < size - 1; x++)
                for (int y = 1; y < size - 1; y++)
                {
                    if (x % 2 == 0 && y % 2 == 0)
                        this[x, y] = new CaseVide(x, y, this);
                    else if (_random.Next() % 2 != 0)
                        this[x, y] = new CaseWall(x, y, this);
                    else
                        this[x, y] = new CaseVide(x, y, this);
                }

            for (int x = 0; x < size - 1; x++)
            {
                this[x, 0] = new CaseSolidWall(x, 0, this);
                this[size - 1, x] = new CaseSolidWall(size - 1, x, this);
                this[size - 1 - x, size - 1] = new CaseSolidWall(size - 1 - x, size - 1, this);
                this[0, size - 1 - x] = new CaseSolidWall(0, size - 1 - x, this);
            }

            // Génération des coins

            int i = 1;
            this[1, 1] = new CaseVide(1, 1, this);
            this[1, _noCase - 2] = new CaseVide(1, _noCase - 2, this);
            this[_noCase - 2, 1] = new CaseVide(_noCase - 2, 1, this);
            this[_noCase - 2, _noCase - 2] = new CaseVide(_noCase - 2, _noCase - 2, this);
            bool tld = true, tlr = true, trd = true, trl = true, blu = true, blr = true, bru = true, brl = true;
            while (i < 4 && (tld || tlr || trd || trl || blu || blr || bru || brl))
            {
                if (tld)
                {
                    this[1, 1 + i] = new CaseVide(1, 1 + i, this);
                    if (_random.Next() % 7 == 0) tld = false;
                }
                if (tlr)
                {
                    this[1 + i, 1] = new CaseVide(1 + i, 1, this);
                    if (_random.Next() % 7 == 0) tlr = false;
                }
                if (trd)
                {
                    this[_noCase - 2, 1 +  i] = new CaseVide(_noCase - 2, 1 +  i, this);
                    if (_random.Next() % 7 == 0) trd = false;
                }
                if (trl)
                {
                    this[_noCase - 2 - i, 1] = new CaseVide(_noCase - 2 - i, 1, this);
                    if (_random.Next() % 7 == 0) trl = false;
                }
                if (blu)
                {
                    this[1, _noCase - 2 - i] = new CaseVide(1, _noCase - 2 - i, this);
                    if (_random.Next() % 7 == 0) blu = false;
                }
                if (blr)
                {
                    this[1 + i, _noCase - 2] = new CaseVide(1 + i, _noCase - 2, this);
                    if (_random.Next() % 7 == 0) blr = false;
                }
                if (bru)
                {
                    this[_noCase - 2, _noCase - 2 - i] = new CaseVide(_noCase - 2, _noCase - 2 - i, this);
                    if (_random.Next() % 7 == 0) bru = false;
                }
                if (brl)
                {
                    this[_noCase - 2 - i, _noCase - 2] = new CaseVide(_noCase - 2 - i, _noCase - 2, this);
                    if (_random.Next() % 7 == 0) brl = false;
                }
                i++;
            }
        }

        // Indexeur pour aller chercher facilement des cases
        public AbsCase this[int x, int y]
        {
            get
            {
                if (x >= 0 && y >= 0 && x < _noCase && y < _noCase)
                    return _cases[x, y];
                throw new IndexOutOfRangeException($"{x}, {y} is outside the range 0-{_noCase - 1}");
            }
            set
            {
                if (x >= 0 && y >= 0 && x < _noCase && y < _noCase)
                {
                    if (this[x, y] != null)
                    {

                        if (this[x, y].Hull != null)
                        {
                            Game1.Penumbra.Hulls.Remove(this[x, y].Hull);
                            if (Game1.Joueurs != null)
                                foreach (Light light in Game1.Joueurs[0].Lights)
                                {
                                    light.Position += Vector2.One;
                                    light.Position -= Vector2.One;
                                }
                        }
                        if (this[x, y].Fire != null)
                            this[x, y].Fire = null;

                        CaseVide vide = this[x, y] as CaseVide;
                        if (vide != null)
                            if (vide.ContainsBomb)
                                vide.Bomb = null;
                    }
                    
                    _cases[x, y] = value;
                }
            }
        }
        
        
        public void Draw(SpriteBatch sb, Rectangle clientRect)
        {
            foreach (AbsCase c in _cases)
                c.Draw(sb, Width);
        }

        /// <summary>
        /// Tries to place a bonus at the specified spot in the map
        /// </summary>
        /// <param name="x">X coords of the case to place a bonus in</param>
        /// <param name="y">Y coords of the case to place a bonus in</param>
        /// <returns>True if a bonus was place, false otherwise</returns>
        public bool MakeRandomBonus(int x, int y)
        {
            if (x < 0 || y < 0 || x >= _noCase || y >= _noCase)
                return false;
            if (!(this[x, y] is CaseWall))
                return false;

            if (_random.Next() % 3 != 0)
                return false;

            this[x, y] = new CaseBonus(x, y, this, _random);
            return true;

        }

        // Size, byte[Size, Size]
        public bool FromByteArray(byte[] data, ref int position)
        {
            if (data.Length < position + 1)
                return false;
            _noCase = data[position++];
            if (data.Length < position + 1 + _noCase * _noCase)
                return false;
            for (int x = 0; x < _noCase; x++)
            {
                for (int y = 0; y < _noCase; y++)
                {
                    if (data[position] == 2)
                        this[x, y] = new CaseSolidWall(x, y, this);
                    else if (data[position] == 1)
                        this[x, y] = new CaseWall(x, y, this);
                    else
                        this[x, y] = new CaseVide(x, y, this);
                    position++;
                }
            }
            return true;
        }

        public byte[] ToByteArray()
        {
            byte[] ret = new byte[_noCase * _noCase + 1];
            ret[0] = (byte)_noCase;
            for (int x = 0; x < _noCase; x++)
            {
                for (int y = 0; y < _noCase; y++)
                {
                    ret[1 + x * _noCase + y] = (byte)this[x, y].Type;
                }
            }
            return ret;
        }

    }
}
