using UnityEngine;

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
        AstroidsGameManager GameManager
        {
            get
            {
                if (__gameManager == null)
                    __gameManager = AstroidsGameManager.Instance;

                return __gameManager;
            }
        }
        AstroidsGameManager __gameManager;

        AudioSource UiAudio => GameManager.m_AudioSource;
        #endregion

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

        public bool AudioIsPlaying => UiAudio.isPlaying;

        AudioClip RandomClip(AudioClip[] clips)
        {
            if (clips == null || clips.Length == 0)
                return null;

            return clips[Random.Range(0, clips.Length)];
        }

        void PlayAudioClip(AudioClip clip)
        {
            if (clip && UiAudio)
                UiAudio.PlayOneShot(clip);
        }

    }
}