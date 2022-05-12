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

    // Planet diameter scale 1 = 1unity:10,000km, scale 10 = 1unity:1,000km
    [Range(1, 10)]
    [SerializeField] private float _planetScaleMultiplier = 1f;

    public OrbitActiveType orbitActive;

    internal bool IsDemo = true;
    internal bool OrbitLinesVisible = false;

    private KeplerOrbitLinesController _orbitLinesController;

    // Giant planets scale only 1/10 of the planets and moons.
    public float GetPlanetScaleMultiplier(bool isGiantPlanet)
    {
        if (isGiantPlanet)
            return 1f - 0.1f + (_planetScaleMultiplier / 10f);

        return _planetScaleMultiplier;
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
