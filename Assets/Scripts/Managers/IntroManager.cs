using UnityEngine;
using static SolarSystemManager;

public class IntroManager : MonoBehaviour
{
    public void MarkBody(CelestialBodyName body, bool isDeselect)
    {
        Debug.Log("Select : " + body + " - deselect: " + isDeselect);
    }


}
