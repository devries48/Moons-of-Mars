using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple orbits time controller.
/// Allows to change current time for all orbits on scene.
/// </summary>
/// <remarks>
/// Only uses one epoch for orbits state calculations
/// which is fine enough for demonstration purposes.
/// </remarks>
public class KeplerTimeController : MonoBehaviour
{
    struct BodyTimeData
    {
        public KeplerOrbitMover body;
        public double initialMeanAnomaly;
    }

    #region editor fields

    [SerializeField]
    [Tooltip("Display current solar system date in this field.")]
     TMPro.TextMeshProUGUI _displayDateField;

    [SerializeField]
    [Tooltip("Display current solar system time in this field.")]
    TMPro.TextMeshProUGUI _displayTimeField;

    [Header("Epoch origin timestamp")]
    [SerializeField]
    int _epochYear;

    [SerializeField]
    int _epochMonth;

    [SerializeField]
    int _epochDay;

    [SerializeField]
    int _epochHour;

    [SerializeField]
    int _epochMinute;

    #endregion

    #region properties

    public DateTime CurrentTime
    {
        get { return _currentTime; }
    }
    private DateTime _currentTime;

    GameManager GameManager => GameManager.Instance;

    #endregion

    #region fields

    private readonly List<BodyTimeData> _bodies = new();
    private DateTime _epochDate;
    #endregion

    private void Awake()
    {
        var instances = FindObjectsOfType<KeplerOrbitMover>();

        foreach (var item in instances)
            AddBody(item);

        _epochDate = new DateTime(_epochYear, _epochMonth, _epochDay, _epochHour, _epochMinute, 0, DateTimeKind.Utc);
    }

    private void Start()
    {
        SetCurrentGlobalTime();
    }

    private void Update()
    {
        _currentTime = _currentTime.AddSeconds(GameManager.SolarSystemSpeed * Time.deltaTime);

        RefreshTimeDisplay();
    }

    private void RefreshTimeDisplay()
    {
        _displayDateField.text = _currentTime.ToString("yyyy - MM - dd");
        _displayTimeField.text = _currentTime.ToString("HH : mm : ss");
    }

    private void AddBody(KeplerOrbitMover b)
    {
        // Body's initial mean anomaly is taken as origin point for the epoch.
        // And the origin timestamp for the epoch is defined in this component's parameters.
        _bodies.Add(new BodyTimeData()
        {
            body = b,
            initialMeanAnomaly = b.OrbitData.MeanAnomaly
        });
    }

    public void SetCurrentGlobalTime()
    {
        SetGlobalTime(DateTime.UtcNow);
    }

    public void SetGlobalTime(DateTime time)
    {
        bool isAnyNull = false;

        _currentTime = time;
        var elapsedTime = (time - _epochDate).TotalSeconds;
        
        foreach (var item in _bodies)
        {
            if (item.body == null)
            {
                isAnyNull = true;
                continue;
            }

            var value = item.initialMeanAnomaly + elapsedTime * item.body.OrbitData.MeanMotion;
            item.body.OrbitData.SetMeanAnomaly(value);
            if (item.body.AttractorSettings.AttractorObject != null)
            {
                item.body.ForceUpdateViewFromInternalState();
            }
        }

        if (isAnyNull)
        {
            static bool Predicate(BodyTimeData b)
            {
                return b.body == null;
            }

            _bodies.RemoveAll(Predicate);
        }
    }
}
