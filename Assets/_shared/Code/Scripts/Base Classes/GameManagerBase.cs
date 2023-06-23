using Cinemachine;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MoonsOfMars.Shared
{
    using Announcers;
    using System;
    using static MoonsOfMars.Shared.EffectsData;
    using static MoonsOfMars.Shared.EffectsManager1;
    using static MoonsOfMars.Shared.Utils;

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
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

        // Game Manager Settings (scriptable object)
        [SerializeField] bool _useObjectPoolScene;
        [SerializeField] EffectsData _effectsData;

        //---

        /// <summary>
        /// Usage: public AudioManager AudioManager => AudioManager<AudioManager>();
        /// </summary>
        public TaudioManager GetAudioManager<TaudioManager>() where TaudioManager : AudioManagerBase => (TaudioManager)_audioManager;

        /// <summary>
        /// Usage: public InputManager InputManager => InputManager<AudioManager>();
        /// </summary>
        public TinputManager GetInputManager<TinputManager>() where TinputManager : InputManagerBase => (TinputManager)_inputManager;

        protected override void Awake()
        {
            base.Awake();
            StartCoroutine(BuildPools());
        }

        #region Object Pooling & Effects

        IEnumerator BuildPools()
        {
            if (_useObjectPoolScene)
            {
                GameObjectPool.CreateObjectPoolScene();

                while (!GameObjectPool.ObjectPoolSceneLoaded)
                    yield return null;
            }

            Initialize();
        }

        /// <summary>
        /// Creates an object pool, when object pool is needed the method will wait until it is created.
        /// </summary>
        /// <example>
        /// GameManager.CreateObjectPool(BuildPoolsAction);
        /// 
        /// void BuildPoolsAction()
        /// {
        ///    _asteroidPool = GameManager.CreateObjectPool(asteroidPrefab, 20, 100);
        /// }
        /// </example>
        public void CreateObjectPool(Action buildPoolAction) => StartCoroutine(AddObjectPoolCore(buildPoolAction));

        /// <summary>
        /// Move object to the created pool scene, only when set in the Game Setttings Data.
        /// </summary>
        public void AddObjectToPoolScene(GameObject go)
        {
            if (_useObjectPoolScene)
                GameObjectPool.MoveToPoolScene(go);
        }

        IEnumerator AddObjectPoolCore(Action action)
        {
            while (!GameObjectPool.ObjectPoolSceneLoaded)
                yield return null;

            action();
        }

        /// <summary>
        /// Creates an object pool in a separate designated scene. 
        /// </summary>
        public GameObjectPool CreateObjectPool(GameObject prefab, int initialCapacity, int maxCapacity = 1000)
            => GameObjectPool.Build(prefab, initialCapacity, maxCapacity, true);

        /// <summary>
        /// Creates an object pool in the active scene.
        /// </summary>
        public GameObjectPool CreateLocalObjectPool(GameObject prefab, int initialCapacity, int maxCapacity = 1000)
            => GameObjectPool.Build(prefab, initialCapacity, maxCapacity);

        /// <summary>
        /// Build the effect object pools (in a separate scene when set in the Game Settings Data). 
        /// </summary>
        protected virtual void Initialize() => StartCoroutine(_effectsData.BuildPools(_useObjectPoolScene));

        public void PlayEffect(Effect effect, Vector3 position, float scale = 1f, ObjectLayer layer = ObjectLayer.Effects)
        {
            _effectsData.StartEffect(effect, position, scale, layer);
            ClearEffect();
        }

        public void PlayEffect(Effect effect, Vector3 position, Quaternion rotation, float scale = 1f, ObjectLayer layer = ObjectLayer.Effects)
        {
            _effectsData.StartEffect(effect, position, rotation, scale, layer);
            ClearEffect();
        }

        void ClearEffect()
        {
            if (_effectsData.ClearEffectsActive)
                StartCoroutine(_effectsData.ClearEffects());
        }

        #endregion

        #region Game Announcer
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
        #endregion

        #region Camera Switcher
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

        #endregion

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