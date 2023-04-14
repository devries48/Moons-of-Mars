using Cinemachine;
using System.Collections.Generic;

namespace MoonsOfMars.Shared
{
    /// <summary>
    /// Switch Cinemachine camera's
    /// </summary>
    /// <example>
    ///     CameraSwitcher.Register(_MenuCamera);
    ///     CameraSwitcher.Register(_SolarSystemCamera);
    ///     
    ///     CameraSwitcher.SwitchCamera(GmManager.SolarSystemCamera);
    /// 
    ///     CameraSwitcher.Unregister...
    /// </example>
    public static class CameraSwitcher
    {
        public static CinemachineVirtualCamera ActiveCamera = null;

        static readonly List<CinemachineVirtualCamera> _cameras = new();

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
}