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
        GameObject prefabDisplayScore;

        [SerializeField]
        Color scoreColor = Color.white;

        [SerializeField]
        Color positiveColor = Color.green;

        [SerializeField]
        Color negativeColor = Color.red;

        [SerializeField]
        string scoreFormat = "{0:000000}";

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
            scoreTextUI.color = scoreColor;
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

        internal void ScoreEarned(int points, Vector3 pos)
        {
            if (points == 0)
                return;

            SetScore(Score.Earned);

            LeanTween.scale(scoreTextUI.gameObject, new Vector3(1.5f, 1.5f, 1.5f), .5f).setEasePunch();
            LeanTween.scale(scoreTextUI.gameObject, new Vector3(1f, 1f, 1f), .2f).setDelay(.5f).setEase(LeanTweenType.easeInOutCubic);

            TweenColor(scoreColor, points > 0 ? positiveColor : negativeColor, .5f);
            TweenColor(scoreTextUI.color, scoreColor, .1f, .5f);

            if (points > 0)
                PlayDelayedAudio(UISounds.Clip.scorePlus, .2f);
            else
                PlayDelayedAudio(UISounds.Clip.scoreMinus, .2f);
        }

        void TweenColor(Color begin, Color end, float time, float delay = default)
        {
            LeanTween.value(scoreTextUI.gameObject, 0.1f, 1f, time).setDelay(delay)
                .setOnUpdate((value) =>
                {
                    scoreTextUI.color = Color.Lerp(begin, end, value);
                });
        }

        void SetScore(int points)
        {
            scoreTextUI.text = string.Format(scoreFormat, points);
        }

    }
}
