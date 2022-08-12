using System.Collections;
using UnityEngine;
using TMPro;

namespace Game.Astroids
{
    [SelectionBase]
    [RequireComponent(typeof(EffectsManager))]
    public class AstroidsGameManager : MonoBehaviour
    {
        #region singleton

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
        TextMeshProUGUI announcerTextUI;

        [SerializeField]
        Camera gameCamera;
        #endregion

        #region fields
        internal CamBounds m_camBounds;
        internal bool m_GamePlaying;

        bool _requestTitleScreen;

        GameObjectPool _astoidPool;
        GameObjectPool _ufoPool;

        PlayerShipController _playerShip;
        EffectsManager _effects;
        GameAnnouncer _announce;
        CurrentLevel _level;

        #endregion

        void Awake()
        {
            SingletonInstanceGuard();
            TryGetComponent(out _effects);

            m_camBounds = new CamBounds(gameCamera);

            if (asteroidPrefab == null)
                Debug.LogWarning("Asteriod Prefab not set!");

            if (ufoPrefab == null)
                Debug.LogWarning("UfoPrefab Prefab not set!");

            if (asteroidPrefab == null || ufoPrefab == null)
                return;

            _astoidPool = GameObjectPool.Build(asteroidPrefab, 20, 100);
            _ufoPool = GameObjectPool.Build(ufoPrefab, 2);
            _announce = GameAnnouncer.AnnounceTo(Announcer.TextComponent(announcerTextUI), Announcer.Log(this));
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

                    yield return StartCoroutine(ShowTitleScreen());
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

        //todo make gameobject
        IEnumerator ShowTitleScreen()
        {
            _announce.Title();

            while (!Input.anyKeyDown) yield return null;
        }

        IEnumerator LevelStart()
        {
            _playerShip.Recover();
            _playerShip.EnableControls();
            _announce.LevelStarts(_level.Level);

            yield return PauseLong();

            SpawnAsteroids(_level.AstroidsForLevel);
        }

        IEnumerator LevelPlay()
        {
            _announce.LevelPlaying();

            while (_playerShip.IsAlive && _level.HasEnemy)
                yield return null;

            print("alive:" + _playerShip.IsAlive);
            print("enemy:" + _level.HasEnemy);
        }

        IEnumerator LevelEnd()
        {
            bool gameover = !_playerShip.IsAlive;

            if (gameover)
            {
                _announce.GameOver();
                yield return PauseBrief();

                Score.Tally();
                yield return PauseBrief();

                Score.Reset();
                //powerupManager.DenyAllPower(); // ship should reset itself?
                _announce.ClearAnnouncements();
                GameStart();
            }
            else
            {
                _announce.LevelCleared();
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

        #endregion

        IEnumerator UfoSpawnLoop()
        {
            while (m_GamePlaying)
            {
                var wait = Random.Range(15f, 30f);
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
            if (!_level.CanAddUfo)
                return;

            _ufoPool.GetFromPool();
            _level.UfoAdd();
        }

        public void SpawnAsteroids(float asteroidsNum, int generation = 1, Vector3 position = default)
        {
            var isRandom = position == default;

            for (int i = 1; i <= asteroidsNum; i++)
            {
                if (isRandom)
                    position = new Vector3(Random.Range(-20, 20), 10f);

                var scale = generation switch
                {
                    1 => 1f,
                    2 => .5f,
                    _ => .25f
                };

                print("1");
                var astroid = _astoidPool.GetFromPool(position, size: new Vector3(2f, 2f, 2f) * scale);
                print("2");
                astroid.GetComponent<AsteroidController>().SetGeneration(generation);
                print("3");
                _level.AstroidAdd();
                print("4");
            }
        }

        public void ScreenWrapObject(GameObject obj)
        {
            var pos = obj.transform.position;
            var offset = obj.transform.localScale / 2;

            if (pos.x > m_camBounds.RightEdge + offset.x)
                obj.transform.position = new Vector2(m_camBounds.LeftEdge - offset.x, pos.y);

            if (pos.x < m_camBounds.LeftEdge - offset.x)
                obj.transform.position = new Vector2(m_camBounds.RightEdge + offset.x, pos.y);

            if (pos.y > m_camBounds.TopEdge + offset.y)
                obj.transform.position = new Vector2(pos.x, m_camBounds.BottomEdge - offset.y);

            if (pos.y < m_camBounds.BottomEdge - offset.y)
                obj.transform.position = new Vector2(pos.x, m_camBounds.TopEdge + offset.y);
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

        public struct CamBounds
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

        #region struct CurrentLevel

        struct CurrentLevel
        {
            int _level;
            int _astroidsForLevel;
            int _asteroidsActive;
            int _ufosActive;
            int _ufosForLevel;

            public int Level => _level;
            public int AstroidsForLevel => _astroidsForLevel;
            public bool HasEnemy => _asteroidsActive > 0 || _ufosActive > 0;
            public bool CanAddUfo => _ufosActive < _ufosForLevel && _asteroidsActive > 0;

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
                _ufosForLevel = level;

                _asteroidsActive = 0;
                _ufosActive = 0;
            }
        }
        #endregion
    }
}