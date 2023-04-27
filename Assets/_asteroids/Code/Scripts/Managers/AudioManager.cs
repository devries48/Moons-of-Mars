using MoonsOfMars.Shared;
using System;
using UnityEngine;
using static MusicData;

namespace MoonsOfMars.Game.Asteroids
{
    public class AudioManager : AudioManager_MonoBehaviour
    {
        #region editor fields
        [SerializeField] MusicData musicData;

        [SerializeField] int startGameFade = 8;
        [SerializeField] int inGameFade = 4;

        [Header("Enemy count triggers")]
        [SerializeField, Tooltip("raise to medium at"), Range(1, 100)] int mediumIntensityStart = 5;
        [SerializeField, Tooltip("raise lower at"), Range(1, 100)] int mediumIntensityStop = 4;
        [SerializeField, Tooltip("raise higher at"), Range(1, 100)] int highIntensityStart = 10;
        [SerializeField, Tooltip("lower medium at"), Range(1, 100)] int highIntensityStop = 9;
        #endregion

        #region properties
        AsteroidsGameManager GameManager
        {
            get
            {
                if (__gameManager == null)
                    __gameManager = AsteroidsGameManager.GmManager;

                return __gameManager;
            }
        }
        AsteroidsGameManager __gameManager;
        #endregion

        #region fields
        MusicLevel CurrentMusicLevel
        {
            get => (MusicLevel)CurrentLevel;
            set => CurrentLevel = (int)value;
        }

        int _prevIntensity;
        #endregion

        protected override void SelectMusicTrack()
        {
            // Set initial music
            //if (GameManager.IsGameActive && CurrentMusicLevel == MusicLevel.none)
            //{
            //    PlayMusic(MusicLevel.menu);
            //    return;
            //}

            if (GameManager.IsGameStageComplete)
            {
                if (CurrentMusicLevel != MusicLevel.stage)
                    PlayMusic(MusicLevel.stage);
            }
            else if (GameManager.IsGamePaused)
            {
                if (CurrentMusicLevel != MusicLevel.pause)
                    PlayMusic(MusicLevel.pause);
            }
            else if (GameManager.IsGameInMenu)
            {
                 if( CurrentMusicLevel != MusicLevel.menu)
                    PlayMusic(MusicLevel.menu);
            }

            else if (GameManager.IsGameExit)
                StopMusic();
            else if (GameManager.IsGamePlaying)
                CheckGameIntensity();
        }

        protected override AudioClip GetMusicClip(int level)
        {
            return musicData.GetMusicClip((MusicLevel)level);
        }

        void CheckGameIntensity()
        {
            if (GameManager.m_debug.OverrideMusic)
            {
                print("OverrideMusic");
                if (GameManager.m_debug.Level != CurrentMusicLevel)
                {
                    print("Current Level (debug): " + CurrentMusicLevel + " - new: " + GameManager.m_debug.Level);
                    PlayMusic(GameManager.m_debug.Level);
                }
                return;
            }

            var newLevel = MusicLevel.low;
            var intensity = GetCurrentIntensity();

            if (intensity == _prevIntensity)
                return;

            if (CurrentMusicLevel == MusicLevel.low && intensity >= mediumIntensityStart)
                newLevel = MusicLevel.medium;

            if (CurrentMusicLevel == MusicLevel.medium)
            {
                if (intensity <= mediumIntensityStop) newLevel = MusicLevel.low;
                if (intensity <= highIntensityStart) newLevel = MusicLevel.high;
            }

            if (CurrentMusicLevel == MusicLevel.high && intensity <= highIntensityStop)
                newLevel = MusicLevel.low;

            if (CurrentMusicLevel != newLevel)
            {
                print("Current Level: " + CurrentMusicLevel + " - new level: " + newLevel);
                PlayMusic(newLevel);
            }

            _prevIntensity = intensity;
        }

        int GetCurrentIntensity()
        {
            var lvl = GameManager.m_LevelManager;
            return lvl.AsteroidsActive + lvl.UfosActive;
        }

        void PlayMusic(MusicLevel level)
        {
            if (level != MusicLevel.low && level != MusicLevel.medium && level != MusicLevel.high)
                _prevIntensity = -1;

            var timeToFade = CurrentMusicLevel == MusicLevel.none ? startGameFade : inGameFade;

            PlayMusic((int)level, timeToFade);
        }
    }

}
