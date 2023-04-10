using System.Linq;
using UnityEngine;

namespace MoonsOfMars.SolarSystem
{
    [ExecuteInEditMode]
    public class SolarSystemController : MonoBehaviour
    {
        public enum CelestialBodyName
        {
            Sun,
            Mercury,
            Venus,
            Earth,
            Moon,
            Mars,
            Phobos,
            Deimos,
            Jupiter,
            Saturn,
            Uranus,
            Neptune,
            Pluto,
            Io,
            Europa,
            Ganymede,
            Callisto,
            Mimas,
            Enceladus,
            Tethys,
            Dione,
            Rhea,
            Titan,
            Hyperion,
            Iapetus,
            Charon
        }

        public enum CelestialBodyType { Planet, Moon, Sun }

        public enum OrbitActiveType { All, None, MoonsOnly }

        // Planet diameter scale 1 = 1unity:10,000km, scale 10 = 1unity:1,000km
        [Range(1, 10)]
        [SerializeField] float _planetScaleMultiplier = 1f;

        [Range(50f, 100f)]
        [Tooltip("Orbit scale multiplier: world units per 1 au.")]
        [SerializeField] internal float UnitsPerAU = 60f;

        public OrbitActiveType orbitActive;

        internal bool IsDemo = true;
        internal bool OrbitLinesVisible = false;

        private KeplerOrbitLinesController _orbitLinesController;
        static readonly CelestialBodyName[] _moons =
    {
            CelestialBodyName.Moon,
            CelestialBodyName.Phobos,
            CelestialBodyName.Deimos,
            CelestialBodyName.Io,
            CelestialBodyName.Europa,
            CelestialBodyName.Ganymede,
            CelestialBodyName.Callisto,
            CelestialBodyName.Mimas,
            CelestialBodyName.Enceladus,
            CelestialBodyName.Tethys,
            CelestialBodyName.Dione,
            CelestialBodyName.Rhea,
            CelestialBodyName.Titan,
            CelestialBodyName.Hyperion,
            CelestialBodyName.Iapetus,
            CelestialBodyName.Charon
        };


        // Giant planets scale only 1/10 of the planets and moons.
        public float GetPlanetScaleMultiplier(bool isGiantPlanet = false)
        {
            if (isGiantPlanet)
                return 1f - 0.1f + _planetScaleMultiplier / 10f;

            return _planetScaleMultiplier;
        }

        public void SetPlanetScaleMultiplier(float value)
        {
            _planetScaleMultiplier = value;
            ApplyChanges();
        }

        public void ShowOrbitLines()
        {
            StartCoroutine(_orbitLinesController.EaseLines(.5f));
            OrbitLinesVisible = true;
        }

        public void HideOrbitLines()
        {
            StartCoroutine(_orbitLinesController.EaseLines(.5f, true));
            OrbitLinesVisible = false;
        }

        public static bool IsMoon(CelestialBodyName name)
        {
            return _moons.Contains(name);
        }

        private void Awake()
        {
            _orbitLinesController = GetComponent<KeplerOrbitLinesController>();
        }

        /// <summary>
        /// Apply the changes to all celestial bodies.
        /// </summary>
        private void ApplyChanges()
        {
            var bodies = (CelestialBody[])FindObjectsOfType(typeof(CelestialBody));
            foreach (var body in bodies)
                body.ApplyChanges();

            var movers = (KeplerOrbitMover[])FindObjectsOfType(typeof(KeplerOrbitMover));
            foreach (var mover in movers)
                mover.ApplyChanges();
        }

        private void OnValidate()
        {
            name = Constants.SolarSystemMain;

            ApplyChanges();
        }

    }
}