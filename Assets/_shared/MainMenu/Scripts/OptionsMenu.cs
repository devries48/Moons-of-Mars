using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Audio;
using UnityEngine.Events;
using System.Collections.Generic;
using static MoonsOfMars.Shared.MainMenu;

namespace MoonsOfMars.Shared
{
    public class OptionsMenu : MonoBehaviour
    {
        public enum ThemeColor { blue, green, orange };

        [Header("GENERAL SETTINGS")]
        public GameObject showFpsText;
        public Canvas canvasFps;
        public GameObject themeBlueLine;
        public GameObject themeGreenLine;
        public GameObject themeOrangeLine;

        [Header("CONTROLS SETTINGS")]
        public GameObject invertmousetext;

        [Header("VIDEO SETTINGS")]
        public GameObject fullscreentext;
        public GameObject vsynctext;
        public GameObject resolutionDropdown;
        public GameObject qualityDropdown;

        [Header("AUDIO SETTINGS")]
        public AudioMixer masterMixer;
        public AudioMixer effectsMixer;
        public GameObject masterSlider;
        public GameObject musicSlider;
        public GameObject effectsSlider;

        [Header("EVENTS")]
        public UnityEvent<Theme> OnThemeChanged;

        List<Resolution> _resolutions;

        struct Defaults
        {
            static public float masterVolume;
            static public float musicVolume;
            static public float effectVolume;
        }

        public void Start()
        {
            InitGeneralSettings();
            InitAudioSettings();
            InitVideoSettings();
            InitControlsSettings();
        }

        #region General Settings
        void InitGeneralSettings()
        {
            var theme = PlayerPrefs.GetInt("ThemeColor", 0);
            DisplayTheme((ThemeColor)theme);
            DisplayFPS(PlayerPrefs.GetInt("ShowFPS") == 1);
        }

        // Called from UI when theme is changed
        public void ChangeTheme(string color)
        {
            var val = (ThemeColor)System.Enum.Parse(typeof(ThemeColor), color);
            DisplayTheme(val);
        }

        // Called from UI when show FPS is changed
        public void ChangeShowFPS()
        {
            bool show = PlayerPrefs.GetInt("ShowFPS") == 0;
            PlayerPrefs.SetInt("ShowFPS", show ? 1 : 0);
            DisplayFPS(show);
        }

        void DisplayTheme(ThemeColor val)
        {
            themeBlueLine.SetActive(val == ThemeColor.blue);
            themeGreenLine.SetActive(val == ThemeColor.green);
            themeOrangeLine.SetActive(val == ThemeColor.orange);

            PlayerPrefs.SetInt("ThemeColor", (int)val);

            var theme = val switch
            {
                ThemeColor.blue => Theme.custom2,
                ThemeColor.green => Theme.custom3,
                _ => Theme.custom1,
            };

            OnThemeChanged.Invoke(theme);
        }

        void DisplayFPS(bool show)
        {
            showFpsText.GetComponent<TMP_Text>().text = show ? "on" : "off";
            canvasFps.gameObject.SetActive(show);
        }

        #endregion

        #region Audio Settings
        void InitAudioSettings()
        {
            // default values
            masterMixer.GetFloat("MasterVolume", out Defaults.masterVolume);
            masterMixer.GetFloat("MusicVolume", out Defaults.masterVolume);
            effectsMixer.GetFloat("EffectsVolume", out Defaults.masterVolume);

            var volMaster = PlayerPrefs.GetFloat("MasterVolume", Defaults.masterVolume);
            var volMusic = PlayerPrefs.GetFloat("MusicVolume", Defaults.musicVolume);
            var volEffects = PlayerPrefs.GetFloat("EffectsVolume", Defaults.effectVolume);

            masterSlider.GetComponent<Slider>().value = volMaster;
            musicSlider.GetComponent<Slider>().value = volMusic;
            effectsSlider.GetComponent<Slider>().value = volEffects;
        }

        // Called from UI when master is changed
        public void VolumeMasterSlider()
        {
            var vol = masterSlider.GetComponent<Slider>().value;
            PlayerPrefs.SetFloat("MasterVolume", vol);
            masterMixer.SetFloat("MasterVolume", vol);
        }

        // Called from UI when master is changed
        public void VolumeMusicSlider()
        {
            var vol = musicSlider.GetComponent<Slider>().value;
            PlayerPrefs.SetFloat("MusicVolume", vol);
            masterMixer.SetFloat("MusicVolume", vol);
        }

        // Called from UI when master is changed
        public void VolumeEffectsSlider()
        {
            var vol = effectsSlider.GetComponent<Slider>().value;
            PlayerPrefs.SetFloat("EffectsVolume", vol);
            effectsMixer.SetFloat("EffectsVolume", vol);
        }

        #endregion

        #region Video Settings
        void InitVideoSettings()
        {
            var resolutionIndex = 0;
            var dropdown = resolutionDropdown.GetComponent<TMP_Dropdown>();
            var list = new List<string>();
            var i = 0;

            _resolutions = new List<Resolution>();

            foreach (var resolution in Screen.resolutions)
            {
                list.Add(GetResolutionDescription(resolution.width, resolution.height));
                _resolutions.Add(resolution);

                if (Screen.width == resolution.width && Screen.height == resolution.height)
                    resolutionIndex = i;

                i++;
            }

            print("resolutionIndex " + resolutionIndex);

            dropdown.ClearOptions();
            dropdown.AddOptions(list);

            DisplayFullScreen();
            DisplayVsync();
            DisplayResolution(resolutionIndex);
            DisplayQuality(QualitySettings.GetQualityLevel());
        }

        // Called from UI when show fullscreen is changed
        public void ChangeFullScreen()
        {
            Screen.fullScreen = !Screen.fullScreen;
            DisplayFullScreen();
        }

        // Called from UI when VSync is changed
        public void ChangeVsync()
        {
            if (QualitySettings.vSyncCount == 0)
                QualitySettings.vSyncCount = 1;
            else
                QualitySettings.vSyncCount = 0;

            DisplayVsync();
        }

        public void ChangeResolution(int index)
        {
            DisplayResolution(index);
        }

        public void ChangeQuality(int index)
        {
            DisplayQuality(index);
        }

        void DisplayFullScreen()
        {
            fullscreentext.GetComponent<TMP_Text>().text = Screen.fullScreen ? "on" : "off";
        }

        void DisplayVsync()
        {
            vsynctext.GetComponent<TMP_Text>().text = QualitySettings.vSyncCount == 1 ? "on" : "off";
        }

        void DisplayResolution(int index)
        {
            resolutionDropdown.GetComponent<TMP_Dropdown>().value = index;
            Screen.SetResolution(_resolutions[index].width, _resolutions[index].height, Screen.fullScreen);
        }

        void DisplayQuality(int index)
        {
            qualityDropdown.GetComponent<TMP_Dropdown>().value = index;
        }

        string GetResolutionDescription(int width, int height)
        {
            var resolution = $"{width} x {height}";
            var description = (width, height) switch
            {
                (1280, 720) => "720p hd ready",
                (1920, 1080) => "1080p full hd",
                (2560, 1440) => "1440p quad hd",
                (3840, 2160) => "4k ultra HD",
                (7680, 4320) => "8k",
                _ => ""
            };

            if (description != "")
                resolution += $" ({description})";

            return resolution;
        }

        #endregion

        void InitControlsSettings()
        {
            DisplayInvertMouse(PlayerPrefs.GetInt("Inverted") == 1);
        }

        // Called from UI when invert mouse is changed
        public void ChangeInvertMouse()
        {
            bool enable = PlayerPrefs.GetInt("Inverted") == 0;
            PlayerPrefs.SetInt("Inverted", enable ? 1 : 0);
            DisplayInvertMouse(enable);
        }

        void DisplayInvertMouse(bool enable)
        {
            invertmousetext.GetComponent<TMP_Text>().text = enable ? "on" : "off";
        }

    }
}