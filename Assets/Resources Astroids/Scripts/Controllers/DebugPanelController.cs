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
        [SerializeField] GameObject debugPanel;
        [SerializeField] GameObject godModeToggle;
        [SerializeField] GameObject astroidToggle;
        [SerializeField] GameObject ufoToggle;
        [SerializeField] GameObject powerupToggle;
        [SerializeField] TMPro.TextMeshProUGUI version;
        [SerializeField] TMPro.TextMeshProUGUI astroidsCount;
        [SerializeField] TMPro.TextMeshProUGUI ufoCount;
        [SerializeField] Button closeButton;

        AstroidsGameManager GameManager
        {
            get
            {
                if (__gameManager == null)
                    __gameManager = AstroidsGameManager.Instance;

                return __gameManager;
            }
        }
        AstroidsGameManager __gameManager;

        const float REFRESH_TIME = 1 / 30f;

        Toggle _toggleGodMode;

        void Awake()
        {
            if (instance == null)
                instance = this; 
      }

        void Start()
        {
            version.text = Application.version + ".alpha";

            _toggleGodMode = godModeToggle.GetComponent<Toggle>();
            _toggleGodMode.isOn = godModeOn;

            GameManager.m_godMode = _toggleGodMode.isOn;

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

        public void SpawnUfoClick()
        {

        }

        public void ToggleGodModeChanged()
        {
            GameManager.m_godMode = _toggleGodMode.isOn;
        }

        void UpdatePanel()
        {
            astroidsCount.text = GameManager.m_level.AstroidsActive.ToString();
            ufoCount.text = GameManager.m_level.UfosActive.ToString();
        }

    }
}