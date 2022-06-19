using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class StarRenderingController : MonoBehaviour
{
    [SerializeField]
    SolarSystem.StarRenderer _starRenderer;

    CommandBuffer outerSpaceRenderCommand;

    Camera MainCamera
    {
        get
        {
            if (__mainCamera == null)
                TryGetComponent<Camera>(out __mainCamera);

            return __mainCamera;
        }
    }
    Camera __mainCamera;

    void OnEnable()
    {
        Setup();
    }

    void Setup()
    {
        MainCamera.RemoveAllCommandBuffers();

        outerSpaceRenderCommand = new CommandBuffer
        {
            name = "Outer Space Render"
        };

        if (_starRenderer != null)
            _starRenderer.SetUpStarRenderingCommand(outerSpaceRenderCommand, MainCamera);

        MainCamera.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, outerSpaceRenderCommand);
    }

    void OnDisable()
    {
        outerSpaceRenderCommand?.Release();
    }

}
