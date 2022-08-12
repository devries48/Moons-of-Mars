using System;
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
[DisallowMultipleComponent]
[RequireComponent(typeof(SolarSystemPanelController))]
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

    [SerializeField]
    [Tooltip("Display current solar system speed in this field.")]
    TMPro.TextMeshProUGUI _displaySpeedDescriptorField;


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

    DateTime _currentTime;

    GameManager GmManager => GameManager.Instance;

    #endregion

    #region fields

    readonly List<BodyTimeData> _bodies = new();
    DateTime _epochDate;
    int _currentSolarSystemSpeed;

    #endregion

    void Start()
    {
        SetCurrentGlobalTime();

        var instances = FindObjectsOfType<KeplerOrbitMover>();

        foreach (var item in instances)
            AddBody(item);

        _epochDate = new DateTime(_epochYear, _epochMonth, _epochDay, _epochHour, _epochMinute, 0, DateTimeKind.Utc);

    }

    void Update()
    {
        _currentTime = _currentTime.AddSeconds(GmManager.SolarSystemSpeed * Time.deltaTime);

        RefreshTimeDisplay();
    }

    void RefreshTimeDisplay()
    {
        _displayDateField.text = _currentTime.ToString("yyyy - MM - dd");
        _displayTimeField.text = _currentTime.ToString("HH : mm : ss");

        if (_currentSolarSystemSpeed != GmManager.SolarSystemSpeed)
        {
            _currentSolarSystemSpeed = GmManager.SolarSystemSpeed;
            _displayTimeField.enabled = GmManager.SolarSystemSpeed < Constants.SolarSystemSpeedWeek;
            _displaySpeedDescriptorField.enabled = GmManager.SolarSystemSpeed > 1;

            string period = "second";

            switch (GmManager.SolarSystemSpeed)
            {
                case Constants.SolarSystemSpeedHour:
                    period = "hour";
                    break;
                case Constants.SolarSystemSpeedDay:
                    period = "day";
                    break;
                case Constants.SolarSystemSpeedWeek:
                    period = "week";
                    break;
                default:
                    break;
            }

            _displaySpeedDescriptorField.text = $"1 second  = 1 {period}";
        }
    }

    void AddBody(KeplerOrbitMover b)
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

    void SetGlobalTime(DateTime time)
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
                item.body.ForceUpdateViewFromInternalState();
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
