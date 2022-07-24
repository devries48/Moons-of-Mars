using System.Collections;
using UnityEngine;

namespace Game.Astroids
{
    [RequireComponent(typeof(EffectsManager))]
    public class AstroidsGameManager : MonoBehaviour
    {
        #region Singleton

        public static AstroidsGameManager Instance => __instance;
        static AstroidsGameManager __instance;

        void SingletonInstanceGuard()
        {
            if (__instance == null)
            {
                __instance = this;
                DontDestroyOnLoad(transform.root.gameObject);
            }
            else
            {
                Destroy(gameObject);
                throw new System.Exception("Only one instance is allowed");
            }
        }
        #endregion

        #region editor fields

        [SerializeField, Tooltip("Select a spaceship prefab")]
        GameObject shipPrefab;

        [SerializeField, Tooltip("Select an astroid prefab")]
        GameObject asteroidPrefab;

        [SerializeField, Tooltip("Select an UFO prefab")]
        GameObject ufoPrefab;

        [SerializeField]
        Camera gameCamera;
        #endregion

        #region fields

        bool _requestTitleScreen;

        GameObjectPool _astoidPool;
        GameObjectPool _ufoPool;

        PlayerShipController _playerShip;
        EffectsManager _effects;

        CurrentLevel _level;
        CamBounds _camBounds;

        public bool m_GamePlaying;
        #endregion

        void Awake()
        {
            SingletonInstanceGuard();
            TryGetComponent(out _effects);

            _camBounds = new CamBounds(gameCamera);

            if (asteroidPrefab == null)
                Debug.LogWarning("Asteriod Prefab not set!");

            if (ufoPrefab == null)
                Debug.LogWarning("UfoPrefab Prefab not set!");

            if (asteroidPrefab == null || ufoPrefab == null)
                return;

            _astoidPool = GameObjectPool.Build(asteroidPrefab, 20, 100);
            _ufoPool = GameObjectPool.Build(ufoPrefab, 2);
        }

        void Start()
        {
            Camera.SetupCurrent(gameCamera);

            if (_astoidPool == null)
                return;

            m_GamePlaying = true;

            StartCoroutine(GameLoop());
            StartCoroutine(UfoSpawnLoop());
        }

        void OnEnable() => __instance = this;

        #region Game Loop

        IEnumerator GameLoop()
        {
            GameStart();

            while (m_GamePlaying)
            {
                if (_requestTitleScreen)
                {
                    _requestTitleScreen = false;
                    // yield return StartCoroutine(ShowTitleScreen());
                }
                yield return StartCoroutine(LevelStart());
                yield return StartCoroutine(LevelPlay());
                yield return StartCoroutine(LevelEnd());

                System.GC.Collect();
            }
        }

        void GameStart()
        {
            _playerShip = SpawnPlayer(shipPrefab);
            _requestTitleScreen = true;
            _level.Level1();
        }

        IEnumerator ShowTitleScreen()
        {
            //announce.Title();
            while (!Input.anyKeyDown) yield return null;
        }

        IEnumerator LevelStart()
        {
            _playerShip.Recover();
            _playerShip.EnableControls();
            print("// announce.LevelStarts(level);");
            yield return PauseLong();

            SpawnAsteroids(_level.AstroidsForLevel, 1);
        }

        IEnumerator LevelPlay()
        {
            print("//announce.LevelPlaying();");
            while (_playerShip.IsAlive && _level.HasEnemy) yield return null;
        }

        IEnumerator LevelEnd()
        {
            bool gameover = !_playerShip.IsAlive;

            if (gameover)
            {
                print("//announce.GameOver();");
                yield return PauseBrief();
                Score.Tally();
                yield return PauseBrief();
                Score.Reset();
                RemoveRemainingGameTokens();
                //powerupManager.DenyAllPower(); // ship should reset itself?
                print("//announce.ClearAnnouncements();");
                GameStart();
            }
            else
            {
                print("level:" + _level.Level);
                print("//announce.LevelCleared();");
                yield return PauseBrief();
                Score.LevelCleared(_level.Level);
                yield return PauseBrief();
                AdvanceLevel();
            }
            yield return PauseLong();
        }

        void AdvanceLevel()
        {
            _level.LevelAdvance();
        }

        void RemoveRemainingGameTokens()
        {
            //    foreach (var a in FindObjectsOfType<GameToken>())
            //        a.RemoveFromGame();
        }

        #endregion

        IEnumerator UfoSpawnLoop()
        {
            while (m_GamePlaying)
            {
                var wait = Random.Range(15f, 30f);

                if (_level.HasAstroids)
                    SpawnUfo();

                yield return new WaitForSeconds(wait);
            }
        }

        PlayerShipController SpawnPlayer(GameObject spaceShip)
        {
            GameObject ship = Instantiate(spaceShip);
            ship.TryGetComponent(out PlayerShipController shipCtrl);

            return shipCtrl;
        }

        void SpawnUfo()
        {
            var position = ufoPrefab.transform.position;
            var ufo = _ufoPool.GetFromPool(position);
            print("ufo");
        }

        public void SpawnAsteroids(float asteroidsNum, int generation, Vector3 position = default)
        {
            var isRandom = position == default;

            for (int i = 1; i <= asteroidsNum; i++)
            {
                if (isRandom)
                    position = new Vector3(Random.Range(-20, 20), 10f);

                var scaleFact = generation switch
                {
                    1 => 1f,
                    2 => .5f,
                    _ => .25f
                };

                var astroid = _astoidPool.GetFromPool(position, scale: scaleFact);

                astroid.GetComponent<AsteroidController>().SetGeneration(generation);
                _level.AstroidAdd();
            }
        }

        //public static Vector3 WorldPos(Vector3 screenPos)
        //{
        //    return Camera.current.ScreenToWorldPoint(screenPos);
        //}

        public void ScreenWrapObject(GameObject obj)
        {
            var pos = obj.transform.position;
            var offset = obj.transform.localScale / 2;

            if (pos.x > _camBounds.RightEdge + offset.x)
                obj.transform.position = new Vector2(_camBounds.LeftEdge - offset.x, pos.y);

            if (pos.x < _camBounds.LeftEdge - offset.x)
                obj.transform.position = new Vector2(_camBounds.RightEdge + offset.x, pos.y);

            if (pos.y > _camBounds.TopEdge + offset.y)
                obj.transform.position = new Vector2(pos.x, _camBounds.BottomEdge - offset.y);

            if (pos.y < _camBounds.BottomEdge - offset.y)
                obj.transform.position = new Vector2(pos.x, _camBounds.TopEdge + offset.y);
        }

        public void PlayEffect(EffectsManager.Effect effect, Vector3 position, float scale = 1f) => _effects.StartEffect(effect, position, scale);

        public void RocketFail()
        {
            Cursor.visible = true;
            print("GAME OVER");
        }

        public void AsterodDestroyed()
        {
            _level.AstroidRemove();
        }

        public void UfoDestroyed()
        {
            _level.UfoRemove();
        }

        public static WaitForSeconds PauseLong()
        {
            return new WaitForSeconds(2f);
        }

        public static WaitForSeconds PauseBrief()
        {
            return new WaitForSeconds(1f);
        }

        #region struct CamBounds

        struct CamBounds
        {
            public CamBounds(Camera cam)
            {
                Width = cam.orthographicSize * 2 * cam.aspect;
                Height = cam.orthographicSize * 2;

                RightEdge = Width / 2;
                LeftEdge = RightEdge * -1;
                TopEdge = Height / 2;
                BottomEdge = TopEdge * -1;
            }

            public readonly float Width;
            public readonly float Height;
            public readonly float RightEdge;
            public readonly float LeftEdge;
            public readonly float TopEdge;
            public readonly float BottomEdge;
        }
        #endregion

        struct CurrentLevel
        {
            int _level;
            int _astroidsForLevel;
            int _asteroidsActive;
            int _ufosActive;

            public int Level => _level;
            public int AstroidsForLevel => _astroidsForLevel;
            public bool HasAstroids => _asteroidsActive > 0;
            public bool HasEnemy => _asteroidsActive > 0 || _ufosActive > 0;

            public void AstroidAdd() => _asteroidsActive++;
            public void AstroidRemove() => _asteroidsActive--;
            public void UfoAdd() => _asteroidsActive++;
            public void UfoRemove() => _asteroidsActive--;
            public void Level1() => SetLevel(1);
            public void LevelAdvance() => _level++;

            void SetLevel(int level)
            {
                _level = level;
                _astroidsForLevel = level + 1;

                _asteroidsActive = 0;
                _ufosActive = 0;
            }

        }
    }
}