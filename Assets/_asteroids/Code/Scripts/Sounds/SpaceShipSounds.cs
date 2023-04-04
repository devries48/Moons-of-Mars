using System;
using UnityEngine;

namespace Game.Asteroids
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
            spawn
        }

        [SerializeField] AudioSource clipsAudioSource;

        [Header("Clips")]
        [SerializeField] AudioClip shootBullet;
        [SerializeField] AudioClip shieldsUp;
        [SerializeField] AudioClip shieldsDown;
        [SerializeField] AudioClip shieldsHit;
        [SerializeField] AudioClip shipExplosion;
        [SerializeField] AudioClip spawn;

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
                _ => null
            };

            PlayClip(audioClip);
        }

        void PlayClip(AudioClip clip)
        {
            if (clip !=null)
                clipsAudioSource.PlayOneShot(clip);
        }
    }
}