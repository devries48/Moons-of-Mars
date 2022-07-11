using Cinemachine;
using System;
using System.Linq;
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

    public float CameraSwitchTime { get; private set; }

    public int SolarSystemSpeed { get; internal set; }

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


    CelestialBody[] _celestialBodies;

    void Awake()
    {
        _celestialBodies = FindObjectsOfType<CelestialBody>();

        if (MainCamera.TryGetComponent<CinemachineBrain>(out var brain))
            CameraSwitchTime = brain.m_DefaultBlend.BlendTime;
    }

    public CelestialBody CelestialBody(SolarSystemController.CelestialBodyName name)
    {
        Debug.LogWarning(name);
        var body = _celestialBodies.First(b => b.Info.bodyName == name);
        return body;
    }


}