using System;
using UnityEngine;

namespace Game.Astroids
{
    [Serializable]
    public class PowerupSounds
    {
        public enum Clip
        {
            Eject,
            Explode,
            Pickup,
            PickupEnemy
        }

        [Header("Clips")]
        [SerializeField] AudioClip eject;
        [SerializeField] AudioClip explode;
        [SerializeField] AudioClip pickup;
        [SerializeField] AudioClip pickupEnemy;

        public void PlayClip(Clip clip, AudioSource audioSource)
        {
            var audioClip = clip switch
            {
                Clip.Eject => eject,
                Clip.Explode => explode,
                Clip.Pickup => pickup,
                Clip.PickupEnemy => pickupEnemy,
                _ => null
            };

            PlayClip(audioClip, audioSource);
        }

        void PlayClip(AudioClip clip, AudioSource audioSource)
        {
            if (clip)
                audioSource.PlayOneShot(clip);
        }

    }
}