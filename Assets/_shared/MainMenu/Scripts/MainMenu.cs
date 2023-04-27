using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using System.Collections.Generic;

namespace MoonsOfMars.Shared
{
    public class MainMenu : MonoBehaviour
    {
        public enum Theme { custom1, custom2, custom3 };
        public enum MoonsOfMars { solarSystem, asteroids };
        public enum MenuAction { play, solarSystem, asteroids, newGame, loadGame }

        [Header("Moons of Mars")]
        [SerializeField] MoonsOfMars _application;
        [SerializeField] bool _openOnStart = true;
        [SerializeField] float _openDelay;
        [SerializeField] TMP_Text _versionText;
        [SerializeField] GameObject _gamesMenu;
        [SerializeField] GameObject[] _title;

        [Header("Theme Settings")]
        [SerializeField] Theme _theme;
        [SerializeField] FlexibleUIData _themeController;

        [Header("Main Canvasses")]
        [SerializeField, Tooltip("The UI Panel parenting all sub menus")] GameObject _mainCanvas;
        [SerializeField] GameObject _settingsCanvas;
        [SerializeField] GameObject _highScoreCanvas;

        [Header("Panels")]
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

        [Header("PAUSE MENU")]
        [SerializeField] GameObject _pauseMenu;
        [SerializeField] GameObject _quitMenu;

        [Header("LOADING SCREEN")]
        public GameObject loadingMenu;
        public Slider loadBar;
        public GameObject loadText;
        public TMP_Text finishedLoadingText;
        public bool requireInputForNextScene = false;

        [Header("EVENTS")]
        public UnityEvent OnMenuPlay;
        public UnityEvent OnMenuExit;
        public UnityEvent OnMenuResumeGame;
        public UnityEvent OnMenuAbortGame;
        public UnityEvent<MenuAction> OnMenuAction;

        Animator _camAnimator;
        float _loadProgress;
        List<GameObject> _subMenuList;

        void Awake()
        {
            _camAnimator = transform.GetComponent<Animator>();
            _versionText.text = $"version {Application.version}.alpha";
            _subMenuList = new List<GameObject>();

            if (_pauseMenu) _pauseMenu.SetActive(false);

            if (playMenu) _subMenuList.Add(playMenu);
            if (exitMenu) _subMenuList.Add(exitMenu);
            if (_gamesMenu) _subMenuList.Add(_gamesMenu);
            if (_quitMenu) _subMenuList.Add(_quitMenu);

            SetThemeColors();
        }

        void Start()
        {
            if (_openOnStart)
                OpenMenu();
        }

        public void OpenMenu()
        {
            var rect = _mainCanvas.GetComponent<RectTransform>();
            var startScale = rect.localScale / 10;

            rect.localScale = startScale;

            SetMenuActive(true);
            ShowMenu();

            LeanTween.scale(rect, startScale * 10, 1.2f).setEaseOutBounce().setDelay(_openDelay);
        }

        public void CloseMenu()
        {
            var rect = _mainCanvas.GetComponent<RectTransform>();
            var targetScale = rect.localScale / 10; ;

            LeanTween.scale(rect, targetScale, .6f).setEaseInBack().
                setOnComplete(() =>
                {
                    SetMenuActive(false);
                    rect.localScale = targetScale * 10;
                });
        }

        void SetMenuActive(bool active)
        {
            firstMenu.SetActive(active);
            _mainCanvas.SetActive(active);
            _settingsCanvas.SetActive(active);
            _highScoreCanvas.SetActive(active);
        }

        public void SetPauseMenuActive(bool active)
        {
            _pauseMenu.SetActive(active);
            _settingsCanvas.SetActive(active);
        }

        public void OpenPauseMenu()
        {
            TweenUtil.SetPivot(_pauseMenu, new Vector2(.5f, -.1f));
            SetPauseMenuActive(true);
            TweenUtil.TweenPivot(_pauseMenu, new Vector2(.5f, .5f), null, LeanTweenType.easeOutBack, .5f);
        }

        public int ClosePauseMenu()
        {
            var id = TweenUtil.TweenPivot(_pauseMenu, new Vector2(.5f, 1.5f), null, LeanTweenType.easeInBack, .5f);
            var d = LeanTween.descr(id);

            return d.id;
        }

        public void ShowMenu()
        {
            _mainCanvas.SetActive(true);
        }

        public void HideMenu()
        {
            _mainCanvas.SetActive(false);
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

        public void ReturnToMainMenu()
        {
            DisableSubMenus();
            mainMenu.SetActive(true);
        }

        public void ReturnToPauseMenu()
        {
            DisableSubMenus();
            _pauseMenu.SetActive(true);
        }

        public void LoadScene(string scene)
        {
            if (scene != "")
                StartCoroutine(LoadAsynchronously(scene));
        }

        public void DisableSubMenus(GameObject activateMenu = null)
        {
            foreach (var item in _subMenuList)
                item.SetActive(activateMenu != null && activateMenu == item);
        }

        public void CameraToSettings()
        {
            DisableSubMenus();
            _camAnimator.SetFloat("Animate", 1);
        }

        public void CameraToHighScore()
        {
            DisableSubMenus();
            _camAnimator.SetFloat("Animate", 2);
        }

        public void CameraToDefault() => _camAnimator.SetFloat("Animate", 0);

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

        public void PlaySwoosh() => swooshSound.Play();

        public void AreYouSure() => DisableSubMenus(exitMenu);

        public void QuitCurrentGame() => DisableSubMenus(_quitMenu);

        public void GamesMenu() => DisableSubMenus(_gamesMenu);

        public void InvokeMenuActionEvent(string action)
        {
            DisableSubMenus();

            var val = (MenuAction)System.Enum.Parse(typeof(MenuAction), action);

            if (val == MenuAction.asteroids)
                LoadScene("Asteroids");
            else
                OnMenuAction?.Invoke(val);
        }

        public void InvokeMenuPlayEvent()
        {
            OnMenuPlay?.Invoke();
        }
        public void InvokeMenuResumeGameEvent()
        {
            DisableSubMenus();
            OnMenuResumeGame?.Invoke();
        }

        public void InvokeMenuAbortGameEvent()
        {
            DisableSubMenus();
            OnMenuAbortGame?.Invoke();
        }

        public void InvokeMenuExitEvent()
        {
            DisableSubMenus();
            OnMenuExit?.Invoke();
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
            _mainCanvas.SetActive(false);
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
