using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Events;

using static SolarSystemController;
using Cinemachine;
using System;

// see https://easings.net/

[DisallowMultipleComponent]
public class MenuController : MonoBehaviour
{
    #region editor fields
    [Header("UI Elements")]
    [SerializeField] GameObject mainMenuWindow;
    [SerializeField] GameObject gamesMenuWindow;
    [SerializeField] GameObject infoPanel;
    [SerializeField] GameObject exitButton;
    [SerializeField] ParticleSystem spaceDebriSystem;

    [Header("Sound")]
    [SerializeField] AudioSource slideInSound;

    [Header("Controllers")]
    [SerializeField] GameObject solarSystem;
    #endregion

    #region fields
    PlayableDirector _director;
    #endregion

    #region properties
    GameManager GmManager => GameManager.Instance;

    SolarSystemPanelController SystemPanelController
    {
        get
        {
            if (_solarSystemControlPanel == null)
                TryGetComponent(out _solarSystemControlPanel);

            return _solarSystemControlPanel;
        }
    }
    SolarSystemPanelController _solarSystemControlPanel;
    #endregion

    void OnEnable()
    {
        CameraSwitcher.Register(GmManager.MenuCamera);
        CameraSwitcher.Register(GmManager.SolarSystemCamera);
        SystemPanelController.HideControlPanel();
    }

    void OnDisable()
    {
        CameraSwitcher.Unregister(GmManager.MenuCamera);
        CameraSwitcher.Unregister(GmManager.SolarSystemCamera);
    }

    void Awake()
    {
        _director = GetComponent<PlayableDirector>();
        _director.played += Director_played;
        _director.stopped += Director_stopped;
    }

    void Start()
    {
        ShowMainMenu();
        gamesMenuWindow.SetActive(false);
    }

    public void ShowBodyInfoWindow(CelestialBodyName name, bool isDeselect)
    {
        if (isDeselect)
            TweenPivot(infoPanel, new Vector2(0f, 0.5f), null, LeanTweenType.easeInOutBack);
        else
        {
            SetWindowInfo(name);
            slideInSound.Play();
            TweenPivot(infoPanel, new Vector2(1.2f, 0.5f), null, LeanTweenType.easeInOutBack);
        }
    }

    public void MenuStartTour()
    {
        _director.Play();
    }

    // Setup environment for the Solar System viewer
    public void MenuSolarSytem()
    {
        HideMainMenu();

        SystemPanelController.ShowControlPanel();

        StartCoroutine(DelayExecute(GmManager.CameraSwitchTime, GmManager.SolarSystemCtrl.ShowOrbitLines));

        if (GmManager.SolarSystemCtrl.GetPlanetScaleMultiplier() == 1)
            TweenPlanetScale(GmManager.CameraSwitchTime);

        CameraSwitcher.SwitchCamera(GmManager.SolarSystemCamera);
    }

    public void MenuGames()
    {
        gamesMenuWindow.SetActive(true);

        CloseMenuWindow(mainMenuWindow);
        OpenMenuWindow(gamesMenuWindow);
    }

    public void MenuQuit()
    {
        HideMainMenu(true);
    }

    public void ExitToMainMenu()
    {
        var wait = 0f;

        if (GmManager.SolarSystemCtrl.OrbitLinesVisible)
        {
            wait = .5f; // Wait for orbit lines to fade away
            GmManager.SolarSystemCtrl.HideOrbitLines();
            SystemPanelController.HideControlPanel(true);
        }

        if (_director.state == PlayState.Playing)
            _director.Stop();
        else if (wait > 0)
            StartCoroutine(DelayExecute(wait, ShowMainMenu));
        else
            ShowMainMenu();
    }

    public void BackToMainMenu()
    {
        CloseMenuWindow(gamesMenuWindow);
        OpenMenuWindow(mainMenuWindow);
    }

    void SetWindowInfo(CelestialBodyName name)
    {
        var info = GmManager.CelestialBody(name).Info;
        if (info == null)
            return;

        info.SetInfoUI(infoPanel);
    }

    void Director_stopped(PlayableDirector obj)
    {
        ShowMainMenu();
    }

    void Director_played(PlayableDirector obj)
    {
        HideMainMenu();
    }

    // Restore MainMenu environment
    void ShowMainMenu()
    {
        if (spaceDebriSystem == null) return;

        CameraSwitcher.SwitchCamera(GmManager.MenuCamera);

        if (GmManager.SolarSystemCtrl.GetPlanetScaleMultiplier() > 1)
            TweenPlanetScale(1f, true);

        HideExitButton();
        TweenPivot(mainMenuWindow, new Vector2(0f, 0.5f), new Vector3(0, -30, 0), LeanTweenType.easeInOutSine, 1f, LeanTweenType.easeInCirc, GmManager.CameraSwitchTime);

        spaceDebriSystem.Play();
    }

    void HideMainMenu(bool quit = false, bool stopDebri = true)
    {
        if (!quit) ShowExitButton();

        var id = CloseMenuWindow(mainMenuWindow);

        if (stopDebri) spaceDebriSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        if (quit)
        {
            var closeId = Tweens.ApplicationClose(GmManager.MenuCamera);
            var d1 = LeanTween.descr(id);
            var d2 = LeanTween.descr(closeId);
            var d = d1 ?? d2;

            if (d != null)
                d.setOnComplete(QuitApplication);
        }
    }

    void OpenMenuWindow(GameObject window)
    {
        Tweens.MenuWindowOpen(window);
    }

    int CloseMenuWindow(GameObject window)
    {
        return Tweens.MenuWindowClose(window);
    }

    void QuitApplication()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
    }

    void ShowExitButton()
    {
        TweenPivot(exitButton, new Vector2(-.2f, -.2f), GmManager.CameraSwitchTime);
    }

    void HideExitButton()
    {
        TweenPivot(exitButton, new Vector2(-.2f, 2f), null);
    }

    public static int TweenPivot(GameObject gameObj, Vector2 newPivot, object rotateObj,
                LeanTweenType pivotEase = LeanTweenType.easeInOutSine, float pivotTime = 1f,
                LeanTweenType rotateEase = LeanTweenType.notUsed, float rotateTime = 0f)
    {
        var rect = gameObj.GetComponent<RectTransform>();
        var id_pivot = 0;
        var id_rotate = 0;

        if (rotateObj is Vector3 rotate)
        {
            if (rotateEase == LeanTweenType.notUsed)
                rect.Rotate(rotate);
            else
                id_rotate = LeanTween.rotate(gameObj, rotate, rotateTime).setEase(rotateEase).id;
        }

        id_pivot = LeanTween.value(gameObj, rect.pivot, newPivot, pivotTime).setEase(pivotEase).setOnUpdateVector2((Vector2 pos) =>
        {
            rect.pivot = pos;
        }).id;

        return (pivotTime > rotateTime) ? id_pivot : id_rotate;
    }

    void TweenPlanetScale(float scaleTime, bool scaleOut = false)
    {
        var start = scaleOut ? 10 : 1;
        var end = scaleOut ? 1 : 10;
        var type = scaleOut ? LeanTweenType.easeOutQuint : LeanTweenType.easeInSine;

        LeanTween.value(start, end, scaleTime).setEase(type).setOnUpdate((float val) =>
        {
            GmManager.SolarSystemCtrl.SetPlanetScaleMultiplier(val);
        });
    }

    private IEnumerator DelayExecute(float sec, UnityAction method)
    {
        yield return new WaitForSeconds(sec);

        method();
    }

}