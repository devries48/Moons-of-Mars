using Cinemachine;
using System.Collections.Generic;
using UnityEngine;

public static class CameraSwitcher
{
    public static CinemachineVirtualCamera ActiveCamera = null;

    private static readonly List<CinemachineVirtualCamera> _cameras = new List<CinemachineVirtualCamera>();

    public static void SwitchCamera(CinemachineVirtualCamera camera)
    {
        if (camera == ActiveCamera)
            return;

        camera.Priority = 10;
        ActiveCamera = camera;

        foreach (CinemachineVirtualCamera c in _cameras)
        {
            if (c != camera && c.Priority != 0)
                c.Priority = 0;
        }
    }

    public static void Register(CinemachineVirtualCamera camera)
    {
        _cameras.Add(camera);
    }

    public static void Unregister(CinemachineVirtualCamera camera)
    {
        _cameras.Remove(camera);
    }

    // Turn on the bit using an OR operation:
    public static void ShowLinesLayer(Camera camera)

    {
        camera.cullingMask |= 1 << LayerMask.NameToLayer(Constants.OrbitLineLayer);
    }

    // Turn off the bit using an AND operation with the complement of the shifted int:
    public static void HideLinesLayer(Camera camera)
    {
        camera.cullingMask &= ~(1 << LayerMask.NameToLayer(Constants.OrbitLineLayer));
    }

}
