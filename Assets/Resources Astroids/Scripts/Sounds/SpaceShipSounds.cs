using System;
using UnityEngine;

namespace Game.Astroids
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
            shipExplosion
        }

        [SerializeField] AudioSource clipsAudioSource;

        [Header("Clips")]
        [SerializeField] AudioClip shootBullet;
        [SerializeField] AudioClip shieldsUp;
        [SerializeField] AudioClip shieldsDown;
        [SerializeField] AudioClip shieldsHit;
        [SerializeField] AudioClip shipExplosion;

        public void PlayClip(Clip clip)
        {
            var audioClip = clip switch
            {
                Clip.shootBullet => shootBullet,
                Clip.shieldsUp => shieldsUp,
                Clip.shieldsDown => shieldsDown,
                Clip.shieldsHit => shieldsHit,
                Clip.shipExplosion => shipExplosion,
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