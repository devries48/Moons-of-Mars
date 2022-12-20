using Cinemachine;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using static Cinemachine.CinemachineBrain;
using static EffectsManager;
using static Game.Astroids.UfoManagerData;
using static MusicData;
using static Utils;

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
        public enum StageCamera { start, background, end }
        public enum AudioType { UI, Hud = 1 }

        #region editor fields
        public GameManagerData m_GameManagerData;
        public HudManager m_HudManager;
        public LevelManager m_LevelManager;

        [Header("UI Elements")]
        public GameObject m_MainMenuWindow;
        public TextMeshProUGUI m_ScoreTextUI;
        public TextMeshProUGUI m_AnnouncerTextUI;

        [Header("Camera's")]
        [SerializeField] Camera mainCamera;
        [SerializeField] Camera gameCamera;
        public CinemachineVirtualCamera m_StageStartCamera;
        public CinemachineVirtualCamera m_BackgroundCamera;
        public CinemachineVirtualCamera m_StageEndCamera;


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

        public UfoManagerData UfoManager => m_GameManagerData.m_UfoManager;
        public PowerupManagerData PowerupManager => m_GameManagerData.m_PowerupManager;
        public UIManagerData UiManager => m_GameManagerData.m_UiManager;

        #endregion

        #region fields
        internal PlayerShipController m_playerShip;
        internal CamBounds m_camBounds;
        internal bool m_gamePlaying;
        internal bool m_gamePaused;
        internal CurrentLevel m_level;
        internal DebugSettings m_debug = new();
        internal bool m_StageStartCameraActive;

        bool _requestTitleScreen;

        EffectsManager _effects;
        #endregion


        #region unity events
        void Awake()
        {
            SingletonInstanceGuard();

            TryGetComponent(out _effects);
            m_camBounds = new CamBounds(gameCamera);
            UiManager.InitUI(uiAudioSource);

            m_StageStartCamera.m_Transitions.m_OnCameraLive.AddListener(OnCameraActivatedEventHandler);
        }

        void Start()
        {
            Camera.SetupCurrent(gameCamera);

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

                    yield return Run<string>(UiManager.ShowMainMenu(), (output) => result = output);
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
            m_playerShip = m_GameManagerData.CreatePlayer();
            m_level.Level1();
            m_gamePlaying = true;

            _requestTitleScreen = true;

            StartCoroutine(UfoManager.UfoSpawnLoop());
            StartCoroutine(PowerupManager.PowerupSpawnLoop());
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
            UiManager.LevelStarts(m_level.Level);

            if (m_level.Level == 1)
            {
                while (UiManager.AudioPlaying)
                    yield return null;

                m_playerShip.Spawn();
                yield return Wait(2);

                m_HudManager.HudShow();
                m_playerShip.EnableControls();
            }
            yield return Wait(1.5f);

            m_playerShip.Refuel();
            m_GameManagerData.SpawnAsteroids(m_level.AstroidsForLevel);
        }

        IEnumerator LevelPlay()
        {
            UiManager.LevelPlay();

            while (m_playerShip.m_isAlive && m_level.HasEnemy || m_debug.NoAstroids)
                yield return null;
        }

        IEnumerator LevelEnd()
        {
            bool gameover = !m_playerShip.m_isAlive;

            if (gameover)
            {
                m_gamePaused = true;

                StartCoroutine(UiManager.GameOver());
                StartCoroutine(RemoveRemainingObjects());

                while (UiManager.AudioPlaying)
                    yield return null;

                yield return Wait(1f);

                GameStart();
            }
            else
            {
                StartCoroutine(UiManager.LevelCleared(m_level.Level));

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

        public void MenuSelect(int i)
        {
            var menu = (Menu)i;
            switch (menu)
            {
                case Menu.start:
                    UiManager.HideMainMenu();
                    StartCoroutine(ResumeGame());
                    break;

                case Menu.exit:
                    m_gamePlaying = false;
                    var id = UiManager.HideMainMenu(false);
                    var d = LeanTween.descr(id);

                    d?.setOnComplete(QuitGame);
                    break;

                case Menu.none:
                default:
                    break;
            }
        }

        public void PlayEffect(Effect effect, Vector3 position, float scale = 1f, OjectLayer layer = OjectLayer.Game)
            => _effects.StartEffect(effect, position, scale, layer);

        #region Hyperjump
        internal AlliedShipController HyperJump(float duration) => m_GameManagerData.HyperJump(duration);

        /// <summary>
        /// Activate jump-crosshair
        /// </summary>
        internal void JumpSelect(Vector3 pos, float timer)
        {
            jumpController.gameObject.SetActive(true);
            jumpController.StartCountdown(pos, timer);
        }

        /// <summary>
        /// Deactivate jump-crosshair
        /// </summary>
        internal void JumpDeactivate() => jumpController.gameObject.SetActive(false);

        /// <summary>
        /// Return selected jump-position
        /// </summary>
        internal Vector3 JumpPosition() => jumpController.m_JumpPosition;

        internal bool JumpLaunched() => jumpController.m_Launched;

        internal Vector3 GetWorldJumpPosition()
        {
            var pos = JumpPosition();
            pos.z = 0;

            var gameToWorld = gameCamera.ViewportToScreenPoint(pos);
            gameToWorld.z = 0;
            gameToWorld.x *= 1.85f;
            gameToWorld.y *= 2;

            return mainCamera.ScreenToViewportPoint(gameToWorld);
        }

        #endregion

        public void SwitchStageCam(StageCamera camera)
        {
            switch (camera)
            {
                case StageCamera.start:
                    m_StageStartCamera.Priority = 100;
                    m_BackgroundCamera.Priority = 1;
                    m_StageEndCamera.Priority = 1;
                    break;
                case StageCamera.background:
                    m_BackgroundCamera.Priority = 100;
                    m_StageStartCamera.Priority = 1;
                    m_StageEndCamera.Priority = 1;
                    break;
                case StageCamera.end:
                    m_StageEndCamera.Priority = 100;
                    m_StageStartCamera.Priority = 1;
                    m_BackgroundCamera.Priority = 1;
                    break;
            }
        }

        void OnCameraActivatedEventHandler(ICinemachineCamera toCamera, ICinemachineCamera fromCamera)
        {
            m_StageStartCameraActive = toCamera.Equals(m_StageStartCamera);
        }

        /// <summary>
        /// Destroy player with explosion
        /// </summary>
        public void PlayerDestroyed()
        {
            if (m_debug.IsGodMode)
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

        internal class DebugSettings
        {
            internal bool IsGodMode
            {
                get => (__isGodMode ? 1 : 0) * (IsActive ? 1 : 0) > 0;
                set => __isGodMode = value;
            }
            bool __isGodMode;

            internal bool NoAstroids
            {
                get => (__noAstroids ? 1 : 0) * (IsActive ? 1 : 0) > 0;
                set => __noAstroids = value;
            }
            bool __noAstroids;

            internal bool NoUfos
            {
                get => (__noUfos ? 1 : 0) * (IsActive ? 1 : 0) > 0;
                set => __noUfos = value;
            }
            bool __noUfos;

            internal bool NoPowerups
            {
                get => (__noPowerups ? 1 : 0) * (IsActive ? 1 : 0) > 0;
                set => __noPowerups = value;
            }
            bool __noPowerups;

            internal bool OverrideMusic;
            internal MusicLevel Level;

            internal bool IsActive { get; set; }

            internal void SetMusic(int value)
            {
                print("SetMusic: " + value);
                if (value == 0)
                    OverrideMusic = false;
                else
                {
                    OverrideMusic = true;
                    Level = (MusicLevel)value - 1;
                }
            }
        }
    }
}