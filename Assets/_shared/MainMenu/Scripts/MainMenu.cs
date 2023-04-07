using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using System;

namespace MoonsOfMars.Shared
{
    public class MainMenu : MonoBehaviour
    {
        public enum Theme { custom1, custom2, custom3 };
        public enum MenuAction { play, solarSystem, asteroids, newGame, loadGame }

        [Header("Moons of Mars")]
        [SerializeField] TMP_Text _versionText;
        [SerializeField] GameObject _title;
        [SerializeField] GameObject _gamesMenu;
        [SerializeField] GameObject[] _mainMenuItems;

        [Header("Theme Settings")]
        [SerializeField] Theme _theme;
        [SerializeField] FlexibleUIData _themeController;

        [Header("Panels")]
        [Tooltip("The UI Panel parenting all sub menus")] public GameObject mainCanvas;
        [SerializeField, Tooltip("The UI Panel that holds the CONTROLS window tab")] GameObject _panelControls;
        [SerializeField, Tooltip("The UI Panel that holds the VIDEO window tab")] GameObject _panelVideo;
        [SerializeField, Tooltip("The UI Panel that holds the GENERAL window tab")] GameObject _panelGeneral;
        [SerializeField, Tooltip("The UI Panel that holds the AUDIO window tab")] GameObject _panelAudio;
        [SerializeField, Tooltip("The UI Panel that holds the KEY BINDINGS window tab")] GameObject _panelKeyBindings;
        [SerializeField, Tooltip("The UI Sub-Panel under KEY BINDINGS for MOVEMENT")] GameObject _panelBindMovement;
        [SerializeField, Tooltip("The UI Sub-Panel under KEY BINDINGS for COMBAT")] GameObject _panelBindCombat;
        [SerializeField, Tooltip("The UI Sub-Panel under KEY BINDINGS for GENERAL")] GameObject _panelBindGeneral;


        [Header("SFX")]
        [Tooltip("The GameObject holding the Audio Source component for the HOVER SOUND")]
        public AudioSource hoverSound;
        [Tooltip("The GameObject holding the Audio Source component for the AUDIO SLIDER")]
        public AudioSource sliderSound;
        [Tooltip("The GameObject holding the Audio Source component for the SWOOSH SOUND when switching to the Settings Screen")]
        public AudioSource swooshSound;

        // campaign button sub menu
        [Header("Menus")]
        [Tooltip("The Menu for when the MAIN menu buttons")]
        public GameObject mainMenu;
        [Tooltip("THe first list of buttons")]
        public GameObject firstMenu;
        [Tooltip("The Menu for when the PLAY button is clicked")]
        public GameObject playMenu;
        [Tooltip("The Menu for when the EXIT button is clicked")]
        public GameObject exitMenu;

        // highlights
        [Header("Highlight Effects")]
        [SerializeField, Tooltip("Highlight Image for when GENERAL Tab is selected in Settings")] GameObject _lineGeneral;
        [SerializeField, Tooltip("Highlight Image for when AUDIO Tab is selected in Settings")] GameObject _lineAudio;
        [Tooltip("Highlight Image for when VIDEO Tab is selected in Settings")]
        public GameObject lineVideo;
        [Tooltip("Highlight Image for when CONTROLS Tab is selected in Settings")]
        public GameObject lineControls;
        [Tooltip("Highlight Image for when KEY BINDINGS Tab is selected in Settings")]
        public GameObject lineKeyBindings;
        [Tooltip("Highlight Image for when MOVEMENT Sub-Tab is selected in KEY BINDINGS")]
        public GameObject lineMovement;
        [Tooltip("Highlight Image for when COMBAT Sub-Tab is selected in KEY BINDINGS")]
        public GameObject lineCombat;
        [Tooltip("Highlight Image for when GENERAL Sub-Tab is selected in KEY BINDINGS")]
        public GameObject lineGeneral;

        [Header("LOADING SCREEN")]
        public GameObject loadingMenu;
        public Slider loadBar;
        public GameObject loadText;
        public TMP_Text finishedLoadingText;
        public bool requireInputForNextScene = false;

        [Header("EVENTS")]
        public UnityEvent<MenuAction> OnMenuAction;

        Animator CameraObject;
        float _loadProgress;

        void Awake()
        {
            CameraObject = transform.GetComponent<Animator>();
            _versionText.text = $"version {Application.version}.alpha";

            if (playMenu) playMenu.SetActive(false);
            exitMenu.SetActive(false);
            if (_gamesMenu) _gamesMenu.SetActive(false);

            SetThemeColors();

            firstMenu.SetActive(true);
            mainMenu.SetActive(true);
        }

        public void ShowMenu()
        {
            mainCanvas.SetActive(true);
        }

        public void HideMenu()
        {
            mainCanvas.SetActive(false);
        }

        #region Theme
        public void ThemeChanged(Theme theme)
        {
            if (theme != _theme)
            {
                _theme = theme;
                SetThemeColors();
            }
        }

        void SetThemeColors()
        {
            if (_theme == Theme.custom1)
            {
                _themeController.currentColor = _themeController.custom1.graphic1;
                _themeController.textColor = _themeController.custom1.text1;
            }
            else if (_theme == Theme.custom2)
            {
                _themeController.currentColor = _themeController.custom2.graphic2;
                _themeController.textColor = _themeController.custom2.text2;
            }
            else if (_theme == Theme.custom3)
            {
                _themeController.currentColor = _themeController.custom3.graphic3;
                _themeController.textColor = _themeController.custom3.text3;
            }
        }

        #endregion

        public void PlayCampaign()
        {
            exitMenu.SetActive(false);
            if (_gamesMenu) _gamesMenu.SetActive(false);
            if (playMenu) playMenu.SetActive(true);
        }

        public void ReturnMenu()
        {
            if (playMenu) playMenu.SetActive(false);
            if (_gamesMenu) _gamesMenu.SetActive(false);
            exitMenu.SetActive(false);
            mainMenu.SetActive(true);
        }

        public void LoadScene(string scene)
        {
            if (scene != "")
            {
                StartCoroutine(LoadAsynchronously(scene));
            }
        }

        public void DisablePlayCampaign()
        {
            if (playMenu) playMenu.SetActive(false);
        }

        public void Position2()
        {
            DisablePlayCampaign();
            CameraObject.SetFloat("Animate", 1);
        }

        public void Position1()
        {
            CameraObject.SetFloat("Animate", 0);
        }


        #region Panels
        public void ShowPanelGeneral()
        {
            DisablePanels();
            _panelGeneral.SetActive(true);
            _lineGeneral.SetActive(true);
        }

        public void ShowPanelVideo()
        {
            DisablePanels();
            _panelVideo.SetActive(true);
            lineVideo.SetActive(true);
        }

        public void ShowPanelAudio()
        {
            DisablePanels();
            _panelAudio.SetActive(true);
            _lineAudio.SetActive(true);
        }

        public void ShowPanelControls()
        {
            DisablePanels();
            _panelControls.SetActive(true);
            lineControls.SetActive(true);
        }

        public void ShowPanelKeyBindings()
        {
            DisablePanels();
            ShowBindPanelMovement();
            _panelKeyBindings.SetActive(true);
            lineKeyBindings.SetActive(true);
        }

        public void ShowBindPanelMovement()
        {
            DisablePanels();
            _panelKeyBindings.SetActive(true);
            _panelBindMovement.SetActive(true);
            lineMovement.SetActive(true);
        }

        public void ShowBindPanelCombat()
        {
            DisablePanels();
            _panelKeyBindings.SetActive(true);
            _panelBindCombat.SetActive(true);
            lineCombat.SetActive(true);
        }

        public void ShowBindPanelGeneral()
        {
            DisablePanels();
            _panelKeyBindings.SetActive(true);
            _panelBindGeneral.SetActive(true);
            lineGeneral.SetActive(true);
        }

        void DisablePanels()
        {
            _panelControls.SetActive(false);
            _panelVideo.SetActive(false);
            _panelGeneral.SetActive(false);
            _panelAudio.SetActive(false);
            _panelKeyBindings.SetActive(false);

            _lineGeneral.SetActive(false);
            lineControls.SetActive(false);
            lineVideo.SetActive(false);
            _lineAudio.SetActive(false);
            lineKeyBindings.SetActive(false);

            _panelBindMovement.SetActive(false);
            lineMovement.SetActive(false);
            _panelBindCombat.SetActive(false);
            lineCombat.SetActive(false);
            _panelBindGeneral.SetActive(false);
            lineGeneral.SetActive(false);
        }
        #endregion

        public void PlayHover()
        {
            hoverSound.Play();
        }

        public void PlaySFXHover()
        {
            sliderSound.Play();
        }

        public void PlaySwoosh()
        {
            swooshSound.Play();
        }

        // Are You Sure - Quit Panel Pop Up
        public void AreYouSure()
        {
            exitMenu.SetActive(true);
            if (_gamesMenu) _gamesMenu.SetActive(false);
            DisablePlayCampaign();
        }

        public void GamesMenu()
        {
            if (playMenu) playMenu.SetActive(false);
            if (_gamesMenu) _gamesMenu.SetActive(true);
            exitMenu.SetActive(false);
        }

        public void InvokeMenuActionEvent(string action)
        {
            var val = (MenuAction)System.Enum.Parse(typeof(MenuAction), action);

            if (val == MenuAction.asteroids)
                LoadScene("Asteroids");
            else
                OnMenuAction?.Invoke(val);
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
				Application.Quit();
#endif
        }

        IEnumerator LoadAsynchronously(string sceneName)
        {
            mainCanvas.SetActive(false);
            loadingMenu.SetActive(true);
            loadText.SetActive(true);
            finishedLoadingText.gameObject.SetActive(false);

            _loadProgress = 0;
            loadBar.value = 0;

            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
            operation.allowSceneActivation = false;

            while (_loadProgress < 1f)
            {
                _loadProgress = Mathf.Clamp01(operation.progress / .9f);
                yield return null;
            }

            while (!Mathf.Approximately(loadBar.value, 1f))
            {
                yield return null;
            }

            if (requireInputForNextScene)
            {
                finishedLoadingText.gameObject.SetActive(true);
                loadText.SetActive(false);

                while (!operation.isDone)
                {
                    if (Input.anyKeyDown)
                        operation.allowSceneActivation = true;

                    yield return null;
                }
            }
            else
                operation.allowSceneActivation = true;
        }

        void Update()
        {
            if (loadBar.gameObject.activeInHierarchy)
            {
                loadBar.value = Mathf.MoveTowards(loadBar.value, _loadProgress, 3 * Time.deltaTime);
                print(loadBar.value);
            }
        }

    }
}
