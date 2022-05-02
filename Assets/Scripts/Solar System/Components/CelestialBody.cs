using System.Globalization;
using UnityEngine;
using static SolarSystemController;

[ExecuteInEditMode]
[RequireComponent(typeof(Rigidbody))]
public class CelestialBody : MonoBehaviour
{
    #region public fields
    public CelestialBodyName bodyName;
    public CelestialBodyType bodyType;

    [Tooltip("")]
    [Header("Mean distance from sun/planet in 10^6 km")]
    public float distance;
    [Header("Diameter in km")]
    public float diameter;
    [Header("Surface gravity in g")]
    public float gravity;
    [Header("Orbital period in days")]
    public float period;
    [Header("Tilt in degrees")]
    public float BodyAxialTilt;
    [Header("Rotation period in hours")]
    public float RotationPeriod;
    #endregion

    private SolarSystemController _solarSystemController;
    private CelestialBodyInfoData _celestialBodyInfo;

    public CelestialBodyInfoData Info => _celestialBodyInfo;

    const float sunDiameter = 1392700;
    const float sunScale = .1f; // Make the sun smaller

    void Start()
    {
        InitSolarSystemController();
        SetBodyInfo();
    }

    /// <summary>
    /// Handle planet rotation
    /// </summary>
    void FixedUpdate()
    {
        var rotationSpeed = _solarSystemController.isDemo && bodyType != CelestialBodyType.Sun
            ? 30 * Time.fixedDeltaTime
            : (1 / RotationPeriod) * 1000 * Time.fixedDeltaTime;

        transform.Rotate(0, rotationSpeed, 0);
    }

    void OnValidate()
    {
        ApplyChanges();
    }

    /// <summary>
    /// Sets Celestialbody scale, rotation & orbit
    /// </summary>
    public void ApplyChanges()
    {
        InitSolarSystemController();

        var trans = gameObject.transform;
        var scaleMultiplier = 0.0001f;

        if (!Application.isPlaying)
        {
            gameObject.name = bodyName.ToString();
            SetBodyInfo();

            var axialTilt = Quaternion.Euler(BodyAxialTilt, 0, 0);
            trans.rotation = axialTilt;
            //trans.SetPositionAndRotation(Vector3.zero, axialTilt);
        }

        if (bodyType == CelestialBodyType.Sun)
        {
            diameter = sunDiameter;

            var sunSize = sunDiameter * sunScale * scaleMultiplier;
            trans.localScale = sunSize * Vector3.one;
        }
        else
        {
            var mltp = _solarSystemController != null ? _solarSystemController.PlanetScaleMultiplier : 1;
            trans.localScale = diameter * mltp * scaleMultiplier * Vector3.one;
        }
    }

    private void InitSolarSystemController() //make property
    {
        if (_solarSystemController == null)
        {
            var solarSystem = GameObject.Find(Constants.SolarSystemMain);
            if (solarSystem != null)
                _solarSystemController = solarSystem.GetComponent<SolarSystemController>();
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