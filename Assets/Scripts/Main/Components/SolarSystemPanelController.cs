using Cinemachine;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static SolarSystemController;

[DisallowMultipleComponent]
[RequireComponent(typeof(MenuController))]
public class SolarSystemPanelController : MonoBehaviour
{
    #region editor fields
    [Header("UI Elements")]
    [SerializeField] GameObject controlPanel;
    [SerializeField] TextMeshProUGUI centerText;
    [SerializeField] GameObject speedIcon;

    [Header("Images")]
    [SerializeField] Sprite[] centerIcons;
    [SerializeField] Image centerImage;

    readonly float _maxZoomSolarCamDist = 4000f;
    readonly float _maxZoomStepValue = 9f;
    GameObject _followObj;
    #endregion

    #region properties

    GameManager GameManager => GameManager.Instance;

    #endregion

    float _rotateIconSpeed = 5;

    void Update()
    {
        speedIcon.transform.Rotate(new Vector3(0, 0, -1), Time.deltaTime * _rotateIconSpeed);
    }

    /// <summary>
    /// Rotate the panel into view.
    /// </summary>
    public void ShowControlPanel()
    {
        this.enabled = true;

        // Pivot y from 0 to -0.1 rotate x from -90 to 15 
        MenuController.TweenPivot(controlPanel, new Vector2(0.5f, -.1f), new Vector3(15, 0, 0), LeanTweenType.easeOutQuint, 1f, LeanTweenType.easeInQuad, GameManager.CameraSwitchTime);
    }

    /// <summary>
    /// Rotate the panel out of view.
    /// </summary>
    public void HideControlPanel(bool animate = false)
    {
        this.enabled = false;

        _rotateIconSpeed = 5f;
        GameManager.SolarSystemSpeed = 1;

        if (animate)
            MenuController.TweenPivot(controlPanel, new Vector2(0.5f, 0f), new Vector3(-90, 0, 0), LeanTweenType.easeInQuint, .5f, LeanTweenType.easeOutQuad, 1f);
        else
            MenuController.TweenPivot(controlPanel, new Vector2(0.5f, 0f), new Vector3(-90, 0, 0));
    }

    /// <summary>
    /// Zoom the Solar-System in or out.
    /// </summary>
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
    public void SolarSystemRotateVertical(System.Single value)
    {
        // Get the parent of the camera for the rotation.
        var camPivot = GameManager.SolarSystemCamera.gameObject.transform.parent.gameObject;

        LeanTween.rotateX(camPivot, value * 15, 0f);
    }

    /// <summary>
    /// Rotate Solar-System around the y-axis between 0° and 360°.
    /// </summary>
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

        CelestialBodyName body = CelestialBodyName.Sun;

        switch (value)
        {
            case 1:
                body = CelestialBodyName.Jupiter;
                break;
            case 2:
                body = CelestialBodyName.Saturn;
                break;
            default:
                break;
        }

        _followObj = GameManager.CelestialBody(body).gameObject;
        GameManager.SolarSystemCamera.Follow = _followObj.transform.parent.transform;
    }

    public void SolarSystemSpeed(System.Single value)
    {
        switch (value)
        {
            case 2: // 1 second =  1 hour
                _rotateIconSpeed = 20f;
                GameManager.SolarSystemSpeed = 60 * 60;
                break;
            case 3: // 1 second =  1 day
                _rotateIconSpeed = 40f;
                GameManager.SolarSystemSpeed = 60 * 60 * 24;
                break;
            case 4: // 1 second =  1 week
                _rotateIconSpeed = 80f;
                GameManager.SolarSystemSpeed = 60 * 60 * 24 * 7;
                break;
            case 1:
            default:
                _rotateIconSpeed = 5f;
                GameManager.SolarSystemSpeed = 1;
                break;
        }
    }


    private float ZoomEaseInCubic(float value)
    {
        if (value == 0f) return 0f;

        // p = percentage/100 from value of _maxZoomStepValue
        var p = value / _maxZoomStepValue;

        return (p * p * p) * _maxZoomSolarCamDist;
    }
}
