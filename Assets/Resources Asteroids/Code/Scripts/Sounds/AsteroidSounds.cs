using UnityEngine;

namespace Game.Astroids
{
    [System.Serializable]
    public class AsteroidSounds
    {
        public enum Clip
        {
            Collide,
            Explode
        }

        [Header("Clips")]
        [SerializeField] AudioClip collide;
        [SerializeField] AudioClip explode;

        [Header("Volume")]
        [SerializeField, Range(0f, 1f)] float smallAstroidVol = .1f;
        [SerializeField, Range(0f, 1f)] float mediumAstroidVol = .2f;
        [SerializeField, Range(0f, 1f)] float largeAstroidVol = .4f;

        internal AudioClip GetClip(Clip clip)
        {
            return clip switch
            {
                Clip.Collide => collide,
                Clip.Explode => explode,
                _ => null
            };
        }

        internal void SetVolume(AudioSource audioSource, float generation, bool isCollision = false)
        {
            var volume = generation switch
            {
                1 => largeAstroidVol,
                2 => mediumAstroidVol,
                _ => smallAstroidVol,
            };
            if (isCollision) volume *= .75f;
            audioSource.volume = volume;
        }
    }
}
