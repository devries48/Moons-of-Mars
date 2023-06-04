using Cinemachine;
using System.Collections;
using TMPro;
using UnityEngine;

namespace MoonsOfMars.Shared
{
    using Announcers;

    public class GameManagerBase<T> : SingletonBase<T> where T : MonoBehaviour
    {
        [Header("Generic Managers")]
        [SerializeField] protected AudioManagerBase _audioManager;
        [SerializeField] protected InputManagerBase _inputManager;

        [Header("Generic Announcer")]
        [SerializeField, Tooltip("Use the 'Announce' method to pop in/out messages. (Can be left blank.)")]
        protected TextMeshProUGUI _announcerText;
        [SerializeField, Tooltip("Announcer text visible time in seconds.")]
        protected float _announcerTime = 1;


        /// <summary>
        /// Usage: public AudioManager AudioManager => AudioManager<AudioManager>();
        /// </summary>
        public TaudioManager GetAudioManager<TaudioManager>() where TaudioManager : AudioManagerBase => (TaudioManager)_audioManager;

        /// <summary>
        /// Usage: public InputManager InputManager => InputManager<AudioManager>();
        /// </summary>
        public TinputManager GetInputManager<TinputManager>() where TinputManager : InputManagerBase => (TinputManager)_inputManager;

        GameAnnouncer Announcer
        {
            get
            {
                if (__announcer == null && _announcerText != null)
                    __announcer = GameAnnouncer.AnnounceTo(TextAnnouncerBase.TextComponent(_announcerText));

                return __announcer;
            }
        }
        GameAnnouncer __announcer;

        public void Announce(string text) => StartCoroutine(AnnounceCore(text));
        public void Announce(string format, object arg0) => StartCoroutine(AnnounceCore(string.Format(format, arg0)));

        IEnumerator AnnounceCore(string text)
        {
            Announcer.Announce(text);
            yield return new WaitForSeconds(_announcerTime);
            Announcer.ClearAnnouncements();
        }

        public void RegisterCameras(params CinemachineVirtualCamera[] cameras)
        {
            foreach (var cam in cameras)
                CameraSwitcher.Register(cam);
        }

        public void UnregisterCameras(params CinemachineVirtualCamera[] cameras)
        {
            foreach (var cam in cameras)
                CameraSwitcher.Unregister(cam);
        }

        public void SwitchCamera(CinemachineVirtualCamera camera) => CameraSwitcher.SwitchCamera(camera);

        protected void GameExit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
        }
    }
}