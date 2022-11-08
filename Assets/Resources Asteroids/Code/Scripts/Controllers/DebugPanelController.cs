using System;
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

        [Header("UI Elements")]
        [SerializeField] GameObject debugPanel;
        [SerializeField] GameObject godModeToggle;
        [SerializeField] GameObject astroidToggle;
        [SerializeField] GameObject ufoToggle;
        [SerializeField] GameObject powerupToggle;
        [SerializeField] TMPro.TextMeshProUGUI version;
        [SerializeField] TMPro.TextMeshProUGUI astroidsCount;
        [SerializeField] TMPro.TextMeshProUGUI ufoCount;
        [SerializeField] Button closeButton;

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
            _toggleGodMode = godModeToggle.GetComponent<Toggle>();

            _toggleSpawnAstroids.isOn = spawnAstroids;
            _toggleSpawnUfos.isOn = spawnUfos;
            _toggleGodMode.isOn = godModeOn;

            GameManager.m_debug_godMode = _toggleGodMode.isOn;

            if (debugOn)
            {
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
            GameManager.m_PowerupManager.ShuttleLaunch();
        }

        public void ToggleUfoChanged()
        {
            GameManager.m_debug_no_ufos = !_toggleSpawnUfos.isOn;
        }

        public void ToggleAstroidChanged()
        {
            GameManager.m_debug_no_astroids = !_toggleSpawnAstroids.isOn;
        }

        public void ToggleGodModeChanged()
        {
            GameManager.m_debug_godMode = _toggleGodMode.isOn;
        }

        void UpdatePanel()
        {
            astroidsCount.text = GameManager.m_level.AstroidsActive.ToString();
            ufoCount.text = GameManager.m_level.UfosActive.ToString();
        }

    }
}