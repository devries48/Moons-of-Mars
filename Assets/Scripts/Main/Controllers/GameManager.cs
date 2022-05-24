using Cinemachine;
using UnityEngine;

[SelectionBase]
[DisallowMultipleComponent]
public class GameManager : MonoBehaviour
{
    #region editor fields
    [Header("Cameras")]
    [SerializeField] public Camera MainCamera;
    [SerializeField] public CinemachineVirtualCamera MenuCamera;
    [SerializeField] public CinemachineVirtualCamera SolarSystemCamera;

    [Header("Solar System Controller")]
    [SerializeField] public GameObject SolarSystem;
    #endregion

    #region properties

    public static GameManager Instance
    {
        get
        {
            if (__instance == null)
                __instance = FindObjectOfType<GameManager>();

            return __instance;
        }
    }
    static GameManager __instance;

    public SolarSystemController SolarSystemCtrl
    {
        get
        {
            if (__solarSystemCtrl == null)
                SolarSystem.TryGetComponent(out __solarSystemCtrl);

            return __solarSystemCtrl;
        }
    }
    SolarSystemController __solarSystemCtrl;
    #endregion

}