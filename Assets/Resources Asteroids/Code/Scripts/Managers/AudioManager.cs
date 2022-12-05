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

        [Header("Time to Fade")]
        [SerializeField] int startGameFade = 8;
        [SerializeField] int inGameFade = 4;
        [SerializeField] int endGameFade = 2;

        [Header("Enemy count triggers")]
        [SerializeField, Tooltip("raise to medium at"), Range(1, 100)] int mediumIntensityStart = 5;
        [SerializeField, Tooltip("raise lower at"), Range(1, 100)] int mediumIntensityStop = 4;
        [SerializeField, Tooltip("raise higher at"), Range(1, 100)] int highIntensityStart = 10;
        [SerializeField, Tooltip("lower medium at"), Range(1, 100)] int highIntensityStop = 9;

        [Header("Ufo enemy value")]
        [SerializeField] int ufoGreen = 2;
        [SerializeField] int ufoRed = 3;

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

        bool _isPlayingFirstAudioSource;
        float _nextActionTime = 0.0f;

        List<AudioSource> _audioSources;
        List<MusicTrack> _musicTracks;
        MusicLevel _currentLevel = MusicLevel.none;
        MusicTrack _currentTrack;

        #endregion

        void Awake() => CreateAudioSources();

        void Update()
        {
            if (Time.time <= _nextActionTime)
                return;

            _nextActionTime += _checkPeriod;

            if (GameManager.m_gamePlaying)
            {
                if (_currentLevel != MusicLevel.menu)
                {
                    PlayMusic(MusicLevel.menu);
                }
            }

            if (_currentTrack != null && _currentTrack.IsTrackEnding())
                PlayMusic(_currentLevel);

            ////else if (_currentTrack != MusicTrack.low && IsLowIntensity)
            //{

            //}



        }

        //bool IsLowIntensity => GameManager.m_level.AstroidsActive + ;
        void PlayMusic(MusicLevel level)
        {
            StopAllCoroutines();

            var timeToFade = _currentLevel == MusicLevel.none ? startGameFade : inGameFade;

            StartCoroutine(FadeClip(level, timeToFade));

            _currentLevel = level;
            _isPlayingFirstAudioSource = !_isPlayingFirstAudioSource;
        }

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

        void CreateAudioSources()
        {
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

            public bool IsTrackEnding()
            {
                return _activeAudio != null && _clip.length - _activeAudio.time - 1 < 0;
            }

            void SetClip() => _clip = _manager.musicData.GetMusicClip(Level);
        }
    }

}
