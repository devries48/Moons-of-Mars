using Cinemachine;
using System.Collections.Generic;

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

}
