using UnityEngine;

namespace MoonsOfMars.Shared
{
    public class GameManagerBase<T> : SingletonBase<T> where T : MonoBehaviour
    {
        [Header("Generic Managers")]
        [SerializeField] protected AudioManagerBase _audioManager;
        [SerializeField] protected InputManagerBase _inputManager;

        /// <summary>
        /// Usage: public AudioManager AudioManager => AudioManager<AudioManager>();
        /// </summary>
        public TaudioManager GetAudioManager<TaudioManager>() where TaudioManager : AudioManagerBase => (TaudioManager)_audioManager;

        /// <summary>
        /// Usage: public InputManager InputManager => InputManager<AudioManager>();
        /// </summary>
        public TinputManager GetInputManager<TinputManager>() where TinputManager : InputManagerBase => (TinputManager)_inputManager;
    }
}