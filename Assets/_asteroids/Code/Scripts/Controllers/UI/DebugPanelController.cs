using UnityEngine;
using UnityEngine.UI;
using static MusicData;

namespace MoonsOfMars.Game.Asteroids
{
    public class DebugPanelController : MonoBehaviour
    {
        // SINGLETON
        public static DebugPanelController instance;

        [SerializeField] bool debugOn;
        [SerializeField] bool godModeOn;
        [SerializeField] bool spawnAsteroids = true;
        [SerializeField] bool spawnUfos = true;
        [SerializeField] bool spawnPowerups = true;

        [Header("UI Elements")]
        [SerializeField] GameObject debugPanel;
        [SerializeField] GameObject godModeToggle;
        [SerializeField] GameObject astroidToggle;
        [SerializeField] GameObject ufoToggle;
        [SerializeField] GameObject powerupToggle;
        [SerializeField] TMPro.TextMeshProUGUI version;
        [SerializeField] TMPro.TextMeshProUGUI astroidsCount;
        [SerializeField] TMPro.TextMeshProUGUI ufoCount;
        [SerializeField] TMPro.TextMeshProUGUI powerupCount;

        AsteroidsGameManager GameManager
        {
            get
            {
                if (__gameManager == null)
                    __gameManager = AsteroidsGameManager.GmManager;

                return __gameManager;
            }
        }
        AsteroidsGameManager __gameManager;

        const float REFRESH_TIME = 1 / 30f;

        Toggle _toggleGodMode;
        Toggle _toggleSpawnAsteroids;
        Toggle _toggleSpawnUfos;
        Toggle _toggleSpawnPowerup;

        void Awake()
        {
            if (instance == null)
                instance = this;
        }

        void Start()
        {
            version.text = Application.version + ".alpha";

            _toggleSpawnAsteroids = astroidToggle.GetComponent<Toggle>();
            _toggleSpawnUfos = ufoToggle.GetComponent<Toggle>();
            _toggleSpawnPowerup = powerupToggle.GetComponent<Toggle>();
            _toggleGodMode = godModeToggle.GetComponent<Toggle>();

            _toggleSpawnAsteroids.isOn = spawnAsteroids;
            _toggleSpawnUfos.isOn = spawnUfos;
            _toggleSpawnPowerup.isOn = spawnPowerups;
            _toggleGodMode.isOn = godModeOn;

            if (debugOn)
            {
                print("Debug is on");
                GameManager.m_debug.IsActive = true;
                GameManager.m_debug.IsGodMode = godModeOn;
                GameManager.m_debug.NoAsteroids = !spawnAsteroids;
                GameManager.m_debug.NoUfos = !spawnUfos;
                GameManager.m_debug.NoPowerups = !spawnPowerups;

                debugPanel.SetActive(true);
                InvokeRepeating(nameof(UpdatePanel), REFRESH_TIME, REFRESH_TIME);
            }
            else
                debugPanel.SetActive(false);
        }

        public void ClosePanelClick()
        {
            CancelInvoke();
            debugPanel.SetActive(false);
        }

        public void SpawnPowerupClick() => GameManager.PowerupManager.ShuttleLaunch();

        public void SpawnUfoClick() => GameManager.UfoManager.UfoLaunch();

        public void ResetClick()
        {
            StartCoroutine(GameManager.RemoveRemainingObjects());
            GameManager.GameStart();
        }

        public void ToggleUfoChanged() => GameManager.m_debug.NoUfos = !_toggleSpawnUfos.isOn;

        public void ToggleAstroidChanged() => GameManager.m_debug.NoAsteroids = !_toggleSpawnAsteroids.isOn;

        public void TogglePowerupChanged() => GameManager.m_debug.NoPowerups = !_toggleSpawnPowerup.isOn;

        public void ToggleGodModeChanged() => GameManager.m_debug.IsGodMode = _toggleGodMode.isOn;

        public void MusicDropdown(int value)
        {

        }

        public void JumpClick() => GameManager.m_playerShip.Jump();

        public void StageEndClick() => GameManager.m_GameManagerData.StageCompleteAnimation();

        void UpdatePanel()
        {
            astroidsCount.text = GameManager.m_LevelManager.AsteroidsActive.ToString();
            ufoCount.text = GameManager.m_LevelManager.UfosActive.ToString();
            powerupCount.text = GameManager.m_LevelManager.GetStageResults()?.PowerupsPickedUp.ToString() ?? "0";
        }
    }

    public class DebugSettings
    {
        public bool IsActive { get; set; }

        public bool IsGodMode
        {
            get => (__isGodMode ? 1 : 0) * (IsActive ? 1 : 0) > 0;
            set => __isGodMode = value;
        }
        bool __isGodMode;

        public bool NoAsteroids
        {
            get => (__noAsteroids ? 1 : 0) * (IsActive ? 1 : 0) > 0;
            set => __noAsteroids = value;
        }
        bool __noAsteroids;

        public bool NoUfos
        {
            get => (__noUfos ? 1 : 0) * (IsActive ? 1 : 0) > 0;
            set => __noUfos = value;
        }
        bool __noUfos;

        public bool NoPowerups
        {
            get => (__noPowerups ? 1 : 0) * (IsActive ? 1 : 0) > 0;
            set => __noPowerups = value;
        }
        bool __noPowerups;

        internal bool OverrideMusic;
        internal MusicLevel Level;

        internal void SetMusic(int value)
        {
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