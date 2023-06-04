using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Events;

using Cinemachine;
using MoonsOfMars.Shared;
using static MoonsOfMars.SolarSystem.SolarSystemController;

namespace MoonsOfMars.SolarSystem
{
    // see https://easings.net/

    [DisallowMultipleComponent]
    public class MenuManager : MonoBehaviour
    {
        #region editor fields
        [Header("UI Elements")]
        [SerializeField] MainMenu mainMenu;
        [SerializeField] GameObject infoPanel;
        [SerializeField] GameObject exitButton;
        [SerializeField] ParticleSystem spaceDebriSystem;

        [Header("Sound")]
        [SerializeField] AudioSource slideInSound;

        [Header("Controllers")]
        [SerializeField] SolarSystemPanelController solarSystemController;
        #endregion

        #region fields
        PlayableDirector _director;
        #endregion

        #region properties
        GameManager GmManager => GameManager.Instance;

        #endregion

        void OnEnable()
        {
            GmManager.RegisterCameras(GmManager.m_MenuCamera, GmManager.m_SolarSystemCamera);
            solarSystemController.HideControlPanel();
        }

        void OnDisable()
        {
            GmManager.UnregisterCameras(GmManager.m_MenuCamera, GmManager.m_SolarSystemCamera);
        }

        void Awake()
        {
            _director = GetComponent<PlayableDirector>();
            _director.played += Director_played;
            _director.stopped += Director_stopped;
        }

        void Start()
        {
            ShowMainMenu();
        }

        public void ShowBodyInfoWindow(CelestialBodyName name, bool isDeselect)
        {
            if (isDeselect)
                TweenUtil.TweenPivot(infoPanel, new Vector2(0f, 0.5f), null, LeanTweenType.easeInOutBack);
            else
            {
                SetWindowInfo(name);
                slideInSound.Play();
                TweenUtil.TweenPivot(infoPanel, new Vector2(1.2f, 0.5f), null, LeanTweenType.easeInOutBack);
            }
        }


        public void OnMenuAction(MainMenu.MenuAction action)
        {
            if (action == MainMenu.MenuAction.solarSystem)
            {
                MenuSolarSytem();
            }
        }

        public void MenuStartTour()
        {
            _director.Play();
        }

        public void MenuSolarSytem()
        {
            HideMainMenu();

            solarSystemController.ShowControlPanel();
            GmManager.SolarSystemCtrl.IsDemo = false;

            StartCoroutine(DelayExecute(GmManager.CameraSwitchTime, GmManager.SolarSystemCtrl.ShowOrbitLines));

            if (GmManager.SolarSystemCtrl.GetPlanetScaleMultiplier() == 1)
                TweenPlanetScale(GmManager.CameraSwitchTime);

            GmManager.SwitchCamera(GmManager.m_SolarSystemCamera);
        }

        public void MenuQuit()
        {
            HideMainMenu(true);
        }

        public void ExitToMainMenu()
        {
            var wait = 0f;

            if (GmManager.SolarSystemCtrl.OrbitLinesVisible)
            {
                wait = .5f; // Wait for orbit lines to fade away
                GmManager.SolarSystemCtrl.HideOrbitLines();
                solarSystemController.HideControlPanel(true);
            }

            if (_director.state == PlayState.Playing)
                _director.Stop();
            else if (wait > 0)
                StartCoroutine(DelayExecute(wait, ShowMainMenu));
            else
                ShowMainMenu();
        }

        public static IEnumerator DelayExecute(float sec, UnityAction method)
        {
            yield return new WaitForSeconds(sec);

            method();
        }

        void SetWindowInfo(CelestialBodyName name)
        {
            var info = GmManager.CelestialBody(name).Info;
            if (info == null)
                return;

            info.SetInfoUI(infoPanel);
        }

        void Director_stopped(PlayableDirector obj)
        {
            ShowMainMenu();
        }

        void Director_played(PlayableDirector obj)
        {
            HideMainMenu();
        }

        // Restore MainMenu environment
        void ShowMainMenu()
        {
            if (spaceDebriSystem == null) return;

            mainMenu.ShowMenu();
            GmManager.SolarSystemCtrl.IsDemo = true;
            GmManager.SwitchCamera(GmManager.m_MenuCamera);

            if (GmManager.SolarSystemCtrl.GetPlanetScaleMultiplier() > 1)
                TweenPlanetScale(1f, true);

            HideExitButton();
            //TweenUtil.TweenPivot(mainMenu, new Vector2(0f, 0.5f), new Vector3(0, -30, 0), LeanTweenType.easeInOutSine, 1f, LeanTweenType.easeInCirc, GmManager.CameraSwitchTime);

            spaceDebriSystem.Play();
        }

        void HideMainMenu(bool quit = false, bool stopDebri = true)
        {
            if (!quit) ShowExitButton();

            mainMenu.HideMenu();

            if (stopDebri) spaceDebriSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            if (quit)
            {
                var closeId = ApplicationClose(GmManager.m_MenuCamera);
                var d = LeanTween.descr(closeId);

                d?.setOnComplete(QuitApplication);
            }
        }

        //void OpenMenuWindow(GameObject window)
        //{
        //    TweenUtil.MenuWindowOpen(window);
        //}

        //int CloseMenuWindow(GameObject window)
        //{
        //    return TweenUtil.MenuWindowClose(window);
        //}

        int ApplicationClose(CinemachineVirtualCamera menuCamera)
        {
            var zoomId = 0;
            var timeApplicationClose = TweenUtil.m_timeMenuOpenClose;

            CinemachineFramingTransposer transposer = null;

            if (menuCamera.TryGetComponent<CinemachineVirtualCamera>(out var cam))
                transposer = cam.GetCinemachineComponent<CinemachineFramingTransposer>();

            if (transposer != null)
            {
                LeanTween.value(transposer.m_ScreenX, 0.5f, timeApplicationClose / 2).setEase(LeanTweenType.easeOutQuint).setOnUpdate((val) =>
                {
                    transposer.m_ScreenX = val;
                });
                LeanTween.value(transposer.m_ScreenY, 0.5f, timeApplicationClose / 2).setEase(LeanTweenType.easeOutQuint).setOnUpdate((val) =>
                {
                    transposer.m_ScreenY = val;
                });
                zoomId = LeanTween.value(transposer.m_CameraDistance, timeApplicationClose * 50f, timeApplicationClose).setEase(LeanTweenType.easeInOutSine).setOnUpdate((val) =>
                {
                    transposer.m_CameraDistance = val;
                }).id;
            }

            return zoomId;
        }

        void QuitApplication()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
        }

        void ShowExitButton()
        {
            TweenUtil.TweenPivot(exitButton, new Vector2(-.2f, -.2f), GmManager.CameraSwitchTime);
        }

        void HideExitButton()
        {
            TweenUtil.TweenPivot(exitButton, new Vector2(-.2f, 2f), null);
        }



        void TweenPlanetScale(float scaleTime, bool scaleOut = false)
        {
            var start = scaleOut ? 10 : 1;
            var end = scaleOut ? 1 : 10;
            var type = scaleOut ? LeanTweenType.easeOutQuint : LeanTweenType.easeInSine;

            LeanTween.value(start, end, scaleTime).setEase(type).setOnUpdate((val) =>
            {
                GmManager.SolarSystemCtrl.SetPlanetScaleMultiplier(val);
            });
        }


    }
}