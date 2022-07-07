using System.Collections.Generic;
using UnityEngine;

namespace Game.Astroids
{
    public class AudioManager : MonoBehaviour
    {
        [SerializeField, Tooltip("The number of maximum sounds to allow playing simultaneously")]
        public int maxSoundPlaying = 2;

        List<AudioSource> soundsPlaying = new();

        public void PlaySound(AudioSource sound)
        {
            // First remove all the sounds that are done playing
            int i = 0;
            while (i < soundsPlaying.Count)
            {
                if (soundsPlaying[i].isPlaying)
                    i++;
                else
                    soundsPlaying.RemoveAt(i);
            }

            // Then start the sound and add it to the list
            sound.Play();
            soundsPlaying.Add(sound);

            // Check if there is too much sound playing, if yes remove the first one of the list
            if (soundsPlaying.Count > maxSoundPlaying)
            {
                soundsPlaying[0].Stop();
                soundsPlaying.RemoveAt(0);
            }
        }
    }
}