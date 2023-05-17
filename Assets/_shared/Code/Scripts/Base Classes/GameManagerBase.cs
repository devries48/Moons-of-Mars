using MoonsOfMars.Shared;
using UnityEngine;

public class GameManagerBase<T> : SingletonBase<T> where T : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField] protected AudioManagerBase _audioManager;

    public TaudioManager AudioManager<TaudioManager>() where TaudioManager : AudioManagerBase => (TaudioManager)_audioManager;
}
