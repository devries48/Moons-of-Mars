using System.Collections;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Rigidbody))]
public class CelestialBody : MonoBehaviour
{
    public enum BodyType { Planet, Moon, Sun }

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

    private SolarSystemManager solarSystemManager;

    [Range(0, 1f), SerializeField] float orbitProgress = 0f;
    bool orbitActive;
    float orbitPeriod;
    Ellipse orbitPath;

    readonly float physicsDeltaTime = .0001f;

    readonly float orbitPeriodScale = 1f;           // Period   = 
    readonly float rotationPeriodScale = 1f;     // Period   = 1 hour: 0.1 second

    void Start()
    {
        if (Application.isPlaying && parentBody != null && orbitPeriod != 0)
        {
            orbitActive = true;

            SetOrbitingBodyPosition();
            StartCoroutine(AnimateOrbit());
        }
    }

    void FixedUpdate()
    {
        transform.Rotate(0, 360 / ((RotationPeriod) * rotationPeriodScale) * Time.fixedDeltaTime, 0, Space.Self);
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
        var axialTilt = Quaternion.Euler(0, BodyAxialTilt, 0);
        var pos = new Vector3(distance * solarSystemManager.DistanceScale, 0, 0);

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
                // Moons are automatically scaled by the planets, scale the moon by (1/planetScale).
                // (e.g. parent scale is 2, child scale is 0.5)
                var scale = 1 / (solarSystemManager._planetScale * 10);
                gameObject.transform.parent.localScale = Vector3.one * scale;

                orbit = (distance * solarSystemManager._planetScale * 10)
                  + (parentBody.localScale.x / 2)
                  + (gameObject.transform.localScale.x / 2);
            }

            orbitPeriod = period * orbitPeriodScale;
            orbitPath = new Ellipse(orbit, orbit);
            orbitProgress = orbitPeriod < 0 ? 1 : 0;

            SetOrbitingBodyPosition();
        }
    }

    void SetOrbitingBodyPosition()
    {
        Vector2 orbitPos = orbitPath.Evaluate(orbitProgress);
        gameObject.transform.localPosition = new Vector3(orbitPos.x, 0, orbitPos.y);
    }

    IEnumerator AnimateOrbit()
    {
        float orbitSpeed = 1f / orbitPeriod;

        while (orbitActive)
        {
            orbitProgress += Time.fixedDeltaTime * orbitSpeed;
            orbitProgress %= 1f;
            SetOrbitingBodyPosition();
            yield return null;
        }
    }
}