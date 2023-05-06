using System;
using UnityEngine;

namespace MoonsOfMars.Game.Asteroids
{
    [Serializable]
    public class SpaceShipSounds
    {
        public enum Clip
        {
            shootBullet,
            shieldsUp,
            shieldsDown,
            shieldsHit,
            shipExplosion,
            spawn,
            warp
        }

        [SerializeField] AudioSource clipsAudioSource;

        [Header("Clips")]
        [SerializeField] AudioClip shootBullet;
        [SerializeField] AudioClip shieldsUp;
        [SerializeField] AudioClip shieldsDown;
        [SerializeField] AudioClip shieldsHit;
        [SerializeField] AudioClip shipExplosion;
        [SerializeField] AudioClip spawn;
        [SerializeField] AudioClip warp;

        public void PlayClip(Clip clip)
        {
            var audioClip = clip switch
            {
                Clip.shootBullet => shootBullet,
                Clip.shieldsUp => shieldsUp,
                Clip.shieldsDown => shieldsDown,
                Clip.shieldsHit => shieldsHit,
                Clip.shipExplosion => shipExplosion,
                Clip.spawn => spawn,
                Clip.warp => warp,
                _ => null
            };

            PlayClip(audioClip);
        }

        public bool IsAudioPlaying => clipsAudioSource.isPlaying;

        void PlayClip(AudioClip clip)
        {
            if (clip !=null)
                clipsAudioSource.PlayOneShot(clip);
        }

    }
}