using System.Collections;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Astroids
{
    public class JumpController : GameMonoBehaviour
    {
        [SerializeField] float cursorSpeed = 10;
        [SerializeField] TextMeshProUGUI countDownText;
        [SerializeField] TextMeshProUGUI launchText;
        [SerializeField] Image crosshair;

        HudManager Hud
        {
            get
            {
                if (__hud == null)
                    __hud = GameManager.m_HudManager;

                return __hud;
            }
        }
        HudManager __hud;

        internal Vector3 m_JumpPosition;
        internal bool m_Launched;

        float _countDownTime;
        bool _started;
        bool _activateLaunch;

        void OnEnable()
        {
            m_ScreenWrap = true;

            _started = false;
            _activateLaunch = false;
            m_Launched = false;

            launchText.gameObject.SetActive(false);
            countDownText.text = string.Empty;
        }

        void Update()
        {
            if (!_started) return;

            if (ShipInput.IsShooting() &&  !_activateLaunch)
                _activateLaunch = true;

            if (_activateLaunch) return;

            float x = Input.GetAxis("Horizontal");
            float y = Input.GetAxis("Vertical");

            Vector3 movement = new(x, y, 0);
            movement = Vector3.ClampMagnitude(movement, 1);
            transform.Translate(cursorSpeed * Time.deltaTime * movement);
        }

        public void StartCountdown(Vector3 pos, float time)
        {
            _countDownTime = time;
            transform.position = pos;
            SetColors();
            launchText.gameObject.SetActive(false);

            StartCoroutine(Countdown());
        }

        void SetColors()
        {
            crosshair.color = Hud.ColorJumpCrosshair;
            countDownText.color = Hud.ColorHighlight;
            launchText.color = Hud.ColorHighlight;
        }

        IEnumerator Countdown()
        {
            _started = true;

            while (_countDownTime > 0 && !_activateLaunch)
            {
                countDownText.text = _countDownTime.ToString("0.0", CultureInfo.InvariantCulture);
                yield return new WaitForSeconds(.1f);

                _countDownTime -= .1f;
            }
            m_JumpPosition = transform.position;

            print("m_JumpPosition: " + m_JumpPosition);

            countDownText.text = string.Empty;
            launchText.gameObject.SetActive(true);
            m_Launched = true;
        }
    }
}