using UnityEngine;

namespace Game.Astroids
{
    [System.Serializable]
    public class AstroidSounds
    {
        public enum Clip
        {
            Collide,
            Explode
        }

        [SerializeField, Tooltip("Controls the sound of colliding astroids")]
        AudioManager audioManager;

        [Header("Clips")]

        [SerializeField] AudioClip collide;
        [SerializeField] AudioClip explode;

        [Header("Volume")]

        [Range(0f, 1f)]
        [SerializeField] float smallAstroid = .1f;

        [Range(0f, 1f)]
        [SerializeField] float mediumAstroid = .2f;

        [Range(0f, 1f)]
        [SerializeField] float largeAstroid = .4f;

        internal AudioClip GetClip(Clip clip)
        {
            return clip switch
            {
                Clip.Collide => collide,
                Clip.Explode => explode,
                _ => null
            };
        }

        internal void SetVolume(AudioSource audioSource,float generation)
        {
            audioSource.volume = generation switch
            {
                1 => largeAstroid,
                2 => mediumAstroid,
                _ => smallAstroid,
            };
        }
    }
}
