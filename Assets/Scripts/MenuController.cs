using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using Cinemachine;

using static SolarSystemController;
using System.Collections;
using UnityEngine.Events;


// see https://easings.net/

public class MenuController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] GameObject mainMenuWindow;
    [SerializeField] GameObject controlPanel;
    [SerializeField] GameObject infoPanel;
    [SerializeField] GameObject exitButton;
    [SerializeField] ParticleSystem spaceDebriSystem;
    [Header("Sound")]
    [SerializeField] AudioSource slideInSound;
    [Header("Cameras")]
    [SerializeField] CinemachineVirtualCamera menuCamera;
    [SerializeField] CinemachineVirtualCamera solarSystemCamera;

    [Header("Solar System")]
    [SerializeField] GameObject solarSystem;

    private CelestialBody[] _celestialBodies;
    private PlayableDirector _director;
    private SolarSystemController _solarSystemController;
    private bool _controlPanelVisible = false;

    private const float _cameraSwitchTime = 2.0f;

    private void OnEnable()
    {
        CameraSwitcher.Register(menuCamera);
        CameraSwitcher.Register(solarSystemCamera);
    }
    private void OnDisable()
    {
        CameraSwitcher.Unregister(menuCamera);
        CameraSwitcher.Unregister(solarSystemCamera);
    }

    private void Awake()
    {
        _celestialBodies = FindObjectsOfType<CelestialBody>();

        _director = GetComponent<PlayableDirector>();
        _director.played += Director_played;
        _director.stopped += Director_stopped;

        _solarSystemController = solarSystem.GetComponent<SolarSystemController>();

        hideControlPanel();
    }

    private void Start()
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

    public void MenuSolarSytem()
    {
        HideMainMenu();
        ControlPanelDisplay();
        StartCoroutine(DelayExecute(_cameraSwitchTime, _solarSystemController.ShowOrbitLines));
        CameraSwitcher.SwitchCamera(solarSystemCamera);
    }

    public void MenuQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
    }

    public void ExitToMainMenu()
    {
        StartCoroutine(DelayExecute(.5f, _solarSystemController.HideOrbitLines));
        ControlPanelDisplay(true);

        if (_director.state == PlayState.Playing)
            _director.Stop();
        else
            ShowMainMenu();
    }

    public void SolarSystemZoom(System.Single zoom)
    {
        var zPos = zoom switch
        {
            0 => -40f,
            1 => -100f,
            2 => -200f,
            3 => -400f,
            4 => -600f,
            5 => -800f,
            6 => -1000f,
            7 => -1200f,
            _ => -600f,
        };

        LeanTween.moveLocalZ(solarSystemCamera.gameObject, zPos, 0f);
    }

    /// <summary>
    /// Rotate Solar-System around the x-axis between -15° and 90°.
    /// </summary>
    /// <param name="rotate"></param>
    public void SolarSystemRotate(System.Single rotate)
    {
        // Get the parent of the camera for the rotation.
        var camPivot = solarSystemCamera.gameObject.transform.parent.gameObject;

        LeanTween.rotateX(camPivot, rotate * 15, 0f);
    }

    private void ControlPanelDisplay(bool hide = false)
    {
        if (hide && _controlPanelVisible)
        {
            hideControlPanel(true);
            TweenPivot(controlPanel, new Vector2(0.5f, 0f), new Vector3(-90, 0, 0), LeanTweenType.easeInQuint, .5f, LeanTweenType.easeOutQuad, 1f);
        }
        else if (!hide && !_controlPanelVisible)
        {
            // Pivot y from 0 to -0.1 rotate x from -90 to 15 
            TweenPivot(controlPanel, new Vector2(0.5f, -.1f), new Vector3(15, 0, 0), LeanTweenType.easeOutQuint, 1f, LeanTweenType.easeInQuad, 2f);
        }

        _controlPanelVisible = !hide;
    }

    private void hideControlPanel(bool animate = false)
    {
        if (animate)
            TweenPivot(controlPanel, new Vector2(0.5f, 0f), new Vector3(-90, 0, 0), LeanTweenType.easeInQuint, .5f, LeanTweenType.easeOutQuad, 1f);
        else
            TweenPivot(controlPanel, new Vector2(0.5f, 0f), new Vector3(-90, 0, 0));
    }

    private void SetWindowInfo(CelestialBodyName name)
    {
        var info = _celestialBodies.First(b => b.Info.bodyName == name).Info;
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

    private void ShowMainMenu()
    {
        CameraSwitcher.SwitchCamera(menuCamera);

        if (spaceDebriSystem == null)
            return;

        HideExitButton();
        TweenPivot(mainMenuWindow, new Vector2(0f, 0.5f), new Vector3(0, -30, 0), LeanTweenType.easeInOutSine, 1f, LeanTweenType.easeInCirc, 2f);
        spaceDebriSystem.Play();
    }

    private void HideMainMenu()
    {
        spaceDebriSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ShowExitButton();
        TweenPivot(mainMenuWindow, new Vector2(1.2f, 0.5f), new Vector3(0, -110, 0), LeanTweenType.easeInExpo, 1f, LeanTweenType.easeOutCirc, 1f);
    }

    private void ShowExitButton()
    {
        TweenPivot(exitButton, new Vector2(-.2f, -.2f), _cameraSwitchTime);
    }

    private void HideExitButton()
    {
        TweenPivot(exitButton, new Vector2(-.2f, 2f), null);
    }


    private void TweenPivot(GameObject gameObj, Vector2 newPivot, object rotateObj,
                LeanTweenType pivotEase = LeanTweenType.easeInOutSine, float pivotTime = 1f,
                LeanTweenType rotateEase = LeanTweenType.notUsed, float rotateTime = 0f)
    {
        var rect = gameObj.GetComponent<RectTransform>();

        if (rotateObj is Vector3 rotate)
        {
            if (rotateEase == LeanTweenType.notUsed)
                rect.Rotate(rotate);
            else
                LeanTween.rotate(gameObj, rotate, rotateTime).setEase(rotateEase);
        }

        LeanTween.value(gameObject, rect.pivot, newPivot, pivotTime).setEase(pivotEase).setOnUpdateVector2((Vector2 pos) =>
        {
            rect.pivot = pos;
        });
    }

    private IEnumerator DelayExecute(float sec, UnityAction method)
    {
        yield return new WaitForSeconds(sec);

        method();
    }

}