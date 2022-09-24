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
            Explode
        }

        [Header("Clips")]
        [SerializeField] AudioClip powerupEject;
        [SerializeField] AudioClip powerupExplode;

        public void PlayClip(Clip clip, AudioSource audioSource)
        {
            var audioClip = clip switch
            {
                Clip.Eject => powerupEject,
                Clip.Explode => powerupExplode,
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