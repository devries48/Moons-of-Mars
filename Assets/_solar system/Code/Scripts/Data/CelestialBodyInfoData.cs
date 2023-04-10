using UnityEngine;
using static MoonsOfMars.SolarSystem.SolarSystemController;

namespace MoonsOfMars.SolarSystem
{
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
                if (!infoPanel.transform.GetChild(i).TryGetComponent<TMPro.TextMeshProUGUI>(out var child))
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
}