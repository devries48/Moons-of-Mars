using UnityEngine;
using System.Collections;
using TMPro;

namespace Game.Astroids
{
    [CreateAssetMenu(fileName = "UIManager", menuName = "UIManager")]
    public class UIManager : ScriptableObject
    {
        #region editor fields
        [Header("Prefab")]
        [SerializeField] GameObject prefabDisplayPoints;

        [Header("Colors")]
        [SerializeField] Color scoreColor = Color.white;
        [SerializeField] Color positiveColor = Color.green;
        [SerializeField] Color negativeColor = Color.red;

        [Header("Other")]
        [SerializeField] string scoreFormat = "{0:000000}";
        [SerializeField] UISounds uiSounds = new();
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

        GameAnnouncer Announce
        {
            get
            {
                if (__announce == null)
                    __announce = GameAnnouncer.AnnounceTo(Announcer.TextComponent(GameManager.m_AnnouncerTextUI));

                return __announce;
            }
        }
        GameAnnouncer __announce;

        GameObject UiMenu => GameManager?.m_MainMenuWindow;
        TextMeshProUGUI UiScore => GameManager?.m_ScoreTextUI;

        public bool AudioPlaying => uiSounds.UiAudio.isPlaying;

        #endregion

        #region fields
        // Show mainMenu directly the first run.
        bool _firstRun;
        GameObjectPool _displayPointsPool;

        #endregion

        void OnEnable()
        {
            _firstRun = true;

            BuildPools();
            SetScore(Score.Earned);
            Score.OnEarn += ScoreEarned;
        }

        void OnDisable()
        {
            Score.OnEarn -= ScoreEarned;
        }

        /// <summary>
        /// Set window in start position & hide ui items
        /// </summary>
        public void ResetUI()
        {
            UiScore.color = scoreColor;
            DisplayGameScore(false);
            TweenUtil.MenuWindowClose(UiMenu, true);
        }

        //TODO while loop and return selected
        public IEnumerator ShowMainMenu()
        {
            if (_firstRun)
                _firstRun = false;
            else
                yield return AsteroidsGameManager.Wait(2);

            DisplayGameScore(false);
            TweenUtil.MenuWindowOpen(UiMenu);

            yield return null;
        }

        public int HideMainMenu(bool startGame = true)
        {
            DisplayGameScore(startGame);
            return TweenUtil.MenuWindowClose(UiMenu);
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
           
            PlayAudio(UISounds.Clip.levelComplete);
            while (AudioPlaying)
                yield return null;

            Score.LevelCleared(level);
        }

        public IEnumerator GameOver()
        {
            Announce.GameOver();
            yield return AsteroidsGameManager.Wait(2);

            PlayAudio(UISounds.Clip.gameOver);
            while (AudioPlaying)
                yield return null;

            Reset();
        }

        void Reset()
        {
            Announce.ClearAnnouncements();
            Score.Reset();
        }

        void PlayAudio(UISounds.Clip clip) => uiSounds.PlayClip(clip);

        IEnumerator PlayDelayedAudio(UISounds.Clip clip, float delay)
        {
            yield return new WaitForSeconds(delay);
            PlayAudio(clip);
        }

        void ScoreEarned(int points, Vector3 pos)
        {
            if (points == 0)
                return;

            var color = points > 0 ? positiveColor : negativeColor;

            SetScore(Score.Earned);
            LeanTween.scale(UiScore.gameObject, new Vector3(1.5f, 1.5f, 1.5f), .5f).setEasePunch();
            LeanTween.scale(UiScore.gameObject, new Vector3(1f, 1f, 1f), .2f).setDelay(.5f).setEase(LeanTweenType.easeInOutCubic);

            TweenColor(scoreColor, color, .5f);
            TweenColor(scoreColor, color, .5f);
            TweenColor(UiScore.color, scoreColor, .1f, .5f);

            DisplayPoints(points, pos, color);
        }

        void DisplayGameScore(bool show)
        {
            GameManager.m_ScoreTextUI.gameObject.SetActive(show);
            SetScore(Score.Earned);
        }

        void DisplayPoints(int points, Vector3 pos, Color color)
        {
            var pointsObj = _displayPointsPool.GetFromPool(pos);
            TMP_Text pointsText = null;

            if (pointsObj)
            {
                pointsText = pointsObj.GetComponentInChildren<TMP_Text>();
                if (pointsText)
                {
                    pointsText.SetText(points.ToString());
                    pointsText.color = color;
                }
            }
            if (pointsText == null)
                return;

            var endPos = pos + (Vector3.up * 2);

            LeanTween.scale(pointsObj, new Vector3(1.5f, 1.5f, 1.5f), .5f).setEasePunch();
            LeanTween.scale(pointsObj, new Vector3(1, 1, 1), .2f).setDelay(.5f).setEase(LeanTweenType.easeInOutCubic);
            LeanTween.move(pointsObj, endPos, 1f).setEaseOutCubic();

            LeanTween.value(pointsObj, 1, 0, .5f).setDelay(.5f)
                .setOnUpdate((value) =>
                {
                    color.a = value;
                    pointsText.color = color;
                })
                .setEaseOutCubic()
                .setOnComplete(() =>
                {
                    _displayPointsPool.ReturnToPool(pointsObj);
                });

            var clip = (points > 0) ? UISounds.Clip.scorePlus : UISounds.Clip.scoreMinus;
            //PlayAudio(clip);
            GameManager.StartCoroutine(PlayDelayedAudio(clip, .2f));
        }

        void TweenColor(Color begin, Color end, float time, float delay = default)
        {
            LeanTween.value(UiScore.gameObject, 0.1f, 1f, time).setDelay(delay)
                .setOnUpdate((value) =>
                {
                    UiScore.color = Color.Lerp(begin, end, value);
                });
        }

        void SetScore(int points)
        {
            if (UiScore)
                UiScore.text = string.Format(scoreFormat, points);
        }

        void BuildPools()
        {
            _displayPointsPool = GameObjectPool.Build(prefabDisplayPoints, 1);
        }

    }
}
