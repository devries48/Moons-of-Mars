using UnityEngine;
using static Game.SpaceShooter.WeaponsController;

namespace Game.SpaceShooter
{
    [RequireComponent(typeof(AudioSource))]
    public class HardpointController : MonoBehaviour
    {
        public HardpointType m_Type;
        AudioSource _audioSource;

        void Start() => _audioSource = GetComponent<AudioSource>();
        public void PlayAudioClip() => _audioSource.Play();

    }
}