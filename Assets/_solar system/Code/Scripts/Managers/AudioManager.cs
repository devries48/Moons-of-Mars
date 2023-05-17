using MoonsOfMars.Shared;
using UnityEngine;

namespace MoonsOfMars.SolarSystem
{
    public class AudioManager : AudioManagerBase
    {
        [Header("MUSIC CLIPS")]
        [SerializeField] AudioClip[] _music;

        enum MusicLevel { none, menu }

        protected override AudioClip GetMusicClip(int level)
        {
            if (_music == null || _music.Length == 0)
                return null;

            return _music[Random.Range(0, _music.Length)];
        }

        protected override void SelectMusicTrack()
        {
            var level = (MusicLevel)CurrentLevel;
            if (level == MusicLevel.none)
            {
                PlayMusic((int)MusicLevel.menu);
                return;
            }

            if (GameManager.Instance.IsGameQuit)
                StopMusic();
        }

    }
}