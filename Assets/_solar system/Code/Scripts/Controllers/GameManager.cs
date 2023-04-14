using Cinemachine;
using System.Linq;
using UnityEngine;


// todo music
// Music by <a href="https://pixabay.com/users/lexin_music-28841948/?utm_source=link-attribution&amp;utm_medium=referral&amp;utm_campaign=music&amp;utm_content=121842">Lexin_Music</a> from <a href="https://pixabay.com//?utm_source=link-attribution&amp;utm_medium=referral&amp;utm_campaign=music&amp;utm_content=121842">Pixabay</a>

namespace MoonsOfMars.SolarSystem
{
    [SelectionBase]
    [DisallowMultipleComponent]
    public class GameManager : MonoBehaviour
    {

        #region singleton

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

        #endregion

        #region editor fields
        [Header("Cameras")]
        public Camera MainCamera;
        public CinemachineVirtualCamera MenuCamera;
        public CinemachineVirtualCamera SolarSystemCamera;

        [Header("Solar System Controller")]
        public GameObject SolarSystem;
        #endregion

        #region properties

        public float CameraSwitchTime { get; private set; }

        public int SolarSystemSpeed { get; internal set; }

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

        void OnEnable() => __instance = this;

        void Awake()
        {
            _celestialBodies = FindObjectsOfType<CelestialBody>();

            if (MainCamera.TryGetComponent<CinemachineBrain>(out var brain))
                CameraSwitchTime = brain.m_DefaultBlend.BlendTime;
        }

        public CelestialBody CelestialBody(SolarSystemController.CelestialBodyName name)
        {
            return _celestialBodies.First(b => b.Info.bodyName == name);
        }



    }
}