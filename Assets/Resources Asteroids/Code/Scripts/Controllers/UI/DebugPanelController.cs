using UnityEngine;
using UnityEngine.UI;

namespace Game.Astroids
{
    public class DebugPanelController : MonoBehaviour
    {
        // SINGLETON
        public static DebugPanelController instance;

        [SerializeField] bool debugOn;
        [SerializeField] bool godModeOn;
        [SerializeField] bool spawnAstroids = true;
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

        AsteroidsGameManager GameManager
        {
            get
            {
                if (__gameManager == null)
                    __gameManager = AsteroidsGameManager.Instance;

                return __gameManager;
            }
        }
        AsteroidsGameManager __gameManager;

        const float REFRESH_TIME = 1 / 30f;

        Toggle _toggleGodMode;
        Toggle _toggleSpawnAstroids;
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

            _toggleSpawnAstroids = astroidToggle.GetComponent<Toggle>();
            _toggleSpawnUfos = ufoToggle.GetComponent<Toggle>();
            _toggleSpawnPowerup = powerupToggle.GetComponent<Toggle>();
            _toggleGodMode = godModeToggle.GetComponent<Toggle>();

            _toggleSpawnAstroids.isOn = spawnAstroids;
            _toggleSpawnUfos.isOn = spawnUfos;
            _toggleSpawnPowerup.isOn = spawnPowerups;
            _toggleGodMode.isOn = godModeOn;

            if (debugOn)
            {
                GameManager.m_debug.IsActive = true;
                GameManager.m_debug.IsGodMode = godModeOn;
                GameManager.m_debug.NoAstroids = !spawnAstroids;
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

        public void SpawnPowerupClick()
        {
            GameManager.PowerupManager.ShuttleLaunch();
        }

        public void SpawnUfoClick()
        {
            GameManager.UfoManager.UfoLaunch();
        }

        public void ResetClick()
        {
            StartCoroutine(GameManager.RemoveRemainingObjects());
            GameManager.GameStart();
        }

        public void ToggleUfoChanged()
        {
            GameManager.m_debug.NoUfos = !_toggleSpawnUfos.isOn;
        }

        public void ToggleAstroidChanged()
        {
            GameManager.m_debug.NoAstroids = !_toggleSpawnAstroids.isOn;
        }

        public void TogglePowerupChanged()
        {
            GameManager.m_debug.NoPowerups = !_toggleSpawnPowerup.isOn;
        }

        public void ToggleGodModeChanged()
        {
            GameManager.m_debug.IsGodMode = _toggleGodMode.isOn;
        }

        public void MusicDropdown(int value)
        {

        }

        public void JumpClick()
        {
            GameManager.m_playerShip.Jump();
        }

        public void StageEndClick()
        {

            GameManager.m_GameManagerData.StageCompleteAnimation();
        }

        void UpdatePanel()
        {
            astroidsCount.text = GameManager.m_LevelManager.AstroidsActive.ToString();
            ufoCount.text = GameManager.m_LevelManager.UfosActive.ToString();
        }

    }
}