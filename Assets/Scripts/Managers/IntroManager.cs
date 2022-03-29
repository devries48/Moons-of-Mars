using UnityEngine;
using static SolarSystemManager;

public class IntroManager : MonoBehaviour
{
    CelestialBody[] celestialBodies;
    [SerializeField] private GameObject infoWindow;

    private void Awake()
    {
        celestialBodies = FindObjectsOfType<CelestialBody>();
    }

    public void ShowBodyInfoWindow(CelestialBodyName body, bool isDeselect)
    {
        var  infoRect = infoWindow.GetComponent<RectTransform>();

        if (isDeselect)
            LeanTween.move(infoRect, new Vector3(220f, -200f, -200), 1f).setEaseInBack();
        else 
            LeanTween.move(infoRect, new Vector3(-180f, -200f, 100f), 1f).setEaseOutBack();
    }
}
