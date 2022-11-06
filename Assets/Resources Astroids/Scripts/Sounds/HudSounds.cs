using UnityEngine;

namespace Game.Astroids
{
    [System.Serializable]
    public class HudSounds
    {
        [SerializeField] AudioSource hudAudioSource;
        [SerializeField] AudioSource alarmAudioSource;

        [Header("Clips")]
        [SerializeField] AudioClip shieldActivated;
        [SerializeField] AudioClip firerateIncreased;
        [SerializeField] AudioClip shotSpreadActivated;
        [SerializeField] AudioClip jumpActivated;
        [SerializeField] AudioClip fuelLow;
        [SerializeField] AudioClip fuelEmpty;
        [SerializeField] AudioClip deactivate;

        public enum Clip
        {
            shieldActivated,
            firerateIncreased,
            shotSpreadActivated,
            jumpActivated,
            fuelLow,
            fuelEmpty,
            deactivate
        }

        public void PlayClip(Clip clip, bool isAlarm = false)
        {
            var audioClip = clip switch
            {
                Clip.shieldActivated => shieldActivated,
                Clip.firerateIncreased => firerateIncreased,
                Clip.shotSpreadActivated => shotSpreadActivated,
                Clip.jumpActivated => jumpActivated,
                Clip.fuelLow => fuelLow,
                Clip.fuelEmpty => fuelEmpty,
                Clip.deactivate => deactivate,
                _ => null
            };

            if (isAlarm)
                PlayAlarmClip(audioClip);
            else
                PlayAudioClip(audioClip);
        }

        void PlayAudioClip(AudioClip clip)
        {
            if (clip && hudAudioSource)
                hudAudioSource.PlayOneShot(clip);
        }

        void PlayAlarmClip(AudioClip clip)
        {
            if (clip && alarmAudioSource)
                alarmAudioSource.PlayOneShot(clip);
        }

    }
}