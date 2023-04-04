using System;
using UnityEngine;

namespace Game.Asteroids
{
    [Serializable]
    public class PowerupSounds
    {
        public enum Clip
        {
            Eject,
            Explode,
            PickupEnemy
        }

        [Header("Clips")]
        [SerializeField] AudioClip eject;
        [SerializeField] AudioClip explode;
        [SerializeField] AudioClip pickupEnemy;

        public void PlayClip(Clip clip, AudioSource audioSource)
        {
            var audioClip = clip switch
            {
                Clip.Eject => eject,
                Clip.Explode => explode,
                Clip.PickupEnemy => pickupEnemy,
                _ => null
            };

            PlayClip(audioClip, audioSource);
        }

        void PlayClip(AudioClip clip, AudioSource audioSource)
        {
            if (clip !=null)
                audioSource.PlayOneShot(clip);
        }

    }
}