using System;
using System.Collections;
using System.Globalization;
using UnityEngine;
using static SolarSystemManager;

[ExecuteInEditMode]
[RequireComponent(typeof(Rigidbody))]
public class CelestialBody : MonoBehaviour
{
    #region public fields
    public CelestialBodyName bodyName;
    CelestialBodyType bodyType;
    public Transform parentBody;
    [Header("Distance from sun/planet in 10^6 km")]
    public float distance;
    [Header("Diameter in km")]
    public float diameter;
    [Header("Surface gravity in g")]
    public float gravity;
    [Header("Orbital period in days")]
    public float period;
    [Header("Orbital velocity in km/s")]
    public float velocity;
    [Header("Tilt and incline in degrees")]
    public float BodyAxialTilt;
    public float OrbitalInclination;
    [Header("Rotation period in hours")]
    public float RotationPeriod;
    #endregion

    CelestialBodyInfo _celestialBodyInfo;

    public CelestialBodyInfo Info => _celestialBodyInfo;

    SolarSystemManager solarSystemManager;
    Ellipse orbitPath;

    [SerializeField]
    [Range(0, 1f)] float orbitProgress = 0f;
    float orbitPeriod = 0f;

    const float physicsTimeStep = 0.01f;
    const float sunDiameter = 1392700;
    const float sunScale = .25f; // Make the sun smaller

    void Start()
    {
        solarSystemManager = GameObject.Find(Constants.SolarSystemManager).GetComponent<SolarSystemManager>();
        SetBodyInfo();

        if (Application.isPlaying && parentBody != null && orbitPeriod != 0)
        {
            SetOrbitingBodyPosition();
            StartCoroutine(AnimateOrbit());
        }
    }

    void FixedUpdate()
    {
        var rotationSpeed = solarSystemManager.isDemo && bodyType != CelestialBodyType.Sun 
            ? 30 * Time.fixedDeltaTime
            : (1 / RotationPeriod) * 1000 * Time.fixedDeltaTime;

        transform.Rotate(0, rotationSpeed, 0);
    }

    void OnValidate()
    {
        ApplyChanges();
    }

    void SetOrbitingBodyPosition()
    {
        Vector2 orbitPos = orbitPath.Evaluate(orbitProgress);
        Vector3 pos = new Vector3(orbitPos.y, 0, orbitPos.x);

        if (bodyType == CelestialBodyType.Moon)
            pos += parentBody.transform.localPosition;

        gameObject.transform.localPosition = pos;
    }

    IEnumerator AnimateOrbit()
    {
        float orbitSpeed = 1f / orbitPeriod;

        while (IsOrbitActive())
        {
            orbitProgress += Time.fixedDeltaTime * orbitSpeed;
            orbitProgress %= 1f;
            SetOrbitingBodyPosition();
            yield return null;
        }
    }

    bool IsOrbitActive()
    {
        return (solarSystemManager.orbitActive == SolarSystemManager.OrbitActiveType.All
            || (solarSystemManager.orbitActive == SolarSystemManager.OrbitActiveType.MoonsOnly && bodyType == CelestialBodyType.Moon));
    }

    /// <summary>
    /// Sets Celestialbody scale, rotation & orbit
    /// </summary>
    public void ApplyChanges()
    {
        solarSystemManager = GameObject.Find(Constants.SolarSystemManager).GetComponent<SolarSystemManager>();
        gameObject.name = bodyName.ToString();
        SetBodyInfo();

        var trans = gameObject.transform;
        var axialTilt = Quaternion.Euler(BodyAxialTilt, 0, 0);
        var sunSize = sunDiameter * solarSystemManager.PlanetScale * sunScale;
        var sunDistance = (distance * solarSystemManager.DistanceScale) + sunSize / 2;

        var pos = (bodyType == CelestialBodyType.Sun) ? Vector3.zero : new Vector3(sunDistance, 0, 0);

        trans.SetPositionAndRotation(pos, axialTilt);

        if (bodyType == CelestialBodyType.Sun)
        {
            diameter = sunDiameter;
            trans.localScale = sunSize * Vector3.one;
        }
        else
            trans.localScale = diameter * solarSystemManager.PlanetScale * Vector3.one;

        if (parentBody != null)
        {
            float orbit = 0;

            if (bodyType == CelestialBodyType.Planet)
                orbit = sunDistance;
            else if (bodyType == CelestialBodyType.Moon)
                orbit = (distance * solarSystemManager._planetScale * 10) + (parentBody.localScale.x / 2);

            orbitPeriod = period * solarSystemManager.OrbitScale;
            orbitPath = new Ellipse(orbit, orbit);
            orbitProgress = orbitPeriod < 0 ? 1 : 0;

            if (IsOrbitActive())
                SetOrbitingBodyPosition();
        }
    }

    /// <summary>
    /// Set body type and creates the CelestialBodyInfo class used bij the UI Info Window.
    /// </summary>
    private void SetBodyInfo()
    {
        string description;

        switch (bodyName)
        {
            case CelestialBodyName.Sun:
                bodyType = CelestialBodyType.Sun;
                description = "Yellow Dwarf Star";
                break;
            case CelestialBodyName.Mercury:
            case CelestialBodyName.Venus:
            case CelestialBodyName.Earth:
            case CelestialBodyName.Mars:
                bodyType = CelestialBodyType.Planet;
                description = "Terrestial Planet";
                break;
            case CelestialBodyName.Moon:
                bodyType = CelestialBodyType.Moon;
                description = "Moon";
                break;
            case CelestialBodyName.Jupiter:
            case CelestialBodyName.Saturn:
            case CelestialBodyName.Uranus:
            case CelestialBodyName.Neptune:
                bodyType = CelestialBodyType.Planet;
                description = "Gas Giant";
                break;
            case CelestialBodyName.Pluto:
                bodyType = CelestialBodyType.Planet;
                description = "Dwarf Planet";
                break;
            default:
                description = "Unknown";
                break;
        }

        _celestialBodyInfo = new CelestialBodyInfo()
        {
            bodyName = bodyName,
            bodyType = bodyType,
            description = description,
            diameter = diameter.ToString("N0", new CultureInfo("en-US")) + " km",
            gravity = gravity.ToString("N2", new CultureInfo("en-US")) + " g"
        };
    }
}