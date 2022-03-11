using UnityEngine;

[ExecuteInEditMode]
public class SolarSystemManager : MonoBehaviour
{
    // Distance scale 0.1 = 1:100.000km, scale 1 = 1:1.000.000km
    [Header("Scale")]
    [Range(0.1f, 1f)]
    public float _distanceScale = 1f;

    // Planet diameter scale 0.1 = 1:10.000km, scale 1 = 1:1.000km
    [Range(0.1f, 1f)]
    public float _planetScale = 0.1f;

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

        ApplyChanges();
    }

}
