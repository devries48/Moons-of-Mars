using UnityEngine;
using UnityEngine.Playables;

public class BodySelectReceiver : MonoBehaviour, INotificationReceiver
{
    [SerializeField] private IntroManager introManager;

    public void OnNotify(Playable origin, INotification notification, object context)
    {
        var bodySelectMarker = notification as BodySelectMarker;
        if (bodySelectMarker == null && introManager != null) return;

        introManager.ShowBodyInfoWindow(bodySelectMarker.CelestialBody, bodySelectMarker.IsDeselect);
    }
}
