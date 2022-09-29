using UnityEngine;
using TMPro;
using System.Collections;

namespace Game.Astroids
{
    [System.Serializable]
    public class UIManager
    {
        [SerializeField]
        GameObject mainMenuWindow;

        [SerializeField]
        TextMeshProUGUI scoreTextUI;

        [SerializeField]
        TextMeshProUGUI announcerTextUI;

        [SerializeField]
        UISounds uiSounds = new();

        // Show mainMenu directly the first run.
        bool _firstRun = true; 
       
        GameAnnouncer Announce
        {
            get
            {
                if (__announce == null)
                    __announce = GameAnnouncer.AnnounceTo(Announcer.TextComponent(announcerTextUI));

                return __announce;
            }
        }
        GameAnnouncer __announce;

        // Set window in start position & hide ui items
        public void ResetUI()
        {
            scoreTextUI.gameObject.SetActive(false);
            TweenUtil.MenuWindowClose(mainMenuWindow, true);
        }

        //TODO while loop and return selected
        public IEnumerator ShowMainMenu()
        {
            if (_firstRun)
                _firstRun = false;
            else
                yield return AstroidsGameManager.Wait(2);
            
            scoreTextUI.gameObject.SetActive(false);
            TweenUtil.MenuWindowOpen(mainMenuWindow);

            yield return null;
        }

        public int HideMainMenu(bool startGame = true)
        {
            scoreTextUI.gameObject.SetActive(startGame);
            return TweenUtil.MenuWindowClose(mainMenuWindow);
        }

        public void LevelStarts(int level)
        {
            if (level == 1)
            {
                Announce.GameStart();
                PlayAudio(UISounds.Clip.gameStart);
            }
            else
                Announce.LevelStarts(level);
        }

        public void LevelPlay()
        {
            Announce.LevelPlaying();
        }

        public IEnumerator LevelCleared(int level)
        {
            Announce.LevelCleared();
            uiSounds.PlayClip(UISounds.Clip.levelComplete);
            yield return AstroidsGameManager.Wait(1);
            Score.LevelCleared(level);
        }

        public IEnumerator GameOver()
        {
            Announce.GameOver();
            yield return AstroidsGameManager.Wait(2);

            PlayAudio(UISounds.Clip.gameOver);
            while (uiSounds.AudioIsPlaying)
                yield return null;

            yield return null ;
            Reset();
        }

        public void Reset()
        {
            Announce.ClearAnnouncements();
            Score.Reset();
        }

        public void PlayAudio(UISounds.Clip clip) => uiSounds.PlayClip(clip);

        public IEnumerator PlayDelayedAudio(UISounds.Clip clip, float delay)
        {
            yield return new WaitForSeconds(delay);
            PlayAudio(clip);
        }
    }
}
