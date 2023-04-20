using UnityEngine;
using UnityEngine.Rendering;

namespace MoonsOfMars.Game.Asteroids
{
    public class LightsManager : MonoBehaviour
    {
        #region editor fields
        [SerializeField] Volume volume;

        [Header("Night Lighting")]
        [SerializeField] Light lightDefault;
        [SerializeField] Color nightColor;
        [SerializeField] float nightLightIntensity;

        public Camera m_lightCheckCamera;

        [SerializeField, Tooltip("Select from 'Light Camera'")]
        LightCheckController lightCheckController;

        [SerializeField, Tooltip("Switch on below threshold")]
        int lightLevelThreshold;
        #endregion

        #region properties
        AsteroidsGameManager GameManager
        {
            get
            {
                if (__gameManager == null)
                    __gameManager = AsteroidsGameManager.GmManager;

                return __gameManager;
            }
        }
        AsteroidsGameManager __gameManager;
        #endregion

        Color _dayColor;
        float _dayLightIntensity;

        UnityEngine.Rendering.Universal.DepthOfField VolDepthOfField
        {
            get
            {
                if (__volDepthOfField == null)
                {
                    if (!volume.profile.TryGet(out __volDepthOfField)) throw new System.NullReferenceException(nameof(VolDepthOfField));
                }
                return __volDepthOfField;
            }
        }
        UnityEngine.Rendering.Universal.DepthOfField __volDepthOfField;


        void OnEnable()
        {
            _dayColor = lightDefault.color;
            _dayLightIntensity = lightDefault.intensity;

            if (lightCheckController)
                lightCheckController.OnLevelChanged += LevelChanged;
        }

        void OnDisable()
        {
            if (lightCheckController)
                lightCheckController.OnLevelChanged -= LevelChanged;
        }

        public void SetLightCheckOffset(float offset)
        {
            var p = m_lightCheckCamera.transform.position;
            m_lightCheckCamera.transform.position = new Vector3(offset, p.y, p.z);
        }

        public void BlurBackground(bool blur)
        {
            if (blur)
            {
                VolDepthOfField.active = true;
                VolDepthOfField.focalLength.Override(300);
            }
            else
            {
                VolDepthOfField.focalLength.Override(50);
                VolDepthOfField.active = false;
            }
        }

        void LevelChanged(int level)
        {
            if (level == 0)
                return;

            if (level < lightLevelThreshold && lightDefault.color == _dayColor)
            {
                TweenColor(_dayColor, nightColor, 1);
                TweenIntensity(_dayLightIntensity, nightLightIntensity, 1);
                GameManager.IsDay = false;
            }
            else if (level > lightLevelThreshold && lightDefault.color == nightColor)
            {
                TweenColor(nightColor, _dayColor, 1);
                TweenIntensity(nightLightIntensity, _dayLightIntensity, 1);
                GameManager.IsDay = true;
            }
        }

        void TweenColor(Color begin, Color end, float time)
        {
            LeanTween.value(gameObject, 0.1f, 1f, time)
                .setOnUpdate((value) =>
                {
                    lightDefault.color = Color.Lerp(begin, end, value);
                });
        }

        void TweenIntensity(float begin, float end, float time)
        {
            LeanTween.value(gameObject, begin, end, time)
                .setOnUpdate((value) =>
                {
                    lightDefault.intensity = value;
                });
        }


    }
}