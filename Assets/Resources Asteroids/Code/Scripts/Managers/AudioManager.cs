using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using static MusicData;

namespace Game.Astroids
{
    public class AudioManager : MonoBehaviour
    {
        #region editor fields
        [SerializeField] MusicData musicData;
        [SerializeField] AudioMixerGroup musicMixerGroup;
        [SerializeField] AudioMixerGroup backgroundFxMixerGroup;

        [Header("Time to Fade")]
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
                    __gameManager = AsteroidsGameManager.Instance;

                return __gameManager;
            }
        }
        AsteroidsGameManager __gameManager;
        #endregion

        #region fields
        readonly float _checkPeriod = .5f;
        float _endGameFade;

        bool _isPlayingFirstAudioSource;
        float _nextActionTime = 0.0f;

        List<AudioSource> _audioSources;
        List<MusicTrack> _musicTracks;
        MusicLevel _currentLevel = MusicLevel.none;
        MusicTrack _currentTrack;
        int _prevIntensity;
        float _CurrentBackgroundSfxVolume;
        #endregion

        void Awake() => CreateAudioSources();

        void Update()
        {
            if (Time.time <= _nextActionTime)
                return;

            _nextActionTime += _checkPeriod;

            // Set initial music
            if (GameManager.IsGameActive && _currentLevel == MusicLevel.none)
            {
                PlayMusic(MusicLevel.menu);
                return;
            }
            if (GameManager.IsGameStageComplete && _currentLevel != MusicLevel.stage)
                PlayMusic(MusicLevel.stage);
            else if (GameManager.IsGamePlaying)
                CheckGameIntensity();
            else if (!GameManager.IsGameActive)
                StopMusic();

            if (_currentTrack == null) return;

            if (_currentTrack.IsTrackEnding())
                PlayMusic(_currentLevel);
        }

        public void FadeOutBackgroundSfx()
        {
            _CurrentBackgroundSfxVolume = FadeMixerGroup.GetCurrentVolume(backgroundFxMixerGroup.audioMixer, FadeMixerGroup.s_BACKGROUND_VOL);
            StartCoroutine(FadeMixerGroup.StartFade(backgroundFxMixerGroup.audioMixer, FadeMixerGroup.s_BACKGROUND_VOL, 1, 0));
        }

        public void FadeInBackgroundSfx()
        {
            StartCoroutine(FadeMixerGroup.StartFade(backgroundFxMixerGroup.audioMixer, FadeMixerGroup.s_BACKGROUND_VOL, 1, _CurrentBackgroundSfxVolume));
        }

        void CheckGameIntensity()
        {
            if (GameManager.m_debug.OverrideMusic)
            {
                print("OverrideMusic");
                if (GameManager.m_debug.Level != _currentLevel)
                {
                    print("Current Level (debug): " + _currentLevel + " - new: " + GameManager.m_debug.Level);
                    PlayMusic(GameManager.m_debug.Level);
                }
                return;
            }

            var newLevel = MusicLevel.low;
            var intensity = GetCurrentIntensity();

            if (intensity == _prevIntensity)
                return;

            if (_currentLevel == MusicLevel.low && intensity >= mediumIntensityStart)
                newLevel = MusicLevel.medium;

            if (_currentLevel == MusicLevel.medium)
            {
                if (intensity <= mediumIntensityStop) newLevel = MusicLevel.low;
                if (intensity <= highIntensityStart) newLevel = MusicLevel.high;
            }

            if (_currentLevel == MusicLevel.high && intensity <= highIntensityStop)
                newLevel = MusicLevel.low;

            if (_currentLevel != newLevel)
            {
                print("Current Level: " + _currentLevel + " - new: " + newLevel);
                PlayMusic(newLevel);
            }

            _prevIntensity = intensity;
        }

        int GetCurrentIntensity()
        {
            var lvl = GameManager.m_level;
            return lvl.AstroidsActive + lvl.TotalUfosActive;
        }

        void PlayMusic(MusicLevel level)
        {
            if (level != MusicLevel.low && level != MusicLevel.medium && level != MusicLevel.high)
                _prevIntensity = -1;

            _isPlayingFirstAudioSource = !_isPlayingFirstAudioSource;
            var timeToFade = _currentLevel == MusicLevel.none ? startGameFade : inGameFade;

            StopAllCoroutines();
            StartCoroutine(FadeClip(level, timeToFade));

            _currentLevel = level;
        }

        void StopMusic() => StartCoroutine(StopClip(_currentLevel, _endGameFade));

        IEnumerator FadeClip(MusicLevel level, float timeToFade)
        {
            _currentTrack = GetMusicTrack(level);

            var timeElapsed = 0f;
            var fadeInSource = GetAudioSource(_isPlayingFirstAudioSource);
            var fadeOutSource = GetAudioSource(!_isPlayingFirstAudioSource);

            _currentTrack.Play(fadeInSource);

            while (timeElapsed < timeToFade)
            {
                fadeInSource.volume = 1 * (timeElapsed / timeToFade);
                if (fadeOutSource.clip != null)
                    fadeOutSource.volume = 1 - (1 * (timeElapsed / timeToFade));

                timeElapsed += Time.deltaTime;
                yield return null;
            }

            if (fadeOutSource.clip != null)
                _currentTrack.Stop(fadeOutSource);

            yield return null;
        }

        IEnumerator StopClip(MusicLevel level, float timeToFade)
        {
            _currentTrack = GetMusicTrack(level);

            var timeElapsed = 0f;
            var fadeOutSource = GetAudioSource(_isPlayingFirstAudioSource);

            if (fadeOutSource.clip != null)
            {
                while (timeElapsed < timeToFade)
                {
                    fadeOutSource.volume = 1 - (1 * (timeElapsed / timeToFade));
                    //print("vol: " + fadeOutSource.volume + "time: " + timeElapsed);
                    timeElapsed += Time.deltaTime;
                    yield return null;
                }

                _currentTrack.Stop(fadeOutSource);
                print("stoptrack");
            }
            yield return null;
        }

        void CreateAudioSources()
        {
            _endGameFade = TweenUtil.m_timeMenuOpenClose - _checkPeriod;
            _audioSources = new List<AudioSource>();
            _musicTracks = new List<MusicTrack>();

            AudioSource audioSource;

            for (int i = 0; i < 2; i++)
            {
                var obj = new GameObject($"Source{i + 1}", typeof(AudioSource));
                audioSource = obj.GetComponent<AudioSource>();
                audioSource.outputAudioMixerGroup = musicMixerGroup;
                audioSource.volume = 0;

                _audioSources.Add(audioSource);
            }
        }

        AudioSource GetAudioSource(bool isFirst) => isFirst ? _audioSources.First() : _audioSources.Last();

        MusicTrack GetMusicTrack(MusicLevel level)
        {
            var track = _musicTracks.FirstOrDefault(t => t.Level == level);
            if (track != null)
                return track;

            track = new MusicTrack(this, level);
            return track;
        }

        class MusicTrack
        {
            public MusicTrack(AudioManager manager, MusicLevel level)
            {
                _manager = manager;
                _level = level;

                SetClip();
            }

            readonly MusicLevel _level;
            readonly AudioManager _manager;
            AudioSource _activeAudio;

            AudioClip _clip;
            float _time;

            public MusicLevel Level { get => _level; }

            public void Stop(AudioSource audioSource)
            {
                _time = audioSource.time;
                audioSource.Stop();

                // reset clip when there's little time left
                if (_clip.length < _time + _manager.inGameFade * 4)
                {
                    _time = 0;
                    SetClip();
                }
            }

            public void Play(AudioSource fadeInSource)
            {
                _activeAudio = fadeInSource;

                fadeInSource.clip = _clip;
                fadeInSource.time = _time;
                fadeInSource.Play();
                print("Play music: " + _clip.name + " time: " + _time);
            }

            public bool IsTrackEnding() => _activeAudio != null && _clip.length - _activeAudio.time - _manager.inGameFade - 1 < 0;

            void SetClip() => _clip = _manager.musicData.GetMusicClip(Level);
        }
    }

}
