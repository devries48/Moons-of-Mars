using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Astroids
{
    public class HudManager : MonoBehaviour
    {
        #region constants
        const float TRANSITION_DEFAULT = 1f;
        const float TRANSITION_FAST = 3f;

        const string POWERUP_FIRERATE = "FIRE-RATE";
        const string POWERUP_SHOTSPREAD = "SHOT-SPREAD";
        #endregion

        #region editor
        public HudController hudController;

        [Header("Hud parts")]
        [SerializeField] GameObject hudAutoLight;
        [SerializeField] GameObject hudThrustMeter;
        [SerializeField] GameObject hudFuelMeter;
        [SerializeField] GameObject hudShield;
        [SerializeField] GameObject hudWeapon;
        [SerializeField] GameObject hudHyperJump;

        [Header("Hud elements")]
        [SerializeField] Image shieldRing;
        [SerializeField] Image weaponRing;
        [SerializeField] TMPro.TextMeshProUGUI weaponText;
        [SerializeField] Image fuelImage;

        [Header("Day Colors")]
        [SerializeField] Color dayDefaultColor;
        [SerializeField] Color dayBrightColor;
        [SerializeField] Color dayHighlightColor;

        [Header("Night Colors")]
        [SerializeField] Color nightDefaultColor;
        [SerializeField] Color nightBrightColor;
        [SerializeField] Color nightHighlightColor;

        [Header("Sounds")]
        [SerializeField] HudSounds hudSounds = new();

        #endregion

        #region properties
        public bool IsDay
        {
            get => __isDay;
            set
            {
                __isDay = value;
                SetHudColors(TRANSITION_FAST);
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
                    SetHudColors(TRANSITION_FAST);
                }
                else
                {
                    SetHudDisabled(0);
                    hudShield.transform.parent.gameObject.SetActive(false);
                }
            }
        }
        bool __hudActive;

        Color ColorDefault => IsDay ? dayDefaultColor : nightDefaultColor;
        Color ColorBright => IsDay ? dayBrightColor : nightBrightColor;
        Color ColorHighlight => IsDay ? dayHighlightColor : nightHighlightColor;
        Color ColorDisabled => IsDay
            ? new Color(dayDefaultColor.r, dayDefaultColor.g, dayDefaultColor.b, .1f)
            : new Color(nightDefaultColor.r, nightDefaultColor.g, nightDefaultColor.b, .1f);

        #endregion

        #region fields
        float _colorTransitionTime, _pwrShieldTime, _pwrWeaponTime;
        float _totShieldTime, _totWeaponTime;
        int _pwrHyperspaceCount;

        bool _blinkOn, _alarmOn;
        float _blinkTime = 0;
        readonly float _blinkInterval = .8f;

        PlayerShipController _shipCtrl;
        #endregion

        void OnEnable() => HudActive = false;

        void OnDisable() => DisconnectShip();

        void Update() => CheckFuel();

        public void ConnectToShip(PlayerShipController ship)
        {
            _shipCtrl = ship;

            shieldRing.fillAmount = 0;
            weaponRing.fillAmount = 0;

            _shipCtrl.m_ThrustController.ThrustChangedEvent += ThrustChanged;
            _shipCtrl.SpeedChangedEvent += SpeedChanged;
            _shipCtrl.FuelChangedEvent += FuelChanged;
            _shipCtrl.PowerUpActivatedEvent += PowerupActivated;
        }

        public void HudShow() => HudActive = true;

        public void HudHide()
        {
            SetHudDisabled(TRANSITION_DEFAULT);
            hudShield.transform.parent.gameObject.SetActive(false);
            _pwrHyperspaceCount = 0;
            HudActive = false;
        }

        void DisconnectShip()
        {
            if (_shipCtrl)
            {
                _shipCtrl.SpeedChangedEvent -= SpeedChanged;
                _shipCtrl.FuelChangedEvent -= FuelChanged;
                _shipCtrl.PowerUpActivatedEvent -= PowerupActivated;

                if (_shipCtrl.m_ThrustController)
                    _shipCtrl.m_ThrustController.ThrustChangedEvent -= ThrustChanged;
            }
        }

        public void AddHyperJump()
        {
            _pwrHyperspaceCount++;
            _colorTransitionTime = _pwrHyperspaceCount == 1 ? TRANSITION_DEFAULT : 0;

            SetHyperspaceColor();
            if (_pwrHyperspaceCount == 1)
                hudSounds.PlayClip(HudSounds.Clip.jumpActivated);
        }

        public void RemoveHyperJump()
        {
            _pwrHyperspaceCount--;
            SetHyperspaceColor();
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
            print("sfx: shieldActivated");
            hudSounds.PlayClip(HudSounds.Clip.shieldActivated);
        }

        public void ActivateWeapon(float t, PowerupManager.PowerupWeapon weapon)
        {
            print("Weapon time:" + _pwrWeaponTime);
            print("Weapon:" + weapon);
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
                case PowerupManager.PowerupWeapon.firerate:
                    print("sfx: firerateIncreased");
                    clip = HudSounds.Clip.firerateIncreased;
                    text = POWERUP_FIRERATE;
                    break;
                case PowerupManager.PowerupWeapon.shotSpread:
                    print("sfx: shotSpread");
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
                hudSounds.PlayClip(clip.Value);
        }

        void ThrustChanged(float perc) => hudController.SetThrustPercentage(perc * 100f);

        void SpeedChanged(float perc) => hudController.SetSpeedPercentage(perc * 100f);

        void FuelChanged(float perc) => hudController.SetFuelPercentage(perc * 100f);

        void PowerupActivated(float time, PowerupManager.Powerup powerup, PowerupManager.PowerupWeapon? weapon = null)
        {
            if (powerup == PowerupManager.Powerup.shield)
                ActivateShield(time);
            else if (powerup == PowerupManager.Powerup.weapon)
                ActivateWeapon(time, weapon.Value);
            else if (powerup == PowerupManager.Powerup.jump)
                if (time > 0)
                    AddHyperJump();
                else 
                    RemoveHyperJump();
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
            hudSounds.PlayClip(HudSounds.Clip.deactivate);
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
            hudSounds.PlayClip(HudSounds.Clip.deactivate);
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

        void SetHudColors(float t)
        {
            _colorTransitionTime = t;

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

        void SetShieldColor()
        {
            SetPowerupColor(hudShield, _pwrShieldTime > 0);
        }

        void SetWeaponColor()
        {
            SetPowerupColor(hudWeapon, _pwrWeaponTime > 0);
        }

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
        #region fuel
        void CheckFuel()
        {
            if (hudController.IsFuelLow)
            {
                if (!_alarmOn)
                    StartCoroutine(AlarmLoop());

                if (_blinkTime > _blinkInterval)
                {
                    BlinkFuelImage(_blinkOn);
                    _blinkTime = 0;
                    _blinkOn = !_blinkOn;
                }
                _blinkTime += Time.deltaTime;
            }
            else if (_blinkTime > 0)
            {
                if (_blinkOn && !hudController.IsFuelEmpty || !_blinkOn && hudController.IsFuelEmpty)
                    BlinkFuelImage(!hudController.IsFuelEmpty);

                _blinkTime = 0;
                _blinkOn = hudController.IsFuelEmpty;
            }
            else if (_blinkOn && !hudController.IsFuelEmpty)
            {
                BlinkFuelImage(true);
                _blinkOn = false;
            }
            else if (!_blinkOn && hudController.IsFuelEmpty)
            {
                BlinkFuelImage(false);
                _blinkOn = true;
            }
        }

        void BlinkFuelImage(bool on)
        {
            if (on)
                TweenColor(gameObject, ColorHighlight, ColorDefault, _blinkInterval / 2, fuelImage);
            else
                TweenColor(gameObject, ColorDefault, ColorHighlight, _blinkInterval / 2, fuelImage);
        }

        IEnumerator AlarmLoop()
        {
            _alarmOn = true;

            while (hudController.IsFuelLow)
            {
                hudSounds.PlayClip(HudSounds.Clip.fuelLow, true);
                yield return new WaitForSeconds(_blinkInterval * 3);
            }
            if (hudController.IsFuelEmpty)
                hudSounds.PlayClip(HudSounds.Clip.fuelEmpty);

            _alarmOn = false;
        }

        #endregion

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