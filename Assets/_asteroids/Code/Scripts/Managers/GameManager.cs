using Cinemachine;
using MoonsOfMars.Shared;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MoonsOfMars.Game.Asteroids
{
    using static EffectsManager;
    using static Utils;

    [SelectionBase]
    [RequireComponent(typeof(EffectsManager))]
    public class GameManager : GameManagerBase<GameManager>
    {
        public static GameManager GmManager => _instance;

        //enum Menu { none = 0, start = 1, settings = 2, exit = 3 }
        public enum GameStatus { intro, start, menu, playing, paused, stageResults, gameover, exit }
        public enum StageCamera { start, far, background, end }

        #region editor fields
        [Header("Data")]
        public GameManagerData m_GameManagerData;

        [Header("Managers Asteroids")]
        //public AudioManager m_AudioManager;
        public HudManager m_HudManager;
        public LevelManager m_LevelManager;
        public LightsManager m_LightsManager;

        [Header("UI Elements")]
        public MainMenu m_MainMenu; // todo: move to base?
        public TextMeshProUGUI m_ScoreTextUI; // move to score manager
        //public TextMeshProUGUI m_AnnouncerTextUI;
        //public ParticleSystem m_SpaceDebriSystem;

        [Header("Camera's")]
        [SerializeField] Camera mainCamera;
        [SerializeField] Camera gameCamera;
        public CinemachineVirtualCamera m_StageStartCamera;
        public CinemachineVirtualCamera m_StageEndCamera;
        public CinemachineVirtualCamera m_BackgroundCamera;
        [SerializeField] CinemachineVirtualCamera backgroundFarCamera;

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

        public bool IsGameExit => _gameStatus == GameStatus.exit;
        public bool IsGamePlaying => _gameStatus == GameStatus.playing;
        public bool IsGameStageComplete => _gameStatus == GameStatus.stageResults;
        public bool IsGamePaused => _gameStatus == GameStatus.paused;
        public bool IsGameInMenu => _gameStatus == GameStatus.menu;
        public bool IsGameOver => _gameStatus == GameStatus.gameover;

        public AudioManager AudioManager => GetAudioManager<AudioManager>();
        public InputManager InputManager => GetInputManager<InputManager>();

        public UfoManagerData UfoManager => m_GameManagerData.m_UfoManager;
        public PowerupManagerData PowerupManager => m_GameManagerData.m_PowerupManager;
        public UIManagerData UiManager => m_GameManagerData.m_UiManager;
        #endregion

        #region fields
        internal PlayerShipController m_playerShip;
        internal CamBounds m_camBounds;
        internal DebugSettings m_debug = new();
        internal bool m_gameAborted;
        CinemachineBrain _cinemachineBrain;
        GameStatus _gameStatus;
        EffectsManager _effects; // TODO: EffectsManager to base class
        #endregion

        #region unity events
        protected override void Awake()
        {
            base.Awake();
            _cinemachineBrain = mainCamera.GetComponentInChildren<CinemachineBrain>();

            SwitchStageCam(StageCamera.start);
            TryGetComponent(out _effects);

            m_camBounds = new CamBounds(gameCamera);
        }

        void Start() => StartCoroutine(Initialize());

        void OnEnable() => RegisterCameras(m_StageStartCamera, m_BackgroundCamera, m_StageEndCamera, backgroundFarCamera);
        void OnDisable() => UnregisterCameras(m_StageStartCamera, m_BackgroundCamera, m_StageEndCamera, backgroundFarCamera);

        #endregion

        // Wait until Objectpool stage is created
        IEnumerator Initialize()
        {

            while (_effects.UseObjectPoolScene && !_effects.ObjectPoolSceneLoaded)
                yield return null;

            m_GameManagerData.Initialize();
            UiManager.Initialize(uiAudioSource);
            PowerupManager.Initialize();

            Camera.SetupCurrent(gameCamera);
            StartCoroutine(GameLoop());
        }

        #region game loop
        
        IEnumerator GameLoop()
        {
            m_LevelManager.ShowGameIntro();

            while (!IsGameExit)
            {
                while (!IsGamePlaying)
                {
                    if (_gameStatus == GameStatus.start)
                    {
                        m_gameAborted = false;

                        Score.Reset();
                        SetGameStatus(GameStatus.menu);
                        StartCoroutine(UiManager.ShowMainMenu());
                    }
                    yield return null;
                }

                yield return StartCoroutine(m_LevelManager.LevelStartLoop());
                yield return StartCoroutine(m_LevelManager.LevelPlayLoop());
                yield return StartCoroutine(m_LevelManager.LevelEndLoop());

                GC.Collect();
            }
        }

        public void GameStart()
        {
            m_playerShip = m_GameManagerData.CreatePlayer();
            m_LevelManager.StartLevel1();
            SetGameStatus(GameStatus.start);

            StartCoroutine(UfoManager.UfoSpawnLoop());
            StartCoroutine(PowerupManager.PowerupSpawnLoop());
        }

        public void GameOver()
        {
            StartCoroutine(m_LevelManager.AnnounceGameOver());

            if (Score.Earned == 0)
            {
                GameQuit();
                return;
            }
            SetGameStatus(GameStatus.gameover);
            m_LevelManager.ShowGameResults(true);
        }

        public void GameAbort()
        {
            m_gameAborted = true;
            UiManager.HidePauseMenu();
            m_HudManager.HudHide();
            GameQuit();
        }

        public void GameQuit()
        {
            StartCoroutine(RemoveRemainingObjects());
            StartCoroutine(StartNewGame());
            Score.Reset();
        }

        public void GamePause() => UiManager.ShowPauseMenu();

        public void GameResume()
        {
            UiManager.HidePauseMenu();
            SetGameStatus(GameStatus.playing);
        }

        public void StageStartNew() => StartCoroutine(StageStart());

        IEnumerator StageStart()
        {
            SwitchStageCam(StageCamera.far);
            yield return Wait(.1f);

            SwitchStageCam(StageCamera.background);
            yield return Wait(.5f);

            m_playerShip.ResetPosition();

            PlayEffect(Effect.Teleport, m_playerShip.transform.position, 1f, ObjectLayer.Effects);
            yield return Wait(.5f);

            m_playerShip.Teleport(true);
            m_HudManager.HudShow();
            AudioManager.FadeInBackgroundSfx();

            SetGameStatus(GameStatus.playing);
        }

        IEnumerator GamePlay(float delay)
        {
            yield return new WaitForSeconds(delay);
            SetGameStatus(GameStatus.playing);
        }
        #endregion


        /// <summary>
        /// Start the game (invoked by MainMenu event)
        /// </summary>
        public void MenuPlay()
        {
            UiManager.HideMainMenu();
            StartCoroutine(GamePlay(1.5f));
        }

        /// <summary>
        /// Quit the application (invoked by MainMenu event)
        /// </summary>
        public void MenuExit()
        {
            SetGameStatus(GameStatus.exit);

            UiManager.HideMainMenu(false);
            GameExit();
        }

        // TODO: Objectpool to base class (also integrate effectsmanager (scriptable object)
 

        public void CreateObjectPool(Action buildPoolAction) => StartCoroutine(AddObjectPoolCore(buildPoolAction));

        public void AddObjectToPoolScene(GameObject go) => SceneManager.MoveGameObjectToScene(go, _effects.ObjectPoolScene);

        IEnumerator AddObjectPoolCore(Action action)
        {
            while (_effects == null || _effects.UseObjectPoolScene && !_effects.ObjectPoolSceneLoaded)
                yield return null;

            action();
        }

        /// <summary>
        /// Creates an object pool in a separate designated scene. 
        /// </summary>
        public GameObjectPool CreateObjectPool(GameObject prefab, int initialCapacity, int maxCapacity = 1000)
            => GameObjectPool.Build(prefab, initialCapacity, maxCapacity, _effects.ObjectPoolScene);

        /// <summary>
        /// Creates an object pool in the active scene.
        /// </summary>
        public GameObjectPool CreateLocalObjectPool(GameObject prefab, int initialCapacity, int maxCapacity = 1000)
            => GameObjectPool.Build(prefab, initialCapacity, maxCapacity);

        public IEnumerator RemoveRemainingObjects()
        {
            foreach (var obj in FindObjectsOfType<GameBase>())
            {
                if (m_gameAborted || !obj.gameObject.CompareTag("Player"))
                    obj.RemoveFromGame();
            }

            yield return null;
        }

        public bool IsStageStartCameraActive()
            => _cinemachineBrain.ActiveVirtualCamera.Name == m_StageStartCamera.name;

        public void SetGameStatus(GameStatus status)
        {
            _gameStatus = status;
            print("STATUS: " + status.ToString().ToUpper());
        }

        public void PlayEffect(Effect effect, Vector3 position, float scale = 1f, ObjectLayer layer = ObjectLayer.Effects)
            => _effects.StartEffect(effect, position, scale, layer);

        #region Hyperjump
        public AlliedShipController HyperJump(float duration) => m_GameManagerData.HyperJumpAnimation(duration);

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

        public Vector3 GetWorldPlayerPosition()
        {
            var pos = m_playerShip.transform.position;
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
//            backgroundFarCamera.Priority = 1;

            switch (camera)
            {
                case StageCamera.start:
                    SwitchCamera(m_StageStartCamera);
                    //m_StageStartCamera.Priority = 100;
                    //m_BackgroundCamera.Priority = 1;
                    //m_StageEndCamera.Priority = 1;
                    break;
                case StageCamera.far:
                    SwitchCamera(backgroundFarCamera);
                    //backgroundFarCamera.Priority = 100;
                    //m_StageStartCamera.Priority = 1;
                    break;
                case StageCamera.background:
                    SwitchCamera(m_BackgroundCamera);
                    //m_BackgroundCamera.Priority = 100;
                    //m_StageStartCamera.Priority = 1;
                    //m_StageEndCamera.Priority = 1;
                    break;
                case StageCamera.end:
                    SwitchCamera(m_StageEndCamera);
                    //m_StageEndCamera.Priority = 100;
                    //m_StageStartCamera.Priority = 1;
                    //m_BackgroundCamera.Priority = 1;
                    break;
            }
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
            }
        }

        IEnumerator StartNewGame()
        {
            while (UiManager.AudioPlaying)
                yield return null;

            yield return Wait(1f);

            GameStart();
        }

        public static WaitForSeconds Wait(float duration) => new(duration);

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

    }
}