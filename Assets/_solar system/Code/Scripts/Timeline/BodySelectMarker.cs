using System.ComponentModel;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using static MoonsOfMars.SolarSystem.SolarSystemController;

namespace MoonsOfMars.SolarSystem
{
    [DisplayName("BodySelectMarker")]
    public class BodySelectMarker : Marker, INotification, INotificationOptionProvider
    {
        [SerializeField] private CelestialBodyName _celestialBody;
        [SerializeField] private bool _isDeselect;

        [Space(10)]
        [SerializeField] private bool retroactive;
        [SerializeField] private bool emitOnce;
        [SerializeField] private bool emitInEditor;

        public PropertyName id => new();
        public CelestialBodyName CelestialBody => _celestialBody;
        public bool IsDeselect => _isDeselect;

        public NotificationFlags flags =>
            (retroactive ? NotificationFlags.Retroactive : default) |
            (emitOnce ? NotificationFlags.TriggerOnce : default) |
            (emitInEditor ? NotificationFlags.TriggerInEditMode : default);

    }
}