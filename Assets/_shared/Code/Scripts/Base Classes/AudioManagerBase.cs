using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

namespace MoonsOfMars.Shared
{
    public abstract class AudioManagerBase : MonoBehaviour
    {
        [SerializeField] float _fadeMusicTime = 4;

        [Header("Audio Mixer Groups")]
        [SerializeField] AudioMixerGroup _musicMixerGroup;
        [SerializeField] AudioMixerGroup _backgroundFxMixerGroup;

        internal bool m_MusicStopped;

        #region fields
        readonly float _checkPeriod = .5f;
        float _endGameFade;

        bool _isPlayingFirstAudioSource;
        float _nextActionTime = 0.0f;

        List<AudioSource> _audioSources;
        List<MusicTrack> _musicTracks;
        MusicTrack _currentTrack;
        float _CurrentBackgroundSfxVolume;
        #endregion

        public int CurrentLevel { get; set; }

        void Awake() => CreateAudioSources();

        void Update()
        {
            if (Time.unscaledTime <= _nextActionTime)
                return;

            _nextActionTime += _checkPeriod;

            SelectMusicTrack();

            if (_currentTrack == null) return;

            if (_currentTrack.IsTrackEnding())
                PlayMusic(CurrentLevel);
        }

        protected void PlayMusic(int level, float fadeTime = default)
        {
            if (fadeTime == default)
                fadeTime = _fadeMusicTime;

            _isPlayingFirstAudioSource = !_isPlayingFirstAudioSource;

            StopAllCoroutines();
            StartCoroutine(FadeClip(level, fadeTime));

            CurrentLevel = level;
        }

        protected void StopMusic() => StartCoroutine(StopClip(CurrentLevel, _endGameFade));

        protected abstract void SelectMusicTrack();

        protected abstract AudioClip GetMusicClip(int level);

        public void FadeOutBackgroundSfx()
        {
            _CurrentBackgroundSfxVolume = FadeMixerGroup.GetCurrentVolume(_backgroundFxMixerGroup.audioMixer, FadeMixerGroup.s_BACKGROUND_VOL);
            StartCoroutine(FadeMixerGroup.StartFade(_backgroundFxMixerGroup.audioMixer, FadeMixerGroup.s_BACKGROUND_VOL, 1, 0));
        }

        public void FadeInBackgroundSfx()
        {
            StartCoroutine(FadeMixerGroup.StartFade(_backgroundFxMixerGroup.audioMixer, FadeMixerGroup.s_BACKGROUND_VOL, 1, _CurrentBackgroundSfxVolume));
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
                obj.transform.SetParent(transform, false);

                audioSource = obj.GetComponent<AudioSource>();
                audioSource.outputAudioMixerGroup = _musicMixerGroup;
                audioSource.volume = 0;

                _audioSources.Add(audioSource);
            }
        }

        IEnumerator FadeClip(int level, float timeToFade)
        {
            _currentTrack = GetMusicTrack(level);

            var fadeInSource = GetAudioSource(_isPlayingFirstAudioSource);
            var fadeOutSource = GetAudioSource(!_isPlayingFirstAudioSource);

            _currentTrack.Play(fadeInSource);
            var timeElapsed = 0f;

            while (timeElapsed < timeToFade)
            {
                fadeInSource.volume = 1 * (timeElapsed / timeToFade);
                if (fadeOutSource.clip != null)
                    fadeOutSource.volume = 1 - (1 * (timeElapsed / timeToFade));

                timeElapsed += Time.unscaledDeltaTime / timeToFade;
                yield return null;
            }

            if (fadeOutSource.clip != null)
                _currentTrack.Stop(fadeOutSource);

            yield return null;
        }

        IEnumerator StopClip(int level, float timeToFade)
        {
            _currentTrack = GetMusicTrack(level);

            var timeElapsed = 0f;
            var fadeOutSource = GetAudioSource(_isPlayingFirstAudioSource);

            if (fadeOutSource.clip != null)
            {
                while (timeElapsed < timeToFade)
                {
                    fadeOutSource.volume = 1 - (1 * (timeElapsed / timeToFade));
                    timeElapsed += Time.unscaledDeltaTime;
                    yield return null;
                }

                _currentTrack.Stop(fadeOutSource);
                print("stoptrack");
            }
            yield return null;
        }

        AudioSource GetAudioSource(bool isFirst) => isFirst ? _audioSources.First() : _audioSources.Last();

        MusicTrack GetMusicTrack(int level)
        {
            var track = _musicTracks.FirstOrDefault(t => t.Level == level);
            if (track != null)
                return track;

            track = new MusicTrack(this, level);
            return track;
        }

        class MusicTrack
        {
            public MusicTrack(AudioManagerBase manager, int level)
            {
                _manager = manager;
                _level = level;

                SetClip();
            }

            readonly int _level;
            readonly AudioManagerBase _manager;
            AudioSource _activeAudio;

            AudioClip _clip;
            float _time;

            public int Level { get => _level; }

            public void Stop(AudioSource audioSource)
            {
                _time = audioSource.time;
                audioSource.Stop();

                // reset clip when there's little time left
                if (_clip.length < _time + _manager._fadeMusicTime * 4)
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
                print($"MUSIC: {_clip.name} - ({_time})");
            }

            public bool IsTrackEnding() => _activeAudio != null && _clip.length - _activeAudio.time - _manager._fadeMusicTime - 1 < 0;

            void SetClip() => _clip = _manager.GetMusicClip(Level);
        }

    }

}