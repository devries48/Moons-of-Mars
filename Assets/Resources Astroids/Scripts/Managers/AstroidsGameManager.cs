using System.Collections;
using UnityEngine;

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

        public PowerupManager m_powerupManager;

        [SerializeField]
        Camera gameCamera;

        public UIManager m_uiManager = new();

        #endregion


        #region fields
        internal CamBounds m_camBounds;
        internal bool m_GamePlaying;
        internal bool m_GamePaused;

        bool _requestTitleScreen;

        GameObjectPool _astoidPool;
        GameObjectPool _ufoPool;

        PlayerShipController _playerShip;
        EffectsManager _effects;
        CurrentLevel _level;

        #endregion

        enum Menu
        {
            none = 0,
            start = 1,
            exit = 2
        }

        #region Unity Messages

        void Awake()
        {
            SingletonInstanceGuard();
            TryGetComponent(out _effects);

            m_camBounds = new CamBounds(gameCamera);

            if (asteroidPrefab == null)
                Debug.LogError("Asteriod Prefab not set!");

            if (ufoPrefab == null)
                Debug.LogError("UfoPrefab Prefab not set!");

            if (asteroidPrefab == null || ufoPrefab == null)
                return;

            _astoidPool = GameObjectPool.Build(asteroidPrefab, 20, 100);
            _ufoPool = GameObjectPool.Build(ufoPrefab, 2);

            m_uiManager.ResetUI();
        }

        void Start()
        {
            Camera.SetupCurrent(gameCamera);

            if (_astoidPool == null)
                return;

            m_GamePlaying = true;

            StartCoroutine(GameLoop());
            StartCoroutine(UfoSpawnLoop());
            StartCoroutine(m_powerupManager.PowerupSpawnLoop());
        }

        void OnEnable() => __instance = this;

        #endregion

        #region game loops

        IEnumerator GameLoop()
        {
            yield return Wait(1);

            GameStart();

            while (m_GamePlaying)
            {
                if (_requestTitleScreen)
                {
                    _requestTitleScreen = false;
                    m_GamePaused = true;
                    string result = null;
                    yield return Run<string>(ShowTitleScreen(), (output) => result = output);
                }

                while (m_GamePaused)
                    yield return null;

                yield return StartCoroutine(LevelStart());
                yield return StartCoroutine(LevelPlay());
                yield return StartCoroutine(LevelEnd());

                System.GC.Collect();
            }
        }

        IEnumerator UfoSpawnLoop()
        {
            while (m_GamePlaying)
            {
                var wait = Random.Range(15f, 30f);
                SpawnUfo();

                yield return new WaitForSeconds(wait);
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
            m_uiManager.ShowMainMenu();
            yield return null;
        }

        IEnumerator ResumeGame()
        {
            yield return new WaitForSeconds(.5f);
            m_GamePaused = false;
        }

        void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
        }

        IEnumerator LevelStart()
        {
            _playerShip.Recover();
            _playerShip.EnableControls();

            m_uiManager.LevelStarts(_level.Level);

            yield return Wait(2);

            SpawnAsteroids(_level.AstroidsForLevel);
        }

        IEnumerator LevelPlay()
        {
            m_uiManager.LevelPlay();

            while (_playerShip.m_isAlive && _level.HasEnemy)
                yield return null;
        }

        IEnumerator LevelEnd()
        {
            bool gameover = !_playerShip.m_isAlive;

            if (gameover)
            {
                m_GamePaused = true;

                StartCoroutine(RemoveRemainingObjects());

                m_uiManager.GameOver();
                yield return Wait(1);

                Score.Tally();
                yield return Wait(1);

                Score.Reset();
                //powerupManager.DenyAllPower(); // ship should reset itself?

                m_uiManager.Reset();
                GameStart();
            }
            else
            {
                m_uiManager.LevelCleared();
                yield return Wait(1);

                Score.LevelCleared(_level.Level);
                yield return Wait(1);

                AdvanceLevel();
            }
            yield return Wait(2);
        }

        void AdvanceLevel()
        {
            _level.LevelAdvance();
        }

        IEnumerator RemoveRemainingObjects()
        {
            foreach (var obj in FindObjectsOfType<GameMonoBehaviour>())
                obj.RemoveFromGame();

            yield return null;
        }

        #endregion

        #region spawn player, astroids, enemies & powerups

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

                var astroid = _astoidPool.GetFromPool(position, size: new Vector3(2f, 2f, 2f) * scale);
                astroid.GetComponent<AsteroidController>().SetGeneration(generation);

                _level.AstroidAdd();
            }
        }

        #endregion

        public void MenuSelect(int i)
        {
            var menu = (Menu)i;
            switch (menu)
            {
                case Menu.start:
                    m_uiManager.HideMainMenu();
                    StartCoroutine(ResumeGame());
                    break;

                case Menu.exit:
                    var id=m_uiManager.HideMainMenu(false);
                    var d = LeanTween.descr(id);

                    d?.setOnComplete(QuitGame);
                    break;

                case Menu.none:
                default:
                    break;
            }
        }

        // TODO: Move to gamebaseclass? 
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

        /// <summary>
        /// Destroy player with explosion
        /// </summary>
        public void PlayerDestroyed(GameObject player)
        {
            var ship = player.GetComponent<SpaceShipMonoBehaviour>();
            if (ship.m_isAlive)
            {
                ship.Explode();

                PlayerDestroyed();
            }
        }

        /// <summary>
        /// Destroy player without explosion
        /// </summary>
        public void PlayerDestroyed()
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

        public static WaitForSeconds Wait(float duration)
        {
            return new WaitForSeconds(duration);
        }

        static IEnumerator Run<T>(IEnumerator target, System.Action<T> output)
        {
            object result = null;
            while (target.MoveNext())
            {
                result = target.Current;
                yield return result;
            }
            output((T)result);
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

            public void AstroidAdd()
            {
                _asteroidsActive++;
                Log();
            }

            public void AstroidRemove()
            {
                _asteroidsActive--;
                Log();
            }

            public void UfoAdd()
            {
                _ufosActive++;
                Log();
            }

            public void UfoRemove()
            {
                _ufosActive--;
                Log();
            }

            public void Level1() => SetLevel(1);
            public void LevelAdvance() => _level++;

            void Log()
            {
                print("Active Astroids: " + _asteroidsActive + " - Ufo's: " + _ufosActive);
            }

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