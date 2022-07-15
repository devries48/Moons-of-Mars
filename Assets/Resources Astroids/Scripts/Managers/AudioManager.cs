using System.Collections.Generic;
using UnityEngine;

namespace Game.Astroids
{
    public class AudioManager : MonoBehaviour
    {
        [SerializeField, Tooltip("The number of maximum sounds to allow playing simultaneously")]
        int maxSoundPlaying = 2;
        
        readonly List<AudioSource> soundsPlaying = new();

        public void PlaySound(AudioSource sound)
        {
            int i = 0;

            while (i < soundsPlaying.Count)
            {
                if (soundsPlaying[i] != null && soundsPlaying[i].isPlaying)
                    i++;
                else
                    soundsPlaying.RemoveAt(i);
            }

            if (i < maxSoundPlaying)
            {
                // Then start the sound and add it to the list
                sound.Play();
                soundsPlaying.Add(sound);
            }
        }
    }
}