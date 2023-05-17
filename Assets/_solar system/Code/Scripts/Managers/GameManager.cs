using Cinemachine;
using MoonsOfMars.Shared;
using System.Linq;
using UnityEngine;

namespace MoonsOfMars.SolarSystem
{
    [SelectionBase]
    [DisallowMultipleComponent]
    public class GameManager : GameManagerBase<GameManager>
    {

        #region editor fields
        [Header("Cameras")]
        public Camera m_MainCamera;
        public CinemachineVirtualCamera m_MenuCamera;
        public CinemachineVirtualCamera m_SolarSystemCamera;

        [Header("Solar System Controller")]
        public GameObject m_SolarSystem;
        #endregion

        #region properties
        public AudioManager AudioManager => AudioManager<AudioManager>();

        public float CameraSwitchTime { get; private set; }

        public int SolarSystemSpeed { get; internal set; }

        public SolarSystemController SolarSystemCtrl
        {
            get
            {
                if (__solarSystemCtrl == null)
                    m_SolarSystem.TryGetComponent(out __solarSystemCtrl);

                return __solarSystemCtrl;
            }
        }

        public bool IsGameQuit { get; internal set; }

        SolarSystemController __solarSystemCtrl;

        #endregion

        CelestialBody[] _celestialBodies;

        ///void OnEnable() => __instance = this;

        protected override void Awake()
        {
            base.Awake();

            _celestialBodies = FindObjectsOfType<CelestialBody>();

            if (m_MainCamera.TryGetComponent<CinemachineBrain>(out var brain))
                CameraSwitchTime = brain.m_DefaultBlend.BlendTime;
        }

        public CelestialBody CelestialBody(SolarSystemController.CelestialBodyName name)
        {
            return _celestialBodies.First(b => b.Info.bodyName == name);
        }

    }
}