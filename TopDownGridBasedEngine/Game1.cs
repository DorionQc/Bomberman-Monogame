using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using Penumbra;

namespace TopDownGridBasedEngine
{
    public class Game1 : Game
    {
        private readonly GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private KeyWrapper _wrapper;
        private Map _map;
        public static Joueur[] Joueurs;
        
        public static PenumbraComponent Penumbra;
        
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            _graphics.PreferredBackBufferHeight = 900;
            _graphics.PreferredBackBufferWidth = 900;
            _graphics.ApplyChanges();
            Window.AllowUserResizing = true;
            Penumbra = new PenumbraComponent(this);
            Components.Add(Penumbra);

            Penumbra.AmbientColor = Color.Black;
            
            base.Initialize();
            
            _map = new Map(29, Window.ClientBounds);
            Joueurs = new Joueur[4];
            _wrapper = new KeyWrapper(0, 0, 0);

            
            
            int[,] DefaultLocations = new int[4, 2]
            {
                { Map.EntityPixelPerCase + 10, Map.EntityPixelPerCase + 10 },
                { Map.EntityPixelPerCase * (_map.NoCase - 1) - 10, Map.EntityPixelPerCase + 10 },
                { Map.EntityPixelPerCase * (_map.NoCase - 1) - 10, Map.EntityPixelPerCase * (_map.NoCase - 1) - 10 },
                { Map.EntityPixelPerCase + 10, Map.EntityPixelPerCase * (_map.NoCase - 1) - 10 }
            };
            
            for (int i = 0; i < 1; i++)
            {
                Joueurs[i] = new Joueur(DefaultLocations[i, 0], DefaultLocations[i, 1], _map, (byte)i);
            }
            
            
            
            Joueurs[0].Moved += OnPlayerMoved;
            Joueurs[0].DroppedBomb += OnPlayerDroppedBomb;
            Joueurs[0].BombExploded += OnPlayerBombExploded;
            Joueurs[0].PickedBomb += OnPlayerPickedBomb;
            Joueurs[0].KickedBomb += OnPlayerKickedBomb;
            Joueurs[0].Died += OnPlayerDied;
            Joueurs[0].ShotBomb += OnPlayerShotBomb;
            Joueurs[0].PickedBonus += OnPlayerPickedBonus;
            Joueurs[0].BombBrokeBlocks += OnPlayerBombBrokeBlocks;
            Joueurs[0].BombPlacedBonus += OnPlayerBombPlacedBonus;
            
            EntityManager.InitInstance(Joueurs, _map, 0);
            
            
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            TextureManager.InitInstance(Content);
            
            Penumbra.Initialize();
            Penumbra.Visible = true;
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == 
                ButtonState.Pressed || Keyboard.GetState().IsKeyDown(
                    Keys.Escape))
                Exit();
            
            _wrapper.MouseX = Mouse.GetState().X;
            _wrapper.MouseY = Mouse.GetState().Y;
            _wrapper.State = 0;
            if (Keyboard.GetState().IsKeyDown(Keys.Up))
                _wrapper.State |= KeyState.Up;
            if (Keyboard.GetState().IsKeyDown(Keys.Down))
                _wrapper.State |= KeyState.Down;
            if (Keyboard.GetState().IsKeyDown(Keys.Left))
                _wrapper.State |= KeyState.Left;
            if (Keyboard.GetState().IsKeyDown(Keys.Right))
                _wrapper.State |= KeyState.Right;
            _wrapper.ToggleSpace(Keyboard.GetState().IsKeyDown(Keys.Space));
            EntityManager.Instance.TickPlayer(0, gameTime.ElapsedGameTime.Milliseconds, _wrapper);
            _wrapper.ResetSpace();
            EntityManager.Instance.TickEntities(gameTime.ElapsedGameTime.Milliseconds);
            
            
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {

            // Light affected stuff.
            Penumbra.Visible = true;
            Penumbra.BeginDraw();
            
            GraphicsDevice.Clear(Color.Black);
            _spriteBatch.Begin();
            _map.Draw(_spriteBatch, Window.ClientBounds);
            EntityManager.Instance.Draw(_spriteBatch, Window.ClientBounds);
            //EntityManager.Instance.DrawPlayers(_spriteBatch, Window.ClientBounds);
            _spriteBatch.End();
            //Penumbra.Draw(gameTime);
            
                                    
            base.Draw(gameTime);
            
            _spriteBatch.Begin();
            EntityManager.Instance.DrawPlayers(_spriteBatch, Window.ClientBounds);
            _spriteBatch.End();

        }
        
        
        
        
        
        
        /// <summary>
        /// Lorsqu'un joueur place une bombe
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPlayerBombPlacedBonus(object sender, CaseEventArgs e)
        {
            if (!(sender is Bomb))
                return;
            Bomb b = (Bomb)sender;
            if (!(b.Owner == Joueurs[0]))
                return;
            if (!(e.Case is CaseBonus))
                return;
            CaseBonus c = (CaseBonus)e.Case;
            // On envoie la bombe pas TCP au Server
            //Console.WriteLine("Bonus was placed at " + e.Case.X + ", " + e.Case.Y + ", of type " + ((CaseBonus)e.Case).BonusType.ToString());
        }

        /// <summary>
        /// Lorsqu'une bombe brise des cases
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPlayerBombBrokeBlocks(object sender, MultiCaseEventArgs e)
        {
            if (!(sender is Bomb))
                return;
            Bomb b = (Bomb)sender;
            if (b.Owner != Joueurs[0])
                return;
            //Console.WriteLine("Bomb broke ");
            //foreach (AbsCase c in e.Cases)
            //    Console.WriteLine(c.ToString());
            Point[] p = new Point[e.Cases.Length];
            for (int i = 0; i < e.Cases.Length; i++)
                p[i] = new Point(e.Cases[i].X, e.Cases[i].Y);
            // On envoie un tableau de Points contenant les positions des cases brisées
        }

        /// <summary>
        /// Lorsqu'une bombe explose
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPlayerBombExploded(object sender, CaseEventArgs e)
        {
            if (!(sender is Bomb))
                return;
            Bomb b = (Bomb)sender;
            if (!(b.Owner == Joueurs[0]))
                return;
            //Console.WriteLine("Bomb exploded at " + e.Case.X + ", " + e.Case.Y);
            // On envoie un paquet pour dire que la bombe a explosé!

        }

        /// <summary>
        /// Lorsqu'un joueur obtient un bonus
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPlayerPickedBonus(object sender, CaseEventArgs e)
        {
            if (!(sender is Joueur))
                return;
            Joueur j = (Joueur)sender;
            if (!(e.Case is CaseBonus))
                return;
            CaseBonus c = (CaseBonus)e.Case;
            if (j.PlayerID == 0)
            {
                //Console.WriteLine("Picked bonus at " + e.Case.X + ", " + e.Case.Y);
                // On envoie un paquet pour dire que le joueur a pris le bonus
            }
        }

        /// <summary>
        /// Lorsqu'un joueur lance une bombe
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPlayerShotBomb(object sender, ShootBombEventArgs e)
        {
            if (!(sender is Joueur))
                return;
            Joueur j = (Joueur)sender;
            if (j.PlayerID == 0)
            {
                //Console.WriteLine("Shot bomb " + e.Side.ToString());
                // On envoie un paquet pour dire que le joueur a lancé une bombe, en spécifiant la direction
            }
        }

        /// <summary>
        /// Lorsqu'un joueur meurt
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPlayerDied(object sender, CancellableEventArgs e)
        {
            if (!(sender is Joueur))
                return;
            Joueur j = (Joueur)sender;
            if (j.PlayerID == 0)
            {
                //Console.WriteLine("Died");
                // On envoie un avis de décès au Server
            }
        }

        /// <summary>
        /// Lorsqu'un joueur prent une bombe dans ses mains
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPlayerPickedBomb(object sender, CaseEventArgs e)
        {
            if (!(sender is Joueur))
                return;
            Joueur j = (Joueur)sender;
            if (!(e.Case is CaseVide))
                return;
            CaseVide cv = (CaseVide)e.Case;
            if (cv.ContainsBomb == false)
                return;
            if (j.PlayerID == 0)
            {
                //Console.WriteLine("Picked bomb");
                // On en informe le Server
            }
        }

        /// <summary>
        /// Lorsqu'un joueur botte une bombe
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPlayerKickedBomb(object sender, KickedBombEventArgs e)
        {
            if (!(sender is Joueur))
                return;
            Joueur j = (Joueur)sender;
            if (j.PlayerID == 0)
            {
                //Console.WriteLine("Kicked bomb");
                // On en informe le Server en spécifiant la direction
            }
        }

        /// <summary>
        /// Lorsqu'un joueur place une bombe
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPlayerDroppedBomb(object sender, CaseEventArgs e)
        {
            if (!(sender is Joueur))
                return;
            Joueur j = (Joueur)sender;
            if (!(e.Case is CaseVide))
                return;
            CaseVide c = (CaseVide)e.Case;
            if (c.ContainsBomb == false)
                return;
            if (j.PlayerID == 0)
            {
                //Console.WriteLine("Dropped bomb");
                // On envoie la position et l'ID de la bombe au Server
            }
        }

        /// <summary>
        /// Lorsque le joueur bouge
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPlayerMoved(object sender, CancellableEventArgs e)
        {
            if (!(sender is Joueur))
                return;
            Joueur j = (Joueur)sender;
            if (j.PlayerID == 0)
            {
                // On envoie, par UDP, la nouvelle position et la vélocité du joueur!
            }
        }
    }
}