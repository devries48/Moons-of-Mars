using System;
using System.Linq;
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

    public void ShowBodyInfoWindow(CelestialBodyName name, bool isDeselect)
    {

        var infoRect = infoWindow.GetComponent<RectTransform>();

        if (isDeselect)
            LeanTween.move(infoRect, new Vector3(220f, -200f, -200), 1f).setEaseInBack();
        else
        {
            SetWindowInfo(name);
            LeanTween.move(infoRect, new Vector3(-180f, -200f, 100f), 1f).setEaseOutBack();
        }
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
            Debug.Log(child.name);
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
                    Debug.Log(info.gravity);
                    break;
                default:
                    break;
            }
        }
    }
}
