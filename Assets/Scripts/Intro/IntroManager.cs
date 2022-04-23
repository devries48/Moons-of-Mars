using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using static SolarSystemController;

public class IntroManager : MonoBehaviour
{
    [SerializeField] private GameObject infoWindow;
    [SerializeField] private GameObject mainMenuWindow;
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

    public void ShowBodyInfoWindow(CelestialBodyName name, bool isDeselect)
    {
        var infoRect = infoWindow.GetComponent<RectTransform>();

        if (isDeselect)
            LeanTween.move(infoRect, new Vector3(220f, -200f, -200), 1f).setEaseInBack();
        else
        {
            SetWindowInfo(name);
            slideInSound.Play();
            LeanTween.move(infoRect, new Vector3(-180f, -200f, 100f), 1f).setEaseOutBack();
        }
    }

    public void StartTour()
    {
        director.Play();
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
        OpenMenu();
    }

    private void Director_played(PlayableDirector obj)
    {
        CloseMenu();
    }

    private void OpenMenu()
    {
        if (spaceDebri_Particles == null)
            return;

        spaceDebri_Particles.SetActive(true);

        var menuRect = mainMenuWindow.GetComponent<RectTransform>();

        LeanTween.rotate(mainMenuWindow, new Vector3(0, -30, 0), 2f).setEase(LeanTweenType.easeOutExpo);
        LeanTween.move(menuRect, new Vector3(270f, 0f, 0f), 1f).setEaseOutQuint();
    }

    private void CloseMenu()
    {
        spaceDebri_Particles.SetActive(false);

        var menuRect = mainMenuWindow.GetComponent<RectTransform>();

        LeanTween.rotate(mainMenuWindow, new Vector3(0, -89, 0), 2f).setEase(LeanTweenType.easeOutElastic);
        LeanTween.move(menuRect, new Vector3(-200f, 0f, 0f), 1f).setEaseInQuint();
    }
}