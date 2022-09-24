using System;
using UnityEngine;

namespace Game.Astroids
{
    [Serializable]
    public class UISounds
    {
        public enum Clip
        {
            ScorePlus,
            ScoreMinus
        }

        [SerializeField] AudioSource clipsAudioSource;

        [Header("Clips")]
        [SerializeField] AudioClip scorePlus;
        [SerializeField] AudioClip scoreMinus;

        public void PlayClip(Clip clip)
        {
            var audioClip = clip switch
            {
                Clip.ScorePlus => scorePlus,
                Clip.ScoreMinus => scoreMinus,
                _ => null
            };

            PlayClip(audioClip);
        }

        void PlayClip(AudioClip clip)
        {
            if (clip && clipsAudioSource)
                clipsAudioSource.PlayOneShot(clip);
        }
    }
}