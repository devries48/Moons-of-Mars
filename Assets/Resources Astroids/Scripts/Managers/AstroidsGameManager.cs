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

        [SerializeField]
        Camera gameCamera;
        #endregion

        #region fields
        bool _requestTitleScreen;
        int _currentLevel;
        int _numAstroidsForLevel;

        int _asteroidsActive;

        GameObjectPool _astoidsPool;

        PlayerShipController _playerShip;
        EffectsManager _effects;

        CamBounds _camBounds;
        #endregion

        void Awake()
        {
            SingletonInstanceGuard();
            TryGetComponent(out _effects);

            _astoidsPool = GameObjectPool.Build(asteroidPrefab, 20, 100);
            _camBounds = new CamBounds(gameCamera);
        }

        void Start()
        {
            Camera.SetupCurrent(gameCamera);
            StartCoroutine(GameLoop());
        }

        void OnEnable() => __instance = this;

        PlayerShipController SpawnPlayer(GameObject spaceShip)
        {
            GameObject ship = Instantiate(spaceShip);
            ship.TryGetComponent(out PlayerShipController shipCtrl);

            return shipCtrl;
        }


        IEnumerator GameLoop()
        {
            GameStart();

            while (true)
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

        IEnumerator ShowTitleScreen()
        {
            //announce.Title();
            while (!Input.anyKeyDown) yield return null;
        }

        void GameStart()
        {
            _playerShip = SpawnPlayer(shipPrefab);
            _requestTitleScreen = true;
            _currentLevel = 1;
            _numAstroidsForLevel = 2;
        }

        IEnumerator LevelStart()
        {
            _playerShip.Recover();
            _playerShip.EnableControls();
            // announce.LevelStarts(level);
            yield return PauseLong();
            SpawnAsteroids(_numAstroidsForLevel, 1);
        }

        IEnumerator LevelPlay()
        {
            //announce.LevelPlaying();
            while (_playerShip.IsAlive && _asteroidsActive > 0) yield return null;
        }

        IEnumerator LevelEnd()
        {
            bool gameover = !_playerShip.IsAlive;  //AsteroidBehaviour.Any;
            if (gameover)
            {
                //announce.GameOver();
                yield return PauseBrief();
                Score.Tally();
                yield return PauseBrief();
                Score.Reset();
                RemoveRemainingGameTokens();
                //powerupManager.DenyAllPower(); // ship should reset itself?
                //announce.ClearAnnouncements();
                GameStart();
            }
            else
            {
                //announce.LevelCleared();
                yield return PauseBrief();
                Score.LevelCleared(_currentLevel);
                yield return PauseBrief();
                AdvanceLevel();
            }
            yield return PauseLong();
        }

        void AdvanceLevel()
        {
            _currentLevel++;
            _numAstroidsForLevel += _currentLevel;
        }

        void RemoveRemainingGameTokens()
        {
        //    foreach (var a in FindObjectsOfType<GameToken>())
        //        a.RemoveFromGame();
        }

        void SpawnAsteroids(float asteroidsNum, int generation, Vector3 position = default)
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

                var astroid = _astoidsPool.GetFromPool(position, scale: scaleFact);

                astroid.GetComponent<AsteroidController>().SetGeneration(generation);
                ++_asteroidsActive;
            }
        }

        //public static Vector3 WorldPos(Vector3 screenPos)
        //{
        //    return Camera.current.ScreenToWorldPoint(screenPos);
        //}

        public void RePosition(GameObject obj)
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

        public PlayerShipController Spawn(GameObject prefab)
        {
            GameObject clone = Instantiate(prefab);
            var existingShip = clone.GetComponent<PlayerShipController>();
            return existingShip ? existingShip : clone.AddComponent<PlayerShipController>();
        }

        public void PlayEffect(EffectsManager.Effect effect, Vector3 position, float scale = 1f) => _effects.StartEffect(effect, position, scale);

        public void RocketFail()
        {
            Cursor.visible = true;
            print("GAME OVER");
        }

        public void AsterodDestroyed()
        {
            _asteroidsActive--;
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

    }
}