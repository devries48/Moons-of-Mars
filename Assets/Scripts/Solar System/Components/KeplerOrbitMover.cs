using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Component for moving game object in eliptic or hyperbolic path around attractor body.
/// </summary>
/// <seealso cref="MonoBehaviour" />
[ExecuteAlways]
[SelectionBase]
[DisallowMultipleComponent]
public class KeplerOrbitMover : MonoBehaviour
{
    /// <summary>
    /// The attractor settings data.
    /// Attractor object reference must be assigned or orbit moving will not work.
    /// </summary>
    public KeplerAttractorData AttractorSettings = new KeplerAttractorData();

    public bool IsOrbitingPlanet = false;

    [Tooltip("Giant planets scale 1/10 in PlanetScaler.")]
    public bool IsGiantPlanet = false;

    public int OrbitExtraRange = 0;

    /// <summary>
    /// The orbit curve precision.
    /// </summary>
    [Header("Orbit line")]
    public int OrbitPointsCount = 50;

    /// <summary>
    /// The maximum orbit distance of orbit display in world units.
    /// </summary>
    public float MaxOrbitWorldUnitsDistance = 1000f;

    internal float TimeScale = 1f;

    private SolarSystemController _solarSystem;

    private SolarSystemController Controller
    {
        get
        {
            if (_solarSystem == null)
            {
                var solarSystem = GameObject.Find(Constants.SolarSystemMain);
                _solarSystem = solarSystem.GetComponent<SolarSystemController>();
            }
            return _solarSystem;
        }
    }

    /// <summary>
    /// Astronomical unit in SI units.
    /// </summary>
    /// <remarks>
    /// Used to calculate real scale orbit periods.
    /// </remarks>
    const double AU = 1.495978707e11;

    // Gravitational constant. In this context plays role of speed muliplier.
    const double GConstant = 100;

    internal void ApplyChanges()
    {
   
        float units = 50f;

        if (Controller != null)
            units = Controller.UnitsPerAU;
        {
        }

            if (IsOrbitingPlanet)
        {
            var mltp = 1f;

                var parent = AttractorSettings.AttractorObject.GetComponent<KeplerOrbitMover>();

                if (parent != null)
                    mltp = Controller.GetPlanetScaleMultiplier(parent.IsGiantPlanet);

            

            units += OrbitExtraRange * mltp;
        }

        // G constant is used as free parameter to fixate orbits periods values while SemiMajor axis parameter is adjusted for the scene.
        double compensatedGConst = GConstant / Math.Pow(AU / units, 3d);

        AttractorSettings.GravityConstant = (float)compensatedGConst;

        OrbitData = new KeplerOrbitData(
                 eccentricity: eccentricity,
                 semiMajorAxis: semiMajorAxis * units,
                 meanAnomalyDeg: meanAnomalyDeg,
                 inclinationDeg: inclinationDeg,
                 argOfPerifocusDeg: argOfPerifocusDeg,
                 ascendingNodeDeg: ascendingNodeDeg,
                 attractorMass: AttractorSettings.AttractorMass,
                 gConst: compensatedGConst,
                 period: OrbitalPeriod * 86400
                 );

        if (AttractorSettings.AttractorObject != null && OrbitData.SemiMajorAxis > 0)
            ForceUpdateViewFromInternalState();
    }

    /// <summary>
    /// Disable continious editing orbit in update loop, if you don't need it.
    /// It is also very useful in cases, when orbit is not stable due to float precision limits.
    /// </summary>
    /// <remarks>
    /// Internal orbit data uses double prevision vectors, but every update it is compared with unity scene vectors, which are float precision.
    /// In result, if unity vectors precision is not enough for current values, then orbit become unstable.
    /// To avoid this issue, you can disable comparison, and then orbit motion will be nice and stable, but you will no longer be able to change orbit by moving objects in editor.
    /// </remarks>
    [Tooltip("Disable continious editing orbit in update loop, if you don't need it, or you need to fix Kraken issue on large scale orbits.")]
    public bool LockOrbitEditing = false;

    [Header("Orbital period in days")]
    public float OrbitalPeriod;

    /// <summary>
    /// The orbit data.
    /// Internal state of orbit.
    /// </summary>
    internal KeplerOrbitData OrbitData = new KeplerOrbitData();
    [Header("JPL data")]
    public double eccentricity;
    public double semiMajorAxis;
    public double meanAnomalyDeg;
    public double inclinationDeg;
    public double argOfPerifocusDeg;
    public double ascendingNodeDeg;

    private Coroutine _updateRoutine;

    private bool IsReferencesAsigned
    {
        get { return AttractorSettings != null && AttractorSettings.AttractorObject != null; }
    }

    private void OnEnable()
    {
        if (!LockOrbitEditing)
        {
            ForceUpdateOrbitData();
        }
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            return;
        }
#endif
        if (_updateRoutine != null)
        {
            StopCoroutine(_updateRoutine);
        }

        _updateRoutine = StartCoroutine(OrbitUpdateLoop());
    }

    private void OnDisable()
    {
        if (_updateRoutine != null)
        {
            StopCoroutine(_updateRoutine);
            _updateRoutine = null;
        }
    }

    /// <summary>
    /// Updates orbit internal data.
    /// </summary>
    /// <remarks>
    /// In this method orbit data is updating from view state:
    /// If you change body position, attractor mass or any other vital orbit parameter, 
    /// this change will be noticed and applyed to internal OrbitData state in this method.
    /// If you need to change orbitData state directly, by script, you need to change OrbitData state and then call ForceUpdateOrbitData
    /// </remarks>
    private void Update()
    {
        if (IsReferencesAsigned)
        {
            if (!LockOrbitEditing)
            {
                var pos = transform.position - AttractorSettings.AttractorObject.position;
                KeplerVector3d position = new KeplerVector3d(pos.x, pos.y, pos.z);

                if (position != OrbitData.Position ||
                    OrbitData.GravConst != AttractorSettings.GravityConstant ||
                    OrbitData.AttractorMass != AttractorSettings.AttractorMass)
                {
                    ForceUpdateOrbitData();
                }
            }
        }
    }

    private void OnValidate()
    {
        ApplyChanges();
    }


    /// <summary>
    /// Progress orbit path motion.
    /// Actual kepler orbiting is processed here.
    /// </summary>
    /// <remarks>
    /// Orbit motion progress calculations must be placed after Update, so orbit parameters changes can be applyed,
    /// but before LateUpdate, so orbit can be displayed in same frame.
    /// Coroutine loop is best candidate for achieving this.
    /// </remarks>
    private IEnumerator OrbitUpdateLoop()
    {
        while (true)
        {
            if (IsReferencesAsigned)
            {
                if (!OrbitData.IsValidOrbit)
                {
                    //try to fix orbit if we can.
                    OrbitData.CalculateOrbitStateFromOrbitalVectors();
                }

                if (OrbitData.IsValidOrbit)
                {
                    OrbitData.UpdateOrbitDataByTime(Time.deltaTime * TimeScale);
                    ForceUpdateViewFromInternalState();
                }
            }

            yield return null;
        }
    }

    /// <summary>
    /// Forces the update of body position, and velocity handler from OrbitData.
    /// Call this method after any direct changing of OrbitData.
    /// </summary>
    [ContextMenu("Update transform from orbit state")]
    public void ForceUpdateViewFromInternalState()
    {
        var pos = new Vector3((float)OrbitData.Position.x, (float)OrbitData.Position.y, (float)OrbitData.Position.z);
        transform.position = AttractorSettings.AttractorObject.position + pos;
        //ForceUpdateVelocityHandleFromInternalState();
    }

    /// <summary>
    /// Forces the update of internal orbit data from current world positions of body, attractor settings and velocityHandle.
    /// </summary>
    /// <remarks>
    /// This method must be called after any manual changing of body position, velocity handler position or attractor settings.
    /// It will update internal OrbitData state from view state.
    /// </remarks>
    [ContextMenu("Update Orbit data from current vectors")]
    public void ForceUpdateOrbitData()
    {
        if (IsReferencesAsigned)
        {
            OrbitData.AttractorMass = AttractorSettings.AttractorMass;
            OrbitData.GravConst = AttractorSettings.GravityConstant;

            // Possible loss of precision, may be a problem in some situations.
            var pos = transform.position - AttractorSettings.AttractorObject.position;
            OrbitData.Position = new KeplerVector3d(pos.x, pos.y, pos.z);
            OrbitData.CalculateOrbitStateFromOrbitalVectors();
        }
    }

    /// <summary>
    /// Change orbit velocity vector to match circular orbit.
    /// </summary>
    [ContextMenu("Circularize orbit")]
    public void SetAutoCircleOrbit()
    {
        if (IsReferencesAsigned)
        {
            OrbitData.Velocity = KeplerOrbitUtils.CalcCircleOrbitVelocity(KeplerVector3d.zero, OrbitData.Position, OrbitData.AttractorMass, OrbitData.OrbitNormal, OrbitData.GravConst);
            OrbitData.CalculateOrbitStateFromOrbitalVectors();
        }
    }

    [ContextMenu("Inverse velocity")]
    public void InverseVelocity()
    {
        if (IsReferencesAsigned)
        {
            OrbitData.Velocity = -OrbitData.Velocity;
            OrbitData.CalculateOrbitStateFromOrbitalVectors();
        }
    }

    [ContextMenu("Inverse position")]
    public void InversePositionRelativeToAttractor()
    {
        if (IsReferencesAsigned)
        {
            OrbitData.Position = -OrbitData.Position;
            OrbitData.CalculateOrbitStateFromOrbitalVectors();
        }
    }

    [ContextMenu("Inverse velocity and position")]
    public void InverseOrbit()
    {
        if (IsReferencesAsigned)
        {
            OrbitData.Velocity = -OrbitData.Velocity;
            OrbitData.Position = -OrbitData.Position;
            OrbitData.CalculateOrbitStateFromOrbitalVectors();
        }
    }

    [ContextMenu("Reset orbit")]
    public void ResetOrbit()
    {
        OrbitData = new KeplerOrbitData();
    }
}
