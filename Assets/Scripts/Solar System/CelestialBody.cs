using System.Collections;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Rigidbody))]
public class CelestialBody : MonoBehaviour
{
    public enum BodyType { Planet, Moon, Sun, Test }

    #region public fields
    public string bodyName = "Unnamed";
    public BodyType bodyType;
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

    SolarSystemManager solarSystemManager;
    Ellipse orbitPath;

    [SerializeField]
    [Range(0, 1f)] float orbitProgress = 0f;
    float orbitPeriod = 0f;

    private const float physicsTimeStep = 0.01f;
    float sunScale = 139.27f;

    void Start()
    {
        solarSystemManager = GameObject.Find("Solar System Manager").GetComponent<SolarSystemManager>();
        if (Application.isPlaying && parentBody != null && orbitPeriod != 0)
        {
            SetOrbitingBodyPosition();
            StartCoroutine(AnimateOrbit());
        }
    }

    void FixedUpdate()
    {
        transform.Rotate((1 / RotationPeriod) * 1000 * Time.fixedDeltaTime * Vector3.up, Space.World);
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
        solarSystemManager = GameObject.Find("Solar System Manager").GetComponent<SolarSystemManager>();
        gameObject.name = bodyName;

        var trans = gameObject.transform;
        var axialTilt = Quaternion.Euler(BodyAxialTilt, 0, 0);
        var pos = new Vector3(sunScale + (distance * solarSystemManager.DistanceScale), 0, 0);

        trans.SetPositionAndRotation(pos, axialTilt);

        if (bodyType != BodyType.Sun)
            trans.localScale = diameter * solarSystemManager.PlanetScale * Vector3.one;

        if (parentBody != null)
        {
            float orbit = 0;

            if (bodyType == BodyType.Planet)
                orbit = distance * solarSystemManager.DistanceScale;

            if (bodyType == BodyType.Moon)
            {
                // Moons are automatically scaled by orbitting planet, scale the moon by (1/planetScale) e.g. parent scale is 2, child scale is 0.5
                //var scale = 1 / (solarSystemManager._planetScale * 10);

                orbit = (distance * solarSystemManager._planetScale * 10) + (parentBody.localScale.x / 2);

                //gameObject.transform.parent.localScale = Vector3.one * scale;
            }

            orbitPeriod = period * solarSystemManager.OrbitScale;
            orbitPath = new Ellipse(orbit, orbit);
            orbitProgress = orbitPeriod < 0 ? 1 : 0;

            SetOrbitingBodyPosition();
        }
    }

    void SetOrbitingBodyPosition()
    {
        Vector2 orbitPos = orbitPath.Evaluate(orbitProgress);
        Vector3 pos = new Vector3(orbitPos.x, 0, orbitPos.y);

        if (bodyType == BodyType.Moon)
        {
            pos += parentBody.transform.localPosition;
            Debug.Log("Pos: " + pos);
        }

        gameObject.transform.localPosition = pos;
    }

    IEnumerator AnimateOrbit()
    {
        float orbitSpeed = 1f / orbitPeriod;

        while (solarSystemManager.orbitActive == SolarSystemManager.OrbitActiveType.All
            || (solarSystemManager.orbitActive == SolarSystemManager.OrbitActiveType.MoonsOnly && bodyType == BodyType.Moon))
        {
            orbitProgress += Time.fixedDeltaTime * orbitSpeed;
            orbitProgress %= 1f;
            SetOrbitingBodyPosition();
            yield return null;
        }
    }
}