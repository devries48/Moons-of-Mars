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

        public void GameStart()
        {
            scoreTextUI.gameObject.SetActive(true);
        }

        public IEnumerator GameOver()
        {
            Announce.GameOver();
            yield return AstroidsGameManager.Wait(2);
            PlayAudio(UISounds.Clip.gameOver);
            
            Score.Reset();
            Reset();
        }

        public void Reset()
        {
            Announce.ClearAnnouncements();
        }

        public void PlayAudio(UISounds.Clip clip) => uiSounds.PlayClip(clip);

        public IEnumerator PlayDelayedAudio(UISounds.Clip clip, float delay)
        {
            yield return new WaitForSeconds(delay);
            PlayAudio(clip);
        }
    }
}
