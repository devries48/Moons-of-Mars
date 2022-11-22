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
        [SerializeField] AudioClip hyperJumpActivated;
        [SerializeField] AudioClip hyperJumpComplete;
        [SerializeField] AudioClip jumpActivate;
        [SerializeField] AudioClip fuelLow;
        [SerializeField] AudioClip fuelEmpty;
        [SerializeField] AudioClip deactivate;
        [SerializeField] AudioClip lighsOn;
        [SerializeField] AudioClip lighsOff;

        public enum Clip
        {
            shieldActivated,
            firerateIncreased,
            shotSpreadActivated,
            hyperJumpActivated,
            hyperJumpComplete,
            jumpActivate,
            fuelLow,
            fuelEmpty,
            lightsOn,
            lightsOff,
            deactivate
        }

        public void PlayClip(Clip clip, bool isAlarm = false)
        {
            var audioClip = clip switch
            {
                Clip.shieldActivated => shieldActivated,
                Clip.firerateIncreased => firerateIncreased,
                Clip.shotSpreadActivated => shotSpreadActivated,
                Clip.hyperJumpActivated => hyperJumpActivated,
                Clip.hyperJumpComplete => hyperJumpComplete,
                Clip.jumpActivate => jumpActivate,
                Clip.fuelLow => fuelLow,
                Clip.fuelEmpty => fuelEmpty,
                Clip.lightsOn => lighsOn,
                Clip.lightsOff => lighsOff,
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