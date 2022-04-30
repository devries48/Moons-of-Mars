using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using static SolarSystemController;

// see https://easings.net/

public class MenuManager : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuWindow;
    [SerializeField] private GameObject timePanel;
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private GameObject exitButton;
    [SerializeField] private AudioSource slideInSound;

    private CelestialBody[] celestialBodies;
    private PlayableDirector director;
    private GameObject spaceDebri_Particles;

    void Awake()
    {
        celestialBodies = FindObjectsOfType<CelestialBody>();

        director = GetComponent<PlayableDirector>();
        director.played += Director_played;
        director.stopped += Director_stopped;

        spaceDebri_Particles = GameObject.Find(Constants.SpaceDebri);
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
        director.Play();
    }

    public void MenuSolarSytem()
    {

    }

    public void MenuQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
    }

    public void ShowTimeWindow()
    {
        TweenPivot(timePanel, new Vector2(0.5f, -.1f), new Vector3(15, 0, 0));
    }

    public void HideTimeWindow()
    {
        TweenPivot(timePanel, new Vector2(0.5f, 2f),  Vector3.zero);
    }


    private void SetWindowInfo(CelestialBodyName name)
    {
        var info = celestialBodies.First(b => b.Info.bodyName == name).Info;
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
        if (spaceDebri_Particles == null)
            return;

        spaceDebri_Particles.SetActive(true);
        
        HideExitButton();
        TweenPivot(mainMenuWindow, new Vector2(0f, 0.5f), new Vector3(0, -30, 0), LeanTweenType.easeInOutSine, 1f, LeanTweenType.easeInCirc, 2f);
    }

    private void HideMainMenu()
    {
        spaceDebri_Particles.SetActive(false);

        ShowExitButton();
        TweenPivot(mainMenuWindow, new Vector2(1.2f, 0.5f), new Vector3(0, -110, 0), LeanTweenType.easeInExpo, 1f, LeanTweenType.easeOutCirc, 1f);
    }

    private void ShowExitButton()
    {
        TweenPivot(exitButton, new Vector2(-.2f, -.2f), null);
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

}