using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Astroids
{
    public class HudController : MonoBehaviour
    {
        const float MIN_ANGLE = 200;
        const float MAX_ANGLE = -20;
        const float TOTAL_ANGLE = MIN_ANGLE - MAX_ANGLE;

        const float MIN_FUEL_ANGLE = -130;
        const float MAX_FUEL_ANGLE = -55;
        const float TOTAL_FUEL_ANGLE = MIN_FUEL_ANGLE - MAX_FUEL_ANGLE;

        const float MAX_VALUE = 100;
        const int LABEL_COUNT = 8;

        const float NEEDLESPEED = 50;

        [SerializeField] Transform hudThrustMeter;
        [SerializeField] Transform labelTemplate;
        [SerializeField] Transform thrustNeedle;
        [SerializeField] Transform speedNeedle;
        [SerializeField] Transform fuelNeedle;
        [SerializeField] float lowOnFuelPercentage = 10;

        public bool IsFuelLow => _fuel < lowOnFuelPercentage && _fuel > 0;
        public bool IsFuelEmpty => _fuel <= 0;
        public bool IsFuelHalf => _fuel > 49 && _fuel < 51;

        internal bool m_hudCreated;

        float _thurst = 0;
        float _speed = 0;
        float _fuel = 100;
        float _cur_speed = -1;

        bool _hyperJumpActive;

        void Start()
        {
            StartCoroutine(CreateLabels());
        }

        void Update()
        {
            if (_hyperJumpActive)
            {
                _thurst = MAX_VALUE;
                _speed = MAX_VALUE;
            }

            if (_thurst > MAX_VALUE) _thurst = MAX_VALUE;
            if (_speed > MAX_VALUE) _speed = MAX_VALUE;
            if (_fuel > MAX_VALUE) _fuel = MAX_VALUE;

            if (_speed != _cur_speed)
            {
                if (_speed > _cur_speed)
                {
                    _cur_speed += Time.deltaTime * NEEDLESPEED;
                    _cur_speed = Mathf.Clamp(_cur_speed, 0, _speed);
                }
                else if (_speed < _cur_speed)
                {
                    _cur_speed -= Time.deltaTime * NEEDLESPEED;
                    _cur_speed = Mathf.Clamp(_cur_speed, _speed, MAX_VALUE);
                }
                speedNeedle.eulerAngles = new Vector3(0, 0, GetRotation(_cur_speed));
            }

            thrustNeedle.eulerAngles = new Vector3(0, 0, GetRotation(_thurst));
            fuelNeedle.eulerAngles = new Vector3(0, 0, GetFuelRotation(_fuel));
        }

        public void SetThrustPercentage(float perc) => _thurst = perc;
        public void SetSpeedPercentage(float perc) => _speed = perc;
        public void SetFuelPercentage(float perc) => _fuel = perc;

        public void ActivateHyperJump(bool active)
        {
            _hyperJumpActive = active;
            if (!active)
            {
                _thurst = 0;
                _speed = 0;
            }
        }

        IEnumerator CreateLabels()
        {
            labelTemplate.gameObject.SetActive(false);

            for (int i = 0; i <= LABEL_COUNT; i++)
            {
                var label = Instantiate(labelTemplate, hudThrustMeter);
                var value = (float)i / LABEL_COUNT;
                var angle = MIN_ANGLE - value * TOTAL_ANGLE;
                var text = label.GetComponentInChildren<TMPro.TextMeshProUGUI>(true);

                label.SetAsFirstSibling();
                label.eulerAngles = new Vector3(0, 0, angle);

                if (i % 2 == 0)
                {
                    text.text = (value * MAX_VALUE).ToString();
                    text.transform.eulerAngles = Vector3.zero;
                    text.gameObject.SetActive(true);
                }
                else
                {
                    text.text = "";
                    var img = label.GetComponentInChildren<Image>(true);
                    img.gameObject.SetActive(true);
                }

                label.gameObject.SetActive(true);
            }
            m_hudCreated = true;
            yield return null;

        }

        float GetRotation(float value)
        {
            float normalized = value / MAX_VALUE;
            return MIN_ANGLE - normalized * TOTAL_ANGLE;
        }

        float GetFuelRotation(float value)
        {
            float normalized = value / MAX_VALUE;
            return MIN_FUEL_ANGLE - normalized * TOTAL_FUEL_ANGLE;
        }

    }
}