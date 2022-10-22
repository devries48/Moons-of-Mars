using UnityEngine;

namespace Game.Astroids
{
    public class LightsManager : MonoBehaviour
    {
        [Header("Night Lighting")]

        [SerializeField]
        Light lightDefault;

        [SerializeField]
        Color nightColor;

        [SerializeField]
        float nightLightIntensity;

        [SerializeField, Tooltip("Select from 'Light Camera'")]
        LightCheckController lightCheckController;

        [SerializeField, Tooltip("Switch on below threshold")]
        int lightLevelThreshold;

        Color _dayColor;
        float _dayLightIntensity;

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

        void LevelChanged(int level)
        {
            if (level < lightLevelThreshold && lightDefault.color == _dayColor)
            {
                TweenColor(_dayColor, nightColor, 1);
                TweenIntensity(_dayLightIntensity, nightLightIntensity, 1);

            }
            else if (level > lightLevelThreshold && lightDefault.color == nightColor)
            {
                TweenColor(nightColor, _dayColor, 1);
                TweenIntensity(nightLightIntensity, _dayLightIntensity, 1);
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