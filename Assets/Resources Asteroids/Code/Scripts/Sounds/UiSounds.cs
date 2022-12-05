﻿using UnityEngine;

namespace Game.Astroids
{
    [System.Serializable]
    public class UISounds
    {
        public enum Clip
        {
            scorePlus,
            scoreMinus,
            gameStart,
            levelComplete,
            gameOver
        }

        [Header("Clips")]
        [SerializeField] AudioClip scorePlus;
        [SerializeField] AudioClip scoreMinus;
        [SerializeField] AudioClip[] gameStart;
        [SerializeField] AudioClip[] levelComplete;
        [SerializeField] AudioClip[] gameOver;

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

        public AudioSource m_UiAudio;

        public void PlayClip(Clip clip)
        {
            var audioClip = clip switch
            {
                Clip.scorePlus => scorePlus,
                Clip.scoreMinus => scoreMinus,
                Clip.gameStart => RandomClip(gameStart),
                Clip.levelComplete => RandomClip(levelComplete),
                Clip.gameOver => RandomClip(gameOver),
                _ => null
            };
            PlayAudioClip(audioClip);
        }

        public bool IsPlaying() => m_UiAudio.isPlaying;

        AudioClip RandomClip(AudioClip[] clips)
        {
            if (clips == null || clips.Length == 0)
                return null;

            return clips[Random.Range(0, clips.Length)];
        }

        void PlayAudioClip(AudioClip clip)
        {
            if (clip && m_UiAudio)
                m_UiAudio.PlayOneShot(clip);
        }

    }
}