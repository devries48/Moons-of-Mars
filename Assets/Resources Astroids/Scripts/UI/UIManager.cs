using UnityEngine;
using TMPro;
using System;

namespace Game.Astroids
{
    [Serializable]
    public class UIManager
    {
        [SerializeField]
        GameObject mainMenuWindow;

        [SerializeField]
        TextMeshProUGUI scoreTextUI;

        [SerializeField]
        TextMeshProUGUI announcerTextUI;

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

        public void ShowMainMenu()
        {
            scoreTextUI.gameObject.SetActive(false);
            TweenUtil.MenuWindowOpen(mainMenuWindow);
        }

        public void HideMainMenu(bool startGame = true)
        {
            scoreTextUI.gameObject.SetActive(startGame);
            TweenUtil.MenuWindowClose(mainMenuWindow);
        }

        public void LevelStarts(int level)
        {
            Announce.LevelStarts(level);
        }

        public void LevelPlay()
        {
            Announce.LevelPlaying();
        }

        public void LevelCleared()
        {
            Announce.LevelCleared();
        }

        public void GameStart()
        {
            scoreTextUI.gameObject.SetActive(true);
        }

        public void GameOver()
        {
            Announce.GameOver();
        }

        public void Reset()
        {
            Announce.ClearAnnouncements();
        }
    }
}