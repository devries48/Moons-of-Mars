using Cinemachine;
using Game.Asteroids;
using UnityEngine;

public class SetStageSettings : MonoBehaviour
{
    [SerializeField] Light sun;
    [SerializeField] CinemachineVirtualCamera stageBackgroundCamera;

    void Awake()
    {
        RenderSettings.sun = sun;

        var t = stageBackgroundCamera.transform;
        AsteroidsGameManager.GmManager.m_BackgroundCamera.transform
            .SetPositionAndRotation(t.position, t.rotation);
    }
}
