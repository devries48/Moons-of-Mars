using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Astroids
{
    public class HudManager : MonoBehaviour
    {
        #region constants
        const float TRANSITION_DEFAULT = 1f;
        const float TRANSITION_LONG = 3f;

        const string POWERUP_FIRERATE = "FIRE-RATE";
        const string POWERUP_SHOTSPREAD = "SHOT-SPREAD";
        #endregion

        public enum HudAction { none, hyperjumpStart, hyperjumpSelect }

        #region editor
        public HudController hudController;

        [Header("Hud parts")]
        [SerializeField] GameObject hudAutoLight;
        [SerializeField] GameObject hudThrustMeter;
        [SerializeField] GameObject hudFuelMeter;
        [SerializeField] GameObject hudShield;
        [SerializeField] GameObject hudWeapon;
        [SerializeField] GameObject hudHyperJump;

        //TODO: Scriptable object?
        [Header("Hud elements")]
        [SerializeField] Image shieldRing;
        [SerializeField] Image weaponRing;
        [SerializeField] TMPro.TextMeshProUGUI weaponText;
        [SerializeField] Image fuelImage;
        [SerializeField] Image jumpImage;

        [Header("Day Colors")]
        [SerializeField] Color dayDefaultColor;
        [SerializeField] Color dayBrightColor;
        [SerializeField] Color dayHighlightColor;
        [SerializeField] Color dayJumpCrosshairColor;

        [Header("Night Colors")]
        [SerializeField] Color nightDefaultColor;
        [SerializeField] Color nightBrightColor;
        [SerializeField] Color nightHighlightColor;
        [SerializeField] Color nightJumpCrosshairColor;

        [Header("Audio")]
        [SerializeField] HudSounds hudSounds = new();
        #endregion

        #region properties
        public bool IsDay
        {
            get => __isDay;
            set
            {
                __isDay = value;
                StartCoroutine(SetHudColors(TRANSITION_LONG));
                if (value != _lightsOn)
                {
                    _lightsOn = value;
                    PlayClip(value ? HudSounds.Clip.lightsOn : HudSounds.Clip.lightsOff);
                }
            }
        }
        bool __isDay = true;

        public bool HudActive
        {
            get => __hudActive;
            set
            {
                __hudActive = value;

                if (value)
                {
                    hudShield.transform.parent.gameObject.SetActive(true);
                    StartCoroutine(SetHudColors(TRANSITION_LONG));
                }
                else
                {
                    SetHudDisabled(0);
                    hudShield.transform.parent.gameObject.SetActive(false);
                }
            }
        }
        bool __hudActive;

        public int HyperJumps => _pwrHyperspaceCount;
        public Color ColorDefault => IsDay ? dayDefaultColor : nightDefaultColor;
        Color ColorBright => IsDay ? dayBrightColor : nightBrightColor;
        Color ColorDisabled => new(ColorDefault.r, ColorDefault.g, ColorDefault.b, .1f);
        public Color ColorHighlight => IsDay ? dayHighlightColor : nightHighlightColor;
        public Color ColorJumpCrosshair => IsDay ? dayJumpCrosshairColor : nightJumpCrosshairColor;
        #endregion

        #region fields
        float _colorTransitionTime, _pwrShieldTime, _pwrWeaponTime;
        float _totShieldTime, _totWeaponTime;
        int _pwrHyperspaceCount;

        bool _lightsOn;
        readonly float _blinkInterval = .8f;
        bool _blinkFuelOn, _blinkJumpOn, _alarmFuelOn, _fuelHalf;
        float _blinkFuelTime, _blinkJumpTime;

        HudAction _currentAction;

        PlayerShipController _shipCtrl;
        #endregion

        void OnEnable() => HudActive = false;

        void OnDisable() => DisconnectShip();

        void Update()
        {
            CheckFuel();
            CheckJump();
        }

        public void ConnectToShip(PlayerShipController ship)
        {
            _shipCtrl = ship;
            _lightsOn = false;
            _pwrHyperspaceCount = 0;

            shieldRing.fillAmount = 0;
            weaponRing.fillAmount = 0;

            _shipCtrl.m_ThrustController.ThrustChangedEvent += HandleThrustChanged;
            _shipCtrl.SpeedChangedEvent += HandleSpeedChanged;
            _shipCtrl.FuelChangedEvent += HandleFuelChanged;
            _shipCtrl.HudActionEvent += HandleHudAction;
            _shipCtrl.PowerUpActivatedEvent += HandlePowerupActivated;
        }

        public void HudShow() => HudActive = true;

        public void HudHide() => StartCoroutine(HudHideCore());

        IEnumerator HudHideCore()
        {
            SetHudDisabled(TRANSITION_DEFAULT);
            yield return new WaitForSeconds(TRANSITION_DEFAULT);

            hudShield.transform.parent.gameObject.SetActive(false);
            HudActive = false;
        }

        void DisconnectShip()
        {
            if (_shipCtrl)
            {
                _shipCtrl.SpeedChangedEvent -= HandleSpeedChanged;
                _shipCtrl.FuelChangedEvent -= HandleFuelChanged;
                _shipCtrl.PowerUpActivatedEvent -= HandlePowerupActivated;

                if (_shipCtrl.m_ThrustController)
                    _shipCtrl.m_ThrustController.ThrustChangedEvent -= HandleThrustChanged;
            }
        }

        public void AddHyperJump()
        {
            _pwrHyperspaceCount++;
            _colorTransitionTime = _pwrHyperspaceCount == 1 ? TRANSITION_DEFAULT : 0;

            SetHyperspaceColor();
            if (_pwrHyperspaceCount == 1)
                PlayClip(HudSounds.Clip.jumpActivate);
        }

        public void RemoveHyperJump()
        {
            _pwrHyperspaceCount--;
            SetHyperspaceColor();
        }

        public void CancelPowerups()
        {
            _pwrShieldTime = 0;
            _pwrWeaponTime = 0;
            shieldRing.fillAmount = 1;
            weaponRing.fillAmount = 1;
        }

        public void ActivateShield(float t)
        {
            if (_pwrShieldTime > 0)
            {
                _pwrShieldTime += t;
                if (_pwrShieldTime > _totShieldTime)
                    _totShieldTime = _pwrShieldTime;

                return;
            }

            _pwrShieldTime = t;
            _totShieldTime = t;
            _colorTransitionTime = TRANSITION_DEFAULT;

            SetShieldColor();
            StartCoroutine(ShieldCountDown());
            PlayClip(HudSounds.Clip.shieldActivated);
        }

        public void ActivateWeapon(float t, PowerupManagerData.PowerupWeapon weapon)
        {
            if (_pwrWeaponTime > 0)
            {
                _pwrWeaponTime += t;
                if (_pwrWeaponTime > _totWeaponTime)
                    _totWeaponTime = _pwrWeaponTime;

                return;
            }

            _pwrWeaponTime = t;
            _totWeaponTime = t;
            _colorTransitionTime = TRANSITION_DEFAULT;

            HudSounds.Clip? clip = null;
            var text = string.Empty;

            switch (weapon)
            {
                case PowerupManagerData.PowerupWeapon.firerate:
                    clip = HudSounds.Clip.firerateIncreased;
                    text = POWERUP_FIRERATE;
                    break;
                case PowerupManagerData.PowerupWeapon.shotSpread:
                    clip = HudSounds.Clip.shotSpreadActivated;
                    text = POWERUP_SHOTSPREAD;
                    break;
                default:
                    break;
            }

            weaponText.text = text;

            SetWeaponColor();
            StartCoroutine(WeaponCountDown());

            if (clip.HasValue)
                PlayClip(clip.Value);
        }

        public void PlayClip(HudSounds.Clip clip) => hudSounds.PlayClip(clip);

        void HandleThrustChanged(float perc) => hudController.SetThrustPercentage(perc * 100f);

        void HandleSpeedChanged(float perc) => hudController.SetSpeedPercentage(perc * 100f);

        void HandleFuelChanged(float perc) => hudController.SetFuelPercentage(perc * 100f);

        void HandlePowerupActivated(float time, PowerupManagerData.Powerup powerup, PowerupManagerData.PowerupWeapon? weapon = null)
        {
            if (powerup == PowerupManagerData.Powerup.shield)
                ActivateShield(time);
            else if (powerup == PowerupManagerData.Powerup.weapon)
                ActivateWeapon(time, weapon.Value);
            else if (powerup == PowerupManagerData.Powerup.jump)
                AddHyperJump();
        }

        void HandleHudAction(HudAction action)
        {
            _currentAction = action;

            if (action == HudAction.none)
            {
                StartCoroutine(SetHudColors(TRANSITION_DEFAULT));
                PlayClip(HudSounds.Clip.deactivate);
            }
            else if (action == HudAction.hyperjumpStart)
            {
                HandleSpeedChanged(1);
                RemoveHyperJump();
                hudController.ActivateHyperJump(true);
                PlayClip(HudSounds.Clip.hyperJumpActivated);
            }
            else if (action == HudAction.hyperjumpSelect)
            {
                hudController.ActivateHyperJump(false);
                SetHudDisabled(TRANSITION_DEFAULT);
            }
        }

        IEnumerator ShieldCountDown()
        {
            while (_pwrShieldTime > 0)
            {
                _pwrShieldTime -= Time.deltaTime;
                shieldRing.fillAmount = _pwrShieldTime / _totShieldTime;

                yield return null;
            }
            _pwrShieldTime = 0;

            SetShieldColor();
            PlayClip(HudSounds.Clip.deactivate);
        }

        IEnumerator WeaponCountDown()
        {
            while (_pwrWeaponTime > 0)
            {
                _pwrWeaponTime -= Time.deltaTime;
                weaponRing.fillAmount = _pwrWeaponTime / _totWeaponTime;

                yield return null;
            }
            _pwrWeaponTime = 0;

            SetWeaponColor();
            PlayClip(HudSounds.Clip.deactivate);
        }

        #region colors
        void SetHudDisabled(float t)
        {
            _colorTransitionTime = t;

            SetImageColor(hudAutoLight, ColorDisabled);
            SetTextColor(hudThrustMeter, ColorDisabled);
            SetImageColor(hudThrustMeter, ColorDisabled);
            SetTextColor(hudFuelMeter, ColorDisabled);
            SetImageColor(hudFuelMeter, ColorDisabled);

            SetShieldColor();
            SetPowerupColor(hudWeapon, _pwrWeaponTime > 0);
            SetHyperspaceColor();
        }

        IEnumerator SetHudColors(float t)
        {
            _colorTransitionTime = t;

            while (!hudController.m_hudCreated)
                yield return null;

            var dashColor = ColorDefault;
            dashColor.a /= 2f;

            // meters
            SetImageColor(hudAutoLight, IsDay ? ColorDisabled : ColorBright);
            SetTextColor(hudThrustMeter, ColorDefault);
            SetImageColor(hudThrustMeter, ColorDefault, ColorHighlight, dashColor);

            SetTextColor(hudFuelMeter, ColorBright);
            SetImageColor(hudFuelMeter, ColorDefault, ColorHighlight);

            // shield & weapon
            SetShieldColor();
            SetPowerupColor(hudWeapon, _pwrWeaponTime > 0);
            SetHyperspaceColor();
        }

        void SetPowerupColor(GameObject powerup, bool isActive)
        {
            SetImageColor(powerup, isActive ? ColorBright : ColorDisabled);
            SetTextColor(powerup, isActive ? ColorBright : ColorDisabled);
        }

        void SetShieldColor() => SetPowerupColor(hudShield, _pwrShieldTime > 0);

        void SetWeaponColor() => SetPowerupColor(hudWeapon, _pwrWeaponTime > 0);

        void SetHyperspaceColor()
        {
            var hasJump = _pwrHyperspaceCount > 0;
            var txtJmpColor = ColorBright;
            var txtColor = hasJump ? ColorDefault : ColorDisabled;
            var imgColor = hasJump ? ColorDefault : ColorDisabled;

            if (hasJump)
                imgColor.a /= 2f;

            SetImageColor(hudHyperJump, imgColor);

            foreach (var text in hudHyperJump.GetComponentsInChildren<TMPro.TMP_Text>())
            {
                var it = text;
                Color clr;

                if (text.gameObject.name.EndsWith("count"))
                {
                    text.text = _pwrHyperspaceCount == 0 ? "" : _pwrHyperspaceCount.ToString();
                    clr = txtJmpColor;
                }
                else
                    clr = txtColor;

                TweenColor(gameObject, text.color, clr, _colorTransitionTime, it);
            }
        }

        void SetTextColor(GameObject parent, Color color)
        {
            foreach (var text in parent.GetComponentsInChildren<TMPro.TMP_Text>())
            {
                var it = text;

                TweenColor(gameObject, text.color, color, _colorTransitionTime, it);
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

                TweenColor(gameObject, img.color, clr, _colorTransitionTime, it);
            }
        }

        #endregion

        #region hyperspace
        void CheckJump()
        {
            if (_currentAction == HudAction.hyperjumpStart)
            {
                if (_blinkJumpTime == 0)
                {
                    _blinkJumpOn = !_blinkJumpOn;
                    BlinkImage(jumpImage, _blinkJumpOn);
                    _blinkJumpTime += Time.deltaTime;
                }
                else if (_blinkJumpTime > _blinkInterval)
                    _blinkJumpTime = 0;
                else
                    _blinkJumpTime += Time.deltaTime;
            }
            else if (_blinkJumpTime > 0)
            {
                _blinkJumpTime = 0;
                _blinkJumpOn = false;
                SetHyperspaceColor();
            }
        }
        #endregion

        #region fuel
        void CheckFuel()
        {
            if (hudController.IsFuelLow)
            {
                if (!_alarmFuelOn)
                    StartCoroutine(AlarmLoop());

                if (_blinkFuelTime > _blinkInterval)
                {
                    BlinkImage(fuelImage, _blinkFuelOn);
                    _blinkFuelTime = 0;
                    _blinkFuelOn = !_blinkFuelOn;
                }
                _blinkFuelTime += Time.deltaTime;
            }
            else if (_blinkFuelTime > 0)
            {
                if (_blinkFuelOn && !hudController.IsFuelEmpty || !_blinkFuelOn && hudController.IsFuelEmpty)
                    BlinkImage(fuelImage, !hudController.IsFuelEmpty);

                _blinkFuelTime = 0;
                _blinkFuelOn = hudController.IsFuelEmpty;
            }
            else if (_blinkFuelOn && !hudController.IsFuelEmpty)
            {
                BlinkImage(fuelImage, true);
                _blinkFuelOn = false;
            }
            else if (!_blinkFuelOn && hudController.IsFuelEmpty)
            {
                BlinkImage(fuelImage, false);
                _blinkFuelOn = true;
            }

            if (hudController.IsFuelHalf && !_fuelHalf)
            {
                _fuelHalf = true;
                PlayClip(HudSounds.Clip.fuelHalf);
            }
            else if (!hudController.IsFuelHalf && _fuelHalf)
                _fuelHalf = false;

        }

        IEnumerator AlarmLoop()
        {
            _alarmFuelOn = true;

            while (hudController.IsFuelLow)
            {
                hudSounds.PlayClip(HudSounds.Clip.fuelLow, true);
                yield return new WaitForSeconds(_blinkInterval * 3);
            }
            if (hudController.IsFuelEmpty)
                PlayClip(HudSounds.Clip.fuelEmpty);

            _alarmFuelOn = false;
        }
        #endregion

        void BlinkImage(Image image, bool on)
        {
            if (on)
                TweenColor(gameObject, ColorHighlight, ColorDefault, _blinkInterval / 2, image);
            else
                TweenColor(gameObject, ColorDefault, ColorHighlight, _blinkInterval / 2, image);
        }

        public void BlinkText(TMPro.TextMeshProUGUI text, float interval, bool on)
        {
            if (on)
                TweenColor(gameObject, ColorHighlight, ColorDefault, interval / 2, text);
            else
                TweenColor(gameObject, ColorDefault, ColorHighlight, interval / 2, text);
        }


        //TODO: replace tween util with these
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