using Cinemachine;
using MoonsOfMars.Shared;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static MoonsOfMars.SolarSystem.SolarSystemController;

namespace MoonsOfMars.SolarSystem
{
    [DisallowMultipleComponent]
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

        GameManager GmManager => GameManager.Instance;

        // Get the parent of the camera for the rotation.
        GameObject CamPivot
        {
            get
            {
                if (__camPivot == null)
                    __camPivot = GmManager.SolarSystemCamera.gameObject.transform.parent.gameObject;

                return __camPivot;
            }
        }
        GameObject __camPivot;

        KeplerTimeController TimeController
        {
            get
            {
                if (__timeController == null)
                    __timeController = GetComponentInParent<KeplerTimeController>();

                return __timeController;
            }

        }
        KeplerTimeController __timeController;

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
            enabled = true;

            SolarSystemReset();
            // Pivot y from 0 to -0.1 rotate x from -90 to 15 
            TweenUtil.TweenPivot(_controlPanel, new Vector2(0.5f, -.1f), new Vector3(15, 0, 0), LeanTweenType.easeOutQuint, 1f, LeanTweenType.easeInQuad, GmManager.CameraSwitchTime);
        }

        /// <summary>
        /// Rotate the panel out of view.
        /// </summary>
        public void HideControlPanel(bool animate = false)
        {
            enabled = false;

            _slideSpeed.value = _slideSpeed.minValue;

            if (animate)
                TweenUtil.TweenPivot(_controlPanel, new Vector2(0.5f, 0f), new Vector3(-90, 0, 0), LeanTweenType.easeInQuint, .5f, LeanTweenType.easeOutQuad, 1f);
            else
                TweenUtil.TweenPivot(_controlPanel, new Vector2(0.5f, 0f), new Vector3(-90, 0, 0));
        }

        /// <summary>
        /// Zoom the Solar-System in or out.
        /// </summary>
        public void SolarSystemZoom(float value)
        {
            CinemachineComponentBase componentBase = GmManager.SolarSystemCamera.GetCinemachineComponent(CinemachineCore.Stage.Body);
            if (componentBase is CinemachineFramingTransposer)
            {
                var distance = 100f + ZoomEaseInCubic(value);
                (componentBase as CinemachineFramingTransposer).m_CameraDistance = distance;
            }
        }

        /// <summary>
        /// Rotate Solar-System around the x-axis between 15° and 90°.
        /// </summary>
        public void SolarSystemRotateVertical(float value)
        {
            // Get the parent of the camera for the rotation.
            var camPivot = GmManager.SolarSystemCamera.gameObject.transform.parent.gameObject;

            LeanTween.rotateX(camPivot, value * 15, 0f);
        }

        /// <summary>
        /// Rotate Solar-System around the y-axis between 0° and 360°.
        /// </summary>
        public void SolarSystemRotateHorizontal(float value)
        {
            //var trans = GmManager.SolarSystemCamera.Follow.transform;
            print( value);
            //CamPivot.transform.RotateAround(trans.position, trans.up, value * 30);
            var camPivot = GmManager.SolarSystemCamera.gameObject.transform.parent.gameObject;

            LeanTween.rotateY(camPivot, value * 30f, 0f);
        }

        public void SolarSystemCenter(float value)
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

            if (m_followBody == GmManager.CelestialBody(body))
                return;

            m_followBody = GmManager.CelestialBody(body);

            ResetCamRotation();

            GmManager.SolarSystemCamera.transform.localRotation = Quaternion.Euler(30f + m_followBody.BodyAxialTilt, 0, 0);
            GmManager.SolarSystemCamera.Follow = m_followBody.transform;
        }

        public void SolarSystemSpeed(float value)
        {
            switch (value)
            {
                case 2: // 1 second =  1 hour
                    m_rotateIconSpeed = 20f;
                    GmManager.SolarSystemSpeed = Constants.SolarSystemSpeedHour;
                    break;
                case 3: // 1 second =  1 day
                    m_rotateIconSpeed = 40f;
                    GmManager.SolarSystemSpeed = Constants.SolarSystemSpeedDay;
                    break;
                case 4: // 1 second =  1 week
                    m_rotateIconSpeed = 80f;
                    GmManager.SolarSystemSpeed = Constants.SolarSystemSpeedWeek;
                    break;
                case 1:
                default:
                    m_rotateIconSpeed = 5f;
                    GmManager.SolarSystemSpeed = 1;
                    break;
            }
        }

        public void SolarSystemReset()
        {
            _slideSpeed.value = _slideSpeed.minValue;
            TimeController.SetCurrentGlobalTime();
        }

        void ResetCamRotation()
        {
            _slideRotateVertical.value = _slideRotateVertical.minValue;
            _slideRotateHorizontal.value = _slideRotateHorizontal.minValue;
            CamPivot.transform.localRotation = Quaternion.Euler(0, 0, 0);

            GmManager.SolarSystemCamera.transform.localRotation = Quaternion.Euler(30, 0, 0);
        }

        float ZoomEaseInCubic(float value)
        {
            if (value == 0f) return 0f;

            // p = percentage/100 from value of _maxZoomStepValue
            var p = value / m_maxZoomStepValue;

            return p * p * p * m_maxZoomSolarCamDist;
        }
    }
}