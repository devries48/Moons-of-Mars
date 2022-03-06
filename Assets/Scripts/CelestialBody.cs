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

    [Range(0, 1f)]
    public float orbitProgress = 0f;
    bool orbitActive;
    float orbitPeriod;
    Ellipse orbitPath;

    readonly float physicsDeltaTime = .0001f;
    readonly float planetScale = .0001f;            // Diameter = 1:10,000 (1 unity meter = 10.000 km)
    readonly float planetDistanceScale = .1f;       // Distance = 1:10 (planets distance to sun)
    readonly float orbitPeriodScale = 1f;           // Period   = 
    readonly float rotationPeriodScale = 1f;     // Period   = 1 hour: 0.1 second

    private void Start()
    {
        if (Application.isPlaying && parentBody != null && orbitPeriod != 0)
        {
            orbitActive = true;

            SetOrbitingBodyPosition();
            StartCoroutine(AnimateOrbit());
        }
    }

    private void FixedUpdate()
    {
        transform.Rotate(0, 360 / ((RotationPeriod) * rotationPeriodScale) * Time.fixedDeltaTime, 0, Space.Self);
    }

    void OnValidate()
    {
        var trans = gameObject.transform;
        var axialTilt = Quaternion.Euler(0, BodyAxialTilt, 0);
        var pos = new Vector3(distance, 0, 0);

        gameObject.name = bodyName;
        trans.localScale = diameter * planetScale * Vector3.one;
        trans.SetPositionAndRotation(pos, axialTilt);

        // Set the sun's scale based on planetDistanceScale
        if (bodyType == BodyType.Sun)
        {
            trans.localScale *= planetDistanceScale;
        }

        if (parentBody != null)
        {
            float orbit = 0;

            if (bodyType == BodyType.Planet)
                orbit = distance * planetDistanceScale;
            if (bodyType == BodyType.Moon)
                orbit = orbit + (trans.localScale.x / 2) + (parentBody.localScale.x / 2);

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