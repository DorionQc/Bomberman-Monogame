using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace TopDownGridBasedEngine
{
    public class EntityManager
    {
        // Singleton! :D
        private List<AbsEntity> _entities;

        private object someLock;
        private int _id;
        private List<Fire> _deadFire;

        private static EntityManager _instance;

        public event OnFireStopHandler FireStopped;

        private EntityManager() : this(null, null, 0) { }
        
        private EntityManager(Joueur[] j, Map m, int numeroJoueur)
        {
            _entities = new List<AbsEntity>();
            Joueurs = j;
            Map = m;
            _id = numeroJoueur;
            someLock = new object();
            _deadFire = new List<Fire>();
        }

        protected void FireFireStopped(object sender, MultiFireEventArgs e)
        {
            FireStopped?.Invoke(sender, e);
        }

        public static EntityManager Instance
        {
            get
            {
                if (_instance == null)
                    throw new Exception("Instance inexistante. Utilisez InitInstance avant d'utiliser l'instance");
                return _instance;
            }
        }

        public static void InitInstance(Joueur[] j, Map m, int ID)
        {
            if (_instance != null)
            {
                _instance._entities = new List<AbsEntity>();
                _instance.Joueurs = j;
                _instance.Map = m;
                _instance._id = ID;
            }
            _instance = new EntityManager(j, m, ID);
        }

        public List<AbsEntity> Entities => _entities;

        public Joueur[] Joueurs { get; private set; }

        public Map Map { get; set; }

        public AbsEntity EntityFromID(int ID)
        {
            int i = 0;
            
            lock (someLock)
            {
                while (i < _entities.Count && _entities[i].ID != ID)
                    i++;
            }
            
            return i == _entities.Count ? null : _entities[i];
        }

        public void Add(AbsEntity e)
        {
            lock (someLock)
            {
                _entities.Add(e);

                for (int i = 0; i < _entities.Count; i++)
                    if (_entities.Count(ent => ent.ID == _entities[i].ID) > 1)
                        Console.WriteLine("L'identifiant d'entités " + _entities[i].ID +
                                          "est déja en cours d'utilisation");
            }
        }

        public bool Remove(AbsEntity e)
        {
            Fire fire = e as Fire;
            if (fire != null)
            {
                _deadFire.Add(fire);
            }
            
            lock (someLock)
            {
                return _entities.Remove(e);
            }
        }

        public void TickPlayer(int idPlayer, long deltaTime, KeyWrapper wrapper)
        {
            if (!Joueurs[idPlayer].IsDead)
                Joueurs[idPlayer].TickPlayer(deltaTime, wrapper);
        }

        public void TickEntities(long deltaTime)
        {
            lock (someLock)
            {
                List<AbsEntity> toUpdate = _entities.ToList();

                foreach (AbsEntity e in toUpdate)
                {
                    e.Tick(deltaTime);
                    if (e is ITexturable)
                        ((ITexturable) e).UpdateTexture(deltaTime);
                }
                for (int i = 0; i < 4; i++)
                    if (Joueurs[i] != null && Joueurs[i].IsDead == false)
                        Joueurs[i].UpdateTexture(deltaTime); //Tick(DeltaTime);
                if (Joueurs[_id].IsDead == false)
                    Joueurs[_id].Tick(deltaTime);
                if (_deadFire.Count != 0)
                {
                    FireFireStopped(this, new MultiFireEventArgs(_deadFire.ToArray(), false));
                    _deadFire = new List<Fire>();
                }
            }

        }

        public void Draw(SpriteBatch sb, Rectangle clientRect)
        {
            float w = Map.Width;
            
            List<AbsEntity> toDraw = null;
            
            lock (someLock)
            {
                toDraw = _entities.ToList();
            }
            
            foreach (AbsEntity e in toDraw)
                e.Draw(sb, w / 30);
        }

        public void DrawPlayers(SpriteBatch sb, Rectangle clientRect)
        {
            float w = Map.Width;
            for (int i = 0; i < 4; i++)
                if (Joueurs[i] != null && Joueurs[i].IsDead == false)
                    Joueurs[i].Draw(sb, w / 30);
        }

    }
}
