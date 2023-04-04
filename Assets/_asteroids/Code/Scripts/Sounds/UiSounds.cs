using UnityEngine;

namespace Game.Asteroids
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
            stageComplete,
            gameOver,
        }

        [Header("Clips")]
        [SerializeField] AudioClip scorePlus;
        [SerializeField] AudioClip scoreMinus;
        [SerializeField] AudioClip[] gameStart;
        [SerializeField] AudioClip[] levelComplete;
        [SerializeField] AudioClip[] stageComplete;
        [SerializeField] AudioClip[] gameOver;

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

        public AudioSource m_UiAudio;

        public void PlayClip(Clip clip)
        {
            var audioClip = clip switch
            {
                Clip.scorePlus => scorePlus,
                Clip.scoreMinus => scoreMinus,
                Clip.gameStart => RandomClip(gameStart),
                Clip.levelComplete => RandomClip(levelComplete),
                Clip.stageComplete => RandomClip(stageComplete),
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