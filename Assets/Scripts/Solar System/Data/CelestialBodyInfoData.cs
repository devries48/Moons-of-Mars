using UnityEngine;
using static SolarSystemController;

public class CelestialBodyInfoData
{
    public CelestialBodyType bodyType;
    public CelestialBodyName bodyName;
    public string description;
    public string diameter;
    public string gravity;

    internal void SetInfoUI(GameObject infoPanel)
    {
        for (int i = 0; i < infoPanel.transform.childCount; i++)
        {
            var child = infoPanel.transform.GetChild(i).GetComponent<TMPro.TextMeshProUGUI>();
            if (child == null)
                continue;

            switch (child.name)
            {
                case "Value Title":
                    child.text = bodyName.ToString();
                    break;
                case "Value Description":
                    child.text = description;
                    break;
                case "Value Diameter":
                    child.text = diameter;
                    break;
                case "Value Gravity":
                    child.text = gravity;
                    break;
                default:
                    break;
            }
        }

    }
}
