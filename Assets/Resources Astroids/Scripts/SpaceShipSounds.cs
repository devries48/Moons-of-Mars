using UnityEngine;

namespace Game.Astroids
{
    [System.Serializable]
    public class SpaceShipSounds
    {
        public enum Clip
        {
            ShootBullet,
            ShieldsUp,
            ShieldsDown,
            ShieldsHit
        }

        public AudioClip ShootBullet;
        public AudioClip ShieldsUp;
        public AudioClip ShieldsDown;
        public AudioClip ShieldsHit;
    }
}