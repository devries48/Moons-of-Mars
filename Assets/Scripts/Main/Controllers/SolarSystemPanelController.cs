using Cinemachine;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static SolarSystemController;

[DisallowMultipleComponent]
[SelectionBase]
[RequireComponent(typeof(MenuController))]
public class SolarSystemPanelController : MonoBehaviour
{
    #region editor fields

    [Header("UI Elements")]
    [SerializeField] GameObject _controlPanel;
    [SerializeField] Slider _slideZoom;
    [SerializeField] Slider _slideRotateVertical;
    [SerializeField] Slider _slideRotateHorizontal;
    [SerializeField] Slider _slideCenter;
    [SerializeField] Slider _slideSpeed;

    [SerializeField] TextMeshProUGUI _centerText;
    [SerializeField] GameObject _speedIcon;

    [Header("Images")]
    [SerializeField] Sprite[] _centerIcons;
    [SerializeField] Image _centerImage;
    #endregion

    #region properties

    GameManager GameManager => GameManager.Instance;

    // Get the parent of the camera for the rotation.
    GameObject CamPivot
    {
        get
        {
            if (__camPivot == null)
                __camPivot = GameManager.SolarSystemCamera.gameObject.transform.parent.gameObject;

            return __camPivot;
        }
    }
    GameObject __camPivot;
    #endregion

    #region fields
    float m_rotateIconSpeed = 5;

    readonly float m_maxZoomSolarCamDist = 4000f;
    readonly float m_maxZoomStepValue = 9f;
    CelestialBody m_followBody;

    #endregion


    void Update()
    {
        _speedIcon.transform.Rotate(new Vector3(0, 0, -1), Time.deltaTime * m_rotateIconSpeed);
    }

    /// <summary>
    /// Rotate the panel into view.
    /// </summary>
    public void ShowControlPanel()
    {
        this.enabled = true;

        // Pivot y from 0 to -0.1 rotate x from -90 to 15 
        MenuController.TweenPivot(_controlPanel, new Vector2(0.5f, -.1f), new Vector3(15, 0, 0), LeanTweenType.easeOutQuint, 1f, LeanTweenType.easeInQuad, GameManager.CameraSwitchTime);
    }

    /// <summary>
    /// Rotate the panel out of view.
    /// </summary>
    public void HideControlPanel(bool animate = false)
    {
        this.enabled = false;

        ResetCamRotation();

        _slideZoom.value = _slideZoom.minValue;
        _slideCenter.value = _slideCenter.minValue;
        _slideSpeed.value = _slideSpeed.minValue;

        m_rotateIconSpeed = 5f;

        if (animate)
            MenuController.TweenPivot(_controlPanel, new Vector2(0.5f, 0f), new Vector3(-90, 0, 0), LeanTweenType.easeInQuint, .5f, LeanTweenType.easeOutQuad, 1f);
        else
            MenuController.TweenPivot(_controlPanel, new Vector2(0.5f, 0f), new Vector3(-90, 0, 0));
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
        var trans = GameManager.SolarSystemCamera.Follow.transform;

        CamPivot.transform.RotateAround(trans.position, trans.up, value * 30);
    }

    public void SolarSystemCenter(System.Single value)
    {
        _centerImage.sprite = _centerIcons[(int)value];
        _centerText.text = _centerImage.sprite.name.ToString().ToLower();

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

        if (m_followBody == GameManager.CelestialBody(body))
            return;

        m_followBody = GameManager.CelestialBody(body);

        ResetCamRotation();

        GameManager.SolarSystemCamera.transform.localRotation = Quaternion.Euler(30f + m_followBody.BodyAxialTilt, 0, 0);
        GameManager.SolarSystemCamera.Follow = m_followBody.transform;
    }

    public void SolarSystemSpeed(System.Single value)
    {
        switch (value)
        {
            case 2: // 1 second =  1 hour
                m_rotateIconSpeed = 20f;
                GameManager.SolarSystemSpeed = Constants.SolarSystemSpeedHour;
                break;
            case 3: // 1 second =  1 day
                m_rotateIconSpeed = 40f;
                GameManager.SolarSystemSpeed = Constants.SolarSystemSpeedDay;
                break;
            case 4: // 1 second =  1 week
                m_rotateIconSpeed = 80f;
                GameManager.SolarSystemSpeed = Constants.SolarSystemSpeedWeek;
                break;
            case 1:
            default:
                m_rotateIconSpeed = 5f;
                GameManager.SolarSystemSpeed = 1;
                break;
        }
    }

    void ResetCamRotation()
    {
        _slideRotateVertical.value = _slideRotateVertical.minValue;
        _slideRotateHorizontal.value = _slideRotateHorizontal.minValue;
        CamPivot.transform.localRotation = Quaternion.Euler(0,0,0);

        GameManager.SolarSystemCamera.transform.localRotation = Quaternion.Euler(30, 0, 0);
    }

    float ZoomEaseInCubic(float value)
    {
        if (value == 0f) return 0f;

        // p = percentage/100 from value of _maxZoomStepValue
        var p = value / m_maxZoomStepValue;

        return (p * p * p) * m_maxZoomSolarCamDist;
    }
}
