using UnityEngine;
using UnityEngine.UI;

namespace Game.Astroids
{
    public class HudManager : MonoBehaviour
    {
        [SerializeField] HudController hudController;

        [Header("Hud parts")]
        public GameObject hudAutoLight;
        public GameObject hudThrustMeter;
        public GameObject hudFuelMeter;

        [Header("Day Colors")]
        [SerializeField] Color dayDefaultColor;
        [SerializeField] Color dayBrightColor;
        [SerializeField] Color dayHighlightColor;
        [SerializeField] Color dayDisabledColor;

        [Header("Night Colors")]
        [SerializeField] Color nightDefaultColor;
        [SerializeField] Color nightBrightColor;
        [SerializeField] Color nightHighlightColor;
        [SerializeField] Color nightDisabledColor;

        PlayerShipController _shipCtrl;

        public void ConnectToShip(PlayerShipController ship, bool isDay)
        {
            print("ConnectToShip");
            SetHudCycle(isDay);

            _shipCtrl = ship;

            DisconnectShip();

            _shipCtrl.m_ThrustController.ThrustChangedEvent += ThrustChanged;
            _shipCtrl.SpeedChangedEvent += SpeedChanged;
        }

        void DisconnectShip()
        {
            if (_shipCtrl)
            {
                _shipCtrl.SpeedChangedEvent -= SpeedChanged;

                if (_shipCtrl.m_ThrustController)
                    _shipCtrl.m_ThrustController.ThrustChangedEvent -= ThrustChanged;
            }
        }
        void ThrustChanged(float perc)
        {
            hudController.SetThrustPercentage(perc * 100f);
        }

        void SpeedChanged(float perc)
        {
            hudController.SetSpeedPercentage(perc * 100f);
        }

        public void SetHudCycle(bool isDay)
        {
            var defaColor = isDay ? dayDefaultColor : nightDefaultColor;
            var brigColor = isDay ? dayBrightColor : nightBrightColor;
            var highColor = isDay ? dayHighlightColor : nightHighlightColor;
            var disaColor = isDay ? dayDisabledColor : nightDisabledColor;
            var dashColor = defaColor;
            dashColor.a /= 2f;

            SetImageColor(hudAutoLight, isDay ? disaColor : brigColor);

            SetTextColor(hudThrustMeter, defaColor);
            SetImageColor(hudThrustMeter, defaColor, highColor, dashColor);

            SetTextColor(hudFuelMeter, brigColor);
            SetImageColor(hudFuelMeter, defaColor, highColor);
        }

        void SetTextColor(GameObject parent, Color color)
        {
            foreach (var text in parent.GetComponentsInChildren<TMPro.TMP_Text>())
            {
                var it = text;
                TweenColor(gameObject, text.color, color, 3f, it);
            }
        }

        void SetImageColor(GameObject parent, Color color, Color highlightColor = default, Color dashColor = default)
        {
            foreach (var img in parent.GetComponentsInChildren<Image>())
            {
                Color clr;
                var it = img;

                if (img.gameObject.name.EndsWith("highlight"))
                    clr = highlightColor;
                else if (img.gameObject.name.StartsWith("dash"))
                    clr = dashColor;
                else
                    clr = color;

                TweenColor(gameObject, img.color, clr, 3f, it);
            }
        }

        void TweenColor(GameObject gameObject, Color begin, Color end, float time, Image item)
        {
            LeanTween.value(gameObject, 0.01f, 1f, time)
                .setOnUpdate((value) =>
                {
                    item.color = Color.Lerp(begin, end, value);
                });
        }
        void TweenColor(GameObject gameObject, Color begin, Color end, float time, TMPro.TMP_Text item)
        {
            LeanTween.value(gameObject, 0.01f, 1f, time)
                .setOnUpdate((value) =>
                {
                    item.color = Color.Lerp(begin, end, value);
                });
        }

    }
}