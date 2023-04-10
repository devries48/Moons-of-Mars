using UnityEngine;
using UnityEngine.Playables;

namespace MoonsOfMars.SolarSystem
{
    public class BodySelectReceiver : MonoBehaviour, INotificationReceiver
    {
        [SerializeField] private MenuManager introManager;

        public void OnNotify(Playable origin, INotification notification, object context)
        {
            var bodySelectMarker = notification as BodySelectMarker;
            if (bodySelectMarker == null && introManager != null) return;

            introManager.ShowBodyInfoWindow(bodySelectMarker.CelestialBody, bodySelectMarker.IsDeselect);
        }
    }
}