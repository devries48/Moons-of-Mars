using System;
using UnityEngine;

namespace Game.Astroids
{
    [Serializable]
    public class SpaceShipSounds
    {
        public enum Clip
        {
            ShootBullet,
            ShieldsUp,
            ShieldsDown,
            ShieldsHit,
            ShipExplosion
        }

        [SerializeField]
        AudioSource clipsAudioSource;

        [Header("Clips")]
        [SerializeField] AudioClip shootBullet;
        [SerializeField] AudioClip shieldsUp;
        [SerializeField] AudioClip shieldsDown;
        [SerializeField] AudioClip shieldsHit;
        [SerializeField] AudioClip shipExplosion;

        internal void PlayClip(Clip clip)
        {
            var audioClip = clip switch
            {
                Clip.ShootBullet => shootBullet,
                Clip.ShieldsUp => shieldsUp,
                Clip.ShieldsDown => shieldsDown,
                Clip.ShieldsHit => shieldsHit,
                _ => null
            };

            if (audioClip)
                clipsAudioSource.PlayOneShot(audioClip);
        }
    }
}