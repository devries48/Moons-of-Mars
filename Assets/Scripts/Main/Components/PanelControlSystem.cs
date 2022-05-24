using Cinemachine;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(MenuController))]
public class PanelControlSystem :MonoBehaviour
{
    [SerializeField] Sprite[] centerIcons;

    [SerializeField] Image centerImage;
    [SerializeField] TextMeshProUGUI centerText;

    private readonly float _maxZoomSolarCamDist = 4000f;
    private readonly float _maxZoomStepValue = 9f;

    #region properties

    GameManager GameManager => GameManager.Instance;
    
    #endregion


    public void SolarSystemZoom(System.Single value)
    {
        CinemachineComponentBase componentBase = GameManager.SolarSystemCamera.GetCinemachineComponent(CinemachineCore.Stage.Body);
        if (componentBase is CinemachineFramingTransposer)
        {
            var distance = 100f + ZoomEaseInCubic(value);
            (componentBase as CinemachineFramingTransposer).m_CameraDistance = distance;
        }
    }

    /// <summary>
    /// Rotate Solar-System around the x-axis between 15° and 90°.
    /// </summary>
    /// <param name="rotate"></param>
    public void SolarSystemRotateVertical(System.Single value)
    {
        // Get the parent of the camera for the rotation.
        var camPivot = GameManager.SolarSystemCamera.gameObject.transform.parent.gameObject;

        LeanTween.rotateX(camPivot, value * 15, 0f);
    }

    /// <summary>
    /// Rotate Solar-System around the y-axis between 0° and 360°.
    /// </summary>
    /// <param name="rotate"></param>
    public void SolarSystemRotateHorizontal(System.Single value)
    {
        // Get the parent of the camera for the rotation.
        var camPivot = GameManager.SolarSystemCamera.gameObject.transform.parent.gameObject;

        LeanTween.rotateY(camPivot, value * 30, 0f);
    }

    public void SolarSystemCenter(System.Single value)
    {
        centerImage.sprite = centerIcons[(int)value];
        centerText.text = centerImage.sprite.name.ToString().ToLower();
        
    }


    private float ZoomEaseInCubic(float value)
    {
        if (value == 0f) return 0f;

        // p = percentage/100 from value of _maxZoomStepValue
        var p = value / _maxZoomStepValue;

        return (p * p * p) * _maxZoomSolarCamDist;
    }
}
