using System.Collections;
using TMPro;
using UnityEngine;
using static Game.Astroids.UfoManagerData;

namespace Game.Astroids
{
    [SelectionBase]
    [RequireComponent(typeof(EffectsManager))]
    public class AsteroidsGameManager : MonoBehaviour
    {
        #region singleton
        public static AsteroidsGameManager Instance => __instance;

        static AsteroidsGameManager __instance;

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

        enum Menu { none = 0, start = 1, exit = 2 }
        public enum AudioType { UI, Hud = 1 }

        #region editor fields
        public HudManager m_HudManager;

        [Header("Data")]
        public UfoManagerData m_UfoManager;
        public PowerupManagerData m_PowerupManager;
        public UIManagerData m_UiManager;

        [Header("Prefabs")]
        [SerializeField, Tooltip("Select a spaceship prefab")]
        GameObject playerShipPrefab;

        [SerializeField, Tooltip("Select an astroid prefab")]
        GameObject asteroidPrefab;

        [Header("UI Elements")]
        public GameObject m_MainMenuWindow;
        public TextMeshProUGUI m_ScoreTextUI;
        public TextMeshProUGUI m_AnnouncerTextUI;

        [Header("Camera's")]
        [SerializeField] Camera mainCamera;
        [SerializeField] Camera gameCamera;

        [Header("Other")]
        [SerializeField] AudioSource uiAudioSource;
        [SerializeField] JumpController jumpController;
        #endregion

        #region properties
        public bool IsDay
        {
            get => __isDay;
            set
            {
                __isDay = value;
                m_HudManager.IsDay = value;
            }
        }
        bool __isDay = true;
        #endregion

        #region fields
        internal PlayerShipController m_playerShip;
        internal CamBounds m_camBounds;
        internal bool m_gamePlaying;
        internal bool m_gamePaused;
        internal bool m_debug_godMode;
        internal bool m_debug_no_astroids;
        internal bool m_debug_no_ufos;
        internal CurrentLevel m_level;

        bool _requestTitleScreen;

        GameObjectPool _astoidPool;
        EffectsManager _effects;
        #endregion


        #region unity events
        void Awake()
        {
            SingletonInstanceGuard();

            if (asteroidPrefab == null)
            {
                Debug.LogError("Asteriod Prefab not set!");
                return;
            }

            TryGetComponent(out _effects);

            _astoidPool = GameObjectPool.Build(asteroidPrefab, 20, 100);

            m_camBounds = new CamBounds(gameCamera);
            m_UiManager.InitUI(uiAudioSource);
        }

        void Start()
        {
            Camera.SetupCurrent(gameCamera);

            if (_astoidPool == null)
                return;

            StartCoroutine(GameLoop());
        }

        void OnEnable() => __instance = this;
        #endregion

        #region game loops
        IEnumerator GameLoop()
        {
            yield return Wait(.5f);

            GameStart();

            while (m_gamePlaying)
            {
                if (_requestTitleScreen)
                {
                    Score.Reset();

                    _requestTitleScreen = false;
                    m_gamePaused = true;
                    string result = null;

                    yield return Run<string>(m_UiManager.ShowMainMenu(), (output) => result = output);
                }

                while (m_gamePaused)
                    yield return null;

                yield return StartCoroutine(LevelStart());
                yield return StartCoroutine(LevelPlay());
                yield return StartCoroutine(LevelEnd());

                System.GC.Collect();
            }
        }

        public void GameStart()
        {
            m_playerShip = CreatePlayer(playerShipPrefab);
            m_level.Level1();
            m_gamePlaying = true;

            _requestTitleScreen = true;

            StartCoroutine(m_UfoManager.UfoSpawnLoop());
            StartCoroutine(m_PowerupManager.PowerupSpawnLoop());
        }

        IEnumerator ResumeGame()
        {
            yield return new WaitForSeconds(.5f);
            m_gamePaused = false;
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
            m_UiManager.LevelStarts(m_level.Level);

            if (m_level.Level == 1)
            {
                while (m_UiManager.AudioPlaying)
                    yield return null;

                m_playerShip.Spawn();
                yield return Wait(2);

                m_HudManager.HudShow();
                m_playerShip.EnableControls();
            }
            yield return Wait(1.5f);

            m_playerShip.Refuel();
            SpawnAsteroids(m_level.AstroidsForLevel);
        }

        IEnumerator LevelPlay()
        {
            m_UiManager.LevelPlay();

            while (m_playerShip.m_isAlive && m_level.HasEnemy || m_debug_no_astroids)
                yield return null;
        }

        IEnumerator LevelEnd()
        {
            bool gameover = !m_playerShip.m_isAlive;

            if (gameover)
            {
                m_gamePaused = true;

                StartCoroutine(m_UiManager.GameOver());
                StartCoroutine(RemoveRemainingObjects());

                while (m_UiManager.AudioPlaying)
                    yield return null;

                yield return Wait(1f);

                GameStart();
            }
            else
            {
                StartCoroutine(m_UiManager.LevelCleared(m_level.Level));

                yield return Wait(2f);

                m_level.LevelAdvance();
            }
            yield return Wait(1);
        }

        public IEnumerator RemoveRemainingObjects()
        {
            foreach (var obj in FindObjectsOfType<GameMonoBehaviour>())
            {
                if (!obj.gameObject.CompareTag("Player"))
                    obj.RemoveFromGame();
            }

            yield return null;
        }

        #endregion

        #region spawn player, astroids
        PlayerShipController CreatePlayer(GameObject spaceShip)
        {
            var ship = Instantiate(spaceShip);
            ship.TryGetComponent(out PlayerShipController shipCtrl);
            if (ship)
                m_HudManager.ConnectToShip(shipCtrl);

            return shipCtrl;
        }

        public void SpawnAsteroids(float asteroidsNum, int generation = 1, Vector3 position = default)
        {
            if (m_debug_no_astroids)
                return;

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

                m_level.AstroidAdd();
            }
        }
        #endregion

        public void MenuSelect(int i)
        {
            var menu = (Menu)i;
            switch (menu)
            {
                case Menu.start:
                    m_UiManager.HideMainMenu();
                    StartCoroutine(ResumeGame());
                    break;

                case Menu.exit:
                    var id = m_UiManager.HideMainMenu(false);
                    var d = LeanTween.descr(id);

                    d?.setOnComplete(QuitGame);
                    break;

                case Menu.none:
                default:
                    break;
            }
        }

        public void PlayEffect(EffectsManager.Effect effect, Vector3 position, float scale = 1f) => _effects.StartEffect(effect, position, scale);

        /// <summary>
        /// Activate jump-crosshair
        /// </summary>
        public void JumpSelect(Vector3 pos, float timer)
        {
            jumpController.gameObject.SetActive(true);
            jumpController.StartCountdown(pos, timer);
        }

        /// <summary>
        /// Deactivate jump-crosshair
        /// </summary>
        public void JumpDeactivate() => jumpController.gameObject.SetActive(false);

        /// <summary>
        /// Return selected jump-position
        /// </summary>
        public Vector3 JumpPosition() => jumpController.m_JumpPosition;

        public bool JumpLaunched() => jumpController.m_Launched;

        public Vector3 GetWorldJumpPosition()
        {
            var pos = JumpPosition();
            pos.z = 0;

            var gameToWorld = gameCamera.ViewportToScreenPoint(pos);
            gameToWorld.z = 0;
            gameToWorld.x *= 1.85f;
            gameToWorld.y *= 2;

            return mainCamera.ScreenToViewportPoint(gameToWorld);
        }


        /// <summary>
        /// Destroy player with explosion
        /// </summary>
        public void PlayerDestroyed()
        {
            if (m_debug_godMode)
                return;

            m_HudManager.HudHide();

            if (m_playerShip.m_isAlive)
            {
                m_playerShip.DisableControls();
                m_playerShip.Explode();

                Cursor.visible = true;
                print("GAME OVER");
            }
        }

        public void AsterodDestroyed() => m_level.AstroidRemove();

        public void UfoDestroyed(UfoType type) => m_level.UfoRemove(type);

        public static WaitForSeconds Wait(float duration) => new(duration);

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

        public readonly struct CamBounds
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

        public struct CurrentLevel
        {
            int _level;
            int _astroidsForLevel;
            int _asteroidsActive;
            int _ufosGreenActive;
            int _ufosRedActive;
            int _ufosForLevel;

            public int Level => _level;
            public int AstroidsForLevel => _astroidsForLevel;
            public int AstroidsActive => _asteroidsActive;
            public int TotalUfosActive => _ufosGreenActive + _ufosRedActive;
            public int UfosGreenActive => _ufosGreenActive;
            public int UfosRedActive => _ufosRedActive;
            public bool HasEnemy => _asteroidsActive > 0 || TotalUfosActive > 0;
            public bool CanAddUfo => TotalUfosActive < _ufosForLevel && _asteroidsActive > 0;

            public void AstroidAdd() => _asteroidsActive++;
            public void AstroidRemove() => _asteroidsActive--;
            public void UfoAdd(UfoType type)
            {
                if (type == UfoType.green)
                    _ufosGreenActive++;
                else
                    _ufosRedActive++;
            }

            public void UfoRemove(UfoType type)
            {
                if (type == UfoType.green)
                    _ufosGreenActive--;
                else
                    _ufosRedActive--;
            }

            public void Level1() => SetLevel(1);
            public void LevelAdvance() => _level++;

            void SetLevel(int level)
            {
                _level = level;
                _astroidsForLevel = level + 1;
                _ufosForLevel = level;

                _asteroidsActive = 0;
                _ufosGreenActive = 0;
                _ufosRedActive = 0;
            }
        }
        #endregion
    }
}