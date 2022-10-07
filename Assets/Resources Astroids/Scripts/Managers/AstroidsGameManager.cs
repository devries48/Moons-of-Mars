using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;

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
        [Header("Scriptables")]
        public UfoManager m_UfoManager;
        public PowerupManager m_PowerupManager;
        public UIManager m_UiManager;

        [Header("Prefabs")]
        [SerializeField, Tooltip("Select a spaceship prefab")]
        GameObject playerShipPrefab;

        [SerializeField, Tooltip("Select an astroid prefab")]
        GameObject asteroidPrefab;

        [Header("UI Elements")]
        public GameObject m_MainMenuWindow;
        public TextMeshProUGUI m_ScoreTextUI;
        public TextMeshProUGUI m_AnnouncerTextUI;

        [Header("Other")]
        [SerializeField] Camera gameCamera;
        [SerializeField] AudioMixer audioMixer;
        public AudioSource m_AudioSource;
        #endregion

        #region fields
        internal PlayerShipController m_playerShip;
        internal CamBounds m_camBounds;
        internal bool m_gamePlaying;
        internal bool m_gamePaused;
        internal CurrentLevel m_level;

        bool _requestTitleScreen;

        GameObjectPool _astoidPool;
        EffectsManager _effects;
        #endregion

        enum Menu { none = 0, start = 1, exit = 2 }

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
            m_UiManager.ResetUI();
        }

        void Start()
        {
            Camera.SetupCurrent(gameCamera);

            if (_astoidPool == null)
                return;

            m_gamePlaying = true;

            StartCoroutine(GameLoop());
            StartCoroutine(m_UfoManager.UfoSpawnLoop());
            StartCoroutine(m_PowerupManager.PowerupSpawnLoop());
        }

        void OnEnable() => __instance = this;
        #endregion

        #region game loops
        IEnumerator GameLoop()
        {
            yield return Wait(1);

            GameStart();

            while (m_gamePlaying)
            {
                if (_requestTitleScreen)
                {
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

        void GameStart()
        {
            m_playerShip = SpawnPlayer(playerShipPrefab);
            _requestTitleScreen = true;
            m_level.Level1();
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
            m_playerShip.Recover();
            m_playerShip.EnableControls();

            yield return Wait(1);

            m_UiManager.LevelStarts(m_level.Level);

            yield return Wait(2);

            SpawnAsteroids(m_level.AstroidsForLevel);
        }

        IEnumerator LevelPlay()
        {
            m_UiManager.LevelPlay();

            while (m_playerShip.m_isAlive && m_level.HasEnemy)
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
                GameStart();
            }
            else
            {
                StartCoroutine(m_UiManager.LevelCleared(m_level.Level));
                AdvanceLevel();
            }
            yield return Wait(2);
        }

        void AdvanceLevel()
        {
            m_level.LevelAdvance();
        }

        IEnumerator RemoveRemainingObjects()
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
        PlayerShipController SpawnPlayer(GameObject spaceShip)
        {
            GameObject ship = Instantiate(spaceShip);
            ship.TryGetComponent(out PlayerShipController shipCtrl);

            return shipCtrl;
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
        /// spawnAudio.Play();
        /// AudioFadeOut(duration, 0f, 1f);
        /// </summary>
        public void AudioFadeOut(float duration, float targetVol, float startVol = -1)
        {
            StartCoroutine(FadeMixerGroup.StartFade(audioMixer, "Vol1", duration, targetVol, startVol));
        }

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
            m_level.AstroidRemove();
        }

        public void UfoDestroyed()
        {
            m_level.UfoRemove();
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

        public struct CurrentLevel
        {
            int _level;
            int _astroidsForLevel;
            int _asteroidsActive;
            int _ufosActive;
            int _ufosForLevel;

            public int Level => _level;
            public int AstroidsForLevel => _astroidsForLevel;
            public int AstroidsActive => _asteroidsActive;
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