using UnityEngine;

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

    // Distance scale 0.1 = 1:100.000km, scale 1 = 1:1.000.000km
    [Header("Scale")]
    [Range(0.1f, 1f)]
    [SerializeField] float _distanceScale = 1f;

    // Planet diameter scale 1 = 1unity:10,000km, scale 10 = 1unity:1,000km
    [Range(1, 10)]
    [SerializeField] private float _planetScaleMultiplier = 1f;

    // Planet rotation scale 0.1 = 1 sec, scale 1 = 1 hour
    [Range(1, 10)]
    [SerializeField] int _rotationSpeed = 1;

    // Orbit period scale 0.1 = 1 minute, scale 1 = 1 day
    [Range(0.1f, 1f)]
    [SerializeField] float _orbitScale = 1f;

    public OrbitActiveType orbitActive;
    internal bool isDemo = true;

    public float DistanceScale
    {
        get { return _distanceScale; }
        set { _distanceScale = value; ApplyChanges(); }
    }

    public float PlanetScaleMultiplier
    {
        get { return _planetScaleMultiplier; }
        set { _planetScaleMultiplier = value; ApplyChanges(); }
    }

    public int RotationSpeed
    {
        get { return _rotationSpeed * 100; }
        set { _rotationSpeed = value; ApplyChanges(); }
    }

    public float OrbitScale
    {
        get { return _orbitScale * 10; }
        set { _orbitScale = value; ApplyChanges(); }
    }

    /// <summary>
    /// Apply the changes to all celestial bodies.
    /// </summary>
    private void ApplyChanges()
    {
        var bodies = (CelestialBody[])FindObjectsOfType(typeof(CelestialBody));
        foreach (var body in bodies)
            body.ApplyChanges();
    }

    private void OnValidate()
    {
        name = Constants.SolarSystemMain;

        DistanceScale = _distanceScale;
        PlanetScaleMultiplier = _planetScaleMultiplier;
        RotationSpeed = _rotationSpeed;
        OrbitScale = _orbitScale;

        ApplyChanges();
    }

}
