using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Events;

using static SolarSystemController;
using Cinemachine;

// see https://easings.net/

[DisallowMultipleComponent]
public class MenuController : MonoBehaviour
{
    #region editor fields
    [Header("UI Elements")]
    [SerializeField] GameObject mainMenuWindow;
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
    bool _appQuit;
    #endregion

    #region properties
    GameManager GameManager => GameManager.Instance;

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
        CameraSwitcher.Register(GameManager.MenuCamera);
        CameraSwitcher.Register(GameManager.SolarSystemCamera);
        SystemPanelController.HideControlPanel();
    }

    void OnDisable()
    {
        CameraSwitcher.Unregister(GameManager.MenuCamera);
        CameraSwitcher.Unregister(GameManager.SolarSystemCamera);
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

        StartCoroutine(DelayExecute(GameManager.CameraSwitchTime, GameManager.SolarSystemCtrl.ShowOrbitLines));

        if (GameManager.SolarSystemCtrl.GetPlanetScaleMultiplier() == 1)
            TweenPlanetScale(GameManager.CameraSwitchTime);

        CameraSwitcher.SwitchCamera(GameManager.SolarSystemCamera);
    }

    public void MenuQuit()
    {
        HideMainMenu(true);
    }

    public void ExitToMainMenu()
    {
        var wait = 0f;

        if (GameManager.SolarSystemCtrl.OrbitLinesVisible)
        {
            wait = .5f; // Wait for orbit lines to fade away
            GameManager.SolarSystemCtrl.HideOrbitLines();
            SystemPanelController.HideControlPanel(true);
        }

        if (_director.state == PlayState.Playing)
            _director.Stop();
        else if (wait > 0)
            StartCoroutine(DelayExecute(wait, ShowMainMenu));
        else
            ShowMainMenu();
    }

    private void SetWindowInfo(CelestialBodyName name)
    {
        var info = GameManager.CelestialBody(name).Info;
        if (info == null)
            return;

        info.SetInfoUI(infoPanel);
    }

    private void Director_stopped(PlayableDirector obj)
    {
        ShowMainMenu();
    }

    private void Director_played(PlayableDirector obj)
    {
        HideMainMenu();
    }

    // Restore MainMenu environment
    private void ShowMainMenu()
    {

        if (spaceDebriSystem == null)
            return;

        CameraSwitcher.SwitchCamera(GameManager.MenuCamera);

        if (GameManager.SolarSystemCtrl.GetPlanetScaleMultiplier() > 1)
            TweenPlanetScale(1f, true);

        HideExitButton();
        TweenPivot(mainMenuWindow, new Vector2(0f, 0.5f), new Vector3(0, -30, 0), LeanTweenType.easeInOutSine, 1f, LeanTweenType.easeInCirc, GameManager.CameraSwitchTime);

        spaceDebriSystem.Play();
    }

    private void HideMainMenu(bool quit = false)
    {
        if (!quit) ShowExitButton();

        var time = quit ? 0.5f : 1f;
        var id = TweenPivot(mainMenuWindow, new Vector2(1.2f, 0.5f), new Vector3(0, -110, 0), LeanTweenType.easeInExpo, time, LeanTweenType.easeOutCirc, time);

        spaceDebriSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        if (quit)
        {
            //  center & zoom out
            var zoomId = 0;
            CinemachineFramingTransposer transposer = null;

            if (GameManager.MenuCamera.TryGetComponent<CinemachineVirtualCamera>(out var cam))
                transposer = cam.GetCinemachineComponent<CinemachineFramingTransposer>();

            if (transposer != null)
            {
                LeanTween.value(transposer.m_ScreenX, 0.5f, time / 2).setEase(LeanTweenType.easeOutQuint).setOnUpdate((float val) =>
                {
                    transposer.m_ScreenX = val;
                });
                LeanTween.value(transposer.m_ScreenY, 0.5f, time / 2).setEase(LeanTweenType.easeOutQuint).setOnUpdate((float val) =>
                {
                    transposer.m_ScreenY = val;
                });
                zoomId = LeanTween.value(transposer.m_CameraDistance, 100f, time).setEase(LeanTweenType.easeOutQuad).setOnUpdate((float val) =>
                {
                    transposer.m_CameraDistance = val;
                }).id;
            }

            var d1 = LeanTween.descr(id);
            var d2 = LeanTween.descr(zoomId);
            var d = d1 ?? d2;

            if (d != null)
                d.setOnComplete(QuitApplication);
        }
    }

    private void QuitApplication()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
    }

    private void ShowExitButton()
    {
        TweenPivot(exitButton, new Vector2(-.2f, -.2f), GameManager.CameraSwitchTime);
    }

    private void HideExitButton()
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

    private void TweenPlanetScale(float scaleTime, bool scaleOut = false)
    {
        var start = scaleOut ? 10 : 1;
        var end = scaleOut ? 1 : 10;
        var type = scaleOut ? LeanTweenType.easeOutQuint : LeanTweenType.easeInSine;

        LeanTween.value(start, end, scaleTime).setEase(type).setOnUpdate((float val) =>
        {
            GameManager.SolarSystemCtrl.SetPlanetScaleMultiplier(val);
        });
    }

    private IEnumerator DelayExecute(float sec, UnityAction method)
    {
        yield return new WaitForSeconds(sec);

        method();
    }

}