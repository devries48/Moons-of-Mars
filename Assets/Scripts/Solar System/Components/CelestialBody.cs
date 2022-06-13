using System;
using System.Globalization;
using UnityEngine;
using static SolarSystemController;

[ExecuteInEditMode]
public class CelestialBody : MonoBehaviour
{
    #region editor fields

    public CelestialBodyName bodyName;
    public CelestialBodyType bodyType;

    [Header("Custom rotation")]
    [Tooltip("Rotation time in seconds")]
    [SerializeField] int rotationTime;

    [Header("Solar System Data")]
    [Tooltip("Mean distance from sun/planet in 10^6 km")]
    [SerializeField] float distance;

    [Tooltip("Diameter in km")]
    [SerializeField] float diameter;

    [Tooltip("Surface gravity in g")]
    [SerializeField] float gravity;

    [Tooltip("Orbital period in days")]
    [SerializeField] float period;

    [Tooltip("Tilt in degrees")]
    public float BodyAxialTilt;

    [Tooltip("Rotation period in hours")]
    public float RotationPeriod;
    #endregion

    #region properties

    public CelestialBodyInfoData Info => _celestialBodyInfo;

    #endregion

    #region fields
    SolarSystemController _solarSystemController;
    CelestialBodyInfoData _celestialBodyInfo;

    const float sunDiameter = 1392700;
    const float sunScale = .1f; // Make the sun smaller
    #endregion

    void Start()
    {
        InitSolarSystemController();
        SetBodyInfo();
    }

    // Handle planet rotation
    void FixedUpdate()
    {
        float rotationSeconds = 0.0f;

        if (_solarSystemController == null)
            rotationSeconds = rotationTime;
        else if (_solarSystemController.IsDemo)
            if (bodyType != CelestialBodyType.Sun)
                rotationSeconds = 30.0f;
            else
                rotationSeconds = RotationPeriod * 3600.0f;

        if (rotationSeconds > 0.0f)
        {
            var degreesPerSecond = 360.0f / rotationSeconds;
            transform.Rotate(new Vector3(0, degreesPerSecond * Time.fixedDeltaTime, 0));
        }

        //transform.Rotate(0, rotationSpeed, 0);
    }

    void OnValidate()
    {
        ApplyChanges();
    }

    /// <summary>
    /// Sets Celestialbody scale, rotation & orbit
    /// </summary>
    internal void ApplyChanges()
    {
        InitSolarSystemController();

        if (_solarSystemController == null)
            return;

        var trans = gameObject.transform;
        var scaleMultiplier = 0.0001f;

        if (!Application.isPlaying)
        {
            gameObject.name = bodyName.ToString();
            SetBodyInfo();

            var axialTilt = Quaternion.Euler(BodyAxialTilt, 0, 0);
            trans.rotation = axialTilt;
        }

        if (bodyType == CelestialBodyType.Sun)
        {
            diameter = sunDiameter;

            var sunSize = sunDiameter * sunScale * scaleMultiplier;
            trans.localScale = sunSize * Vector3.one;
        }
        else
        {
            var mltp = _solarSystemController != null ? _solarSystemController.GetPlanetScaleMultiplier(IsGiantPlanet()) : 1f;
            trans.localScale = diameter * mltp * scaleMultiplier * Vector3.one;
        }
    }

    public bool IsGiantPlanet()
    {
        CelestialBodyName[] array = { CelestialBodyName.Jupiter, CelestialBodyName.Saturn };
        return Array.Exists(array, e => e == bodyName);
    }

    void InitSolarSystemController()
    {
        if (_solarSystemController == null)
        {
            var solarSystem = GameObject.Find(Constants.SolarSystemMain);
            if (solarSystem != null)
                _solarSystemController = solarSystem.GetComponent<SolarSystemController>();
        }
    }

    // Set body type and creates the CelestialBodyInfo class used bij the UI Info Window.
    void SetBodyInfo()
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

        _celestialBodyInfo = new CelestialBodyInfoData()
        {
            bodyName = bodyName,
            bodyType = bodyType,
            description = description,
            diameter = diameter.ToString("N0", new CultureInfo("en-US")) + " km",
            gravity = gravity.ToString("N2", new CultureInfo("en-US")) + " g"
        };
    }
}