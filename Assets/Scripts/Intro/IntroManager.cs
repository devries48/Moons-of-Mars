using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using static SolarSystemController;

// see https://easings.net/

public class IntroManager : MonoBehaviour
{
    [SerializeField] private GameObject infoWindow;
    [SerializeField] private GameObject mainMenuWindow;
    [SerializeField] private GameObject timeWindow;
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
        ShowTimeWindow();
    }

    public void ShowBodyInfoWindow(CelestialBodyName name, bool isDeselect)
    {
        var infoRect = infoWindow.GetComponent<RectTransform>();

        if (isDeselect)
        {
            LeanTween.move(infoRect, new Vector3(220f, -200f, -200), 1f).setEaseInBack();
            var pivotHide = new Vector2(0f, 0.5f);
            LeanTween.value(gameObject, infoRect.pivot, pivotHide, 1f).setEase(LeanTweenType.easeInOutBack).setOnUpdateVector2((Vector2 pos) =>
            {
                infoRect.pivot = pos;
            });
        }
        else
        {
            SetWindowInfo(name);
            slideInSound.Play();
            var pivotShow = new Vector2(1.2f, 0.5f);

            SetWindowInfo(name);
            slideInSound.Play();
            LeanTween.value(gameObject, infoRect.pivot, pivotShow, 1f).setEase(LeanTweenType.easeInOutBack).setOnUpdateVector2((Vector2 pos) =>
            {
                infoRect.pivot = pos;
            });
        }
    }

    public void StartTour()
    {
        director.Play();
    }


    public void ShowTimeWindow()
    {
        var timeRect = timeWindow.GetComponent<RectTransform>();
        var pivotShow = new Vector2(0.5f, -.1f);

        timeRect.Rotate(15, 0, 0);
        LeanTween.value(gameObject, timeRect.pivot, pivotShow, 1f).setEase(LeanTweenType.easeInOutSine).setOnUpdateVector2((Vector2 pos) =>
        {
            timeRect.pivot = pos;
        });
    }

    public void HideTimeWindow()
    {
        var timeRect = timeWindow.GetComponent<RectTransform>();
        var pivotHide = new Vector2(0.5f, 2f);

        timeRect.Rotate(0, 0, 0);
        LeanTween.value(gameObject, timeRect.pivot, pivotHide, 1f).setEase(LeanTweenType.easeInOutSine).setOnUpdateVector2((Vector2 pos) =>
        {
            timeRect.pivot = pos;
        });
    }


    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
    }

    private void SetWindowInfo(CelestialBodyName name)
    {
        var info = celestialBodies.First(b => b.Info.bodyName == name).Info;
        if (info == null)
            return;

        for (int i = 0; i < infoWindow.transform.childCount; i++)
        {
            var child = infoWindow.transform.GetChild(i).GetComponent<TMPro.TextMeshProUGUI>();
            if (child == null)
                continue;

            switch (child.name)
            {
                case "Value Title":
                    child.text = info.bodyName.ToString();
                    break;
                case "Value Description":
                    child.text = info.description;
                    break;
                case "Value Diameter":
                    child.text = info.diameter;
                    break;
                case "Value Gravity":
                    child.text = info.gravity;
                    break;
                default:
                    break;
            }
        }
    }

    private void Director_stopped(PlayableDirector obj)
    {
        ShowMenu();
    }

    private void Director_played(PlayableDirector obj)
    {
        HideMenu();
    }

    private void ShowMenu()
    {
        if (spaceDebri_Particles == null)
            return;

        spaceDebri_Particles.SetActive(true);

        var menuRect = mainMenuWindow.GetComponent<RectTransform>();
        var pivotShow = new Vector2(0f, 0.5f);

        LeanTween.rotate(mainMenuWindow, new Vector3(0, -30, 0), 2f).setEase(LeanTweenType.easeInCirc);
        LeanTween.value(gameObject, menuRect.pivot, pivotShow, 1f).setEase(LeanTweenType.easeInOutSine).setOnUpdateVector2((Vector2 pos) =>
        {
            menuRect.pivot = pos;
        });
    }

    private void HideMenu()
    {
        spaceDebri_Particles.SetActive(false);

        var menuRect = mainMenuWindow.GetComponent<RectTransform>();
        var pivotHide = new Vector2(1.2f, 0.5f);

        LeanTween.rotate(mainMenuWindow, new Vector3(0, -110, 0), 1f).setEase(LeanTweenType.easeOutCirc);
        LeanTween.value(gameObject, menuRect.pivot, pivotHide, 1.2f).setEase(LeanTweenType.easeInExpo).setOnUpdateVector2((Vector2 pos) =>
        {
            menuRect.pivot = pos;
        });

    }
}