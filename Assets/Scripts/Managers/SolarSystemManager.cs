using UnityEngine;

[ExecuteInEditMode]
public class SolarSystemManager : MonoBehaviour
{
    public enum CelestialBodyName
    {
        Sun,
        Mercury,
        Venus,
        Earth,
        Moon,
        Mars,
        Jupiter,
        Saturn,
        Uranus,
        Neptune,
        Pluto
    }

    public enum CelestialBodyType { Planet, Moon, Sun }

    public enum OrbitActiveType { All, None, MoonsOnly}

    // Distance scale 0.1 = 1:100.000km, scale 1 = 1:1.000.000km
    [Header("Scale")]
    [Range(0.1f, 1f)]
    [SerializeField] float _distanceScale = 1f;

    // Planet diameter scale 0.1 = 1:10.000km, scale 1 = 1:1.000km
    [Range(0.1f, 1f)]
    public float _planetScale = 0.1f;

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

    public float PlanetScale
    {
        get { return _planetScale * 0.001f; }
        set { _planetScale = value; ApplyChanges(); }
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

    private void ApplyChanges()
    {
        var bodies = (CelestialBody[])FindObjectsOfType(typeof(CelestialBody));
        foreach (var body in bodies)
            body.ApplyChanges();
    }

    void OnValidate()
    {
        DistanceScale = _distanceScale;
        PlanetScale = _planetScale;
        RotationSpeed = _rotationSpeed;
        OrbitScale = _orbitScale;

        ApplyChanges();
    }

}
