using UnityEngine;
using System.Collections;
using TMPro;
using MoonsOfMars.Shared;

namespace MoonsOfMars.Game.Asteroids
{
    using static AsteroidsGameManager;

    [CreateAssetMenu(fileName = "UI Manager data", menuName = "Asteroids/UI Manager")]
    public class UIManagerData : ScriptableObject
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
        MainMenu MainMenu => GmManager != null ? GmManager.m_MainMenu : null;
        TextMeshProUGUI UiScore => GmManager != null ? GmManager.m_ScoreTextUI : null;
        ParticleSystem SpaceDebris => GmManager != null ? GmManager.m_SpaceDebriSystem : null;

        public bool AudioPlaying => uiSounds.IsPlaying();

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

        void OnDisable() => Score.OnEarn -= ScoreEarned;

        /// <summary>
        /// Set window in start position & hide ui items
        /// </summary>
        internal void InitUI(AudioSource source)
        {
            uiSounds.m_UiAudio = source;
            UiScore.color = scoreColor;
            DisplayGameScore(false);
            MainMenu.HideMenu();
        }

        public IEnumerator ShowMainMenu()
        {
            if (_firstRun)
                _firstRun = false;
            else
                yield return Wait(2);

            DisplayGameScore(false);
            SpaceDebris.Play();
            MainMenu.OpenMenu();
        }

        public void HideMainMenu(bool startGame = true)
        {
            DisplayGameScore(startGame);
            SpaceDebris.Stop();
            MainMenu.CloseMenu();
            //return TweenUtil.MenuWindowClose(MainMenu);
        }

        public void ShowPauseMenu()
        {
            Time.timeScale = 0;

            GmManager.SetGameStatus(GameStatus.paused);
            GmManager.m_LightsManager.BlurBackground(true);
            GmManager.m_MainMenu.OpenPauseMenu();
        }

        public void HidePauseMenu()
        {
            var id = MainMenu.ClosePauseMenu();
            var d = LeanTween.descr(id);

            d.setOnComplete(() =>
               {
                   MainMenu.SetPauseMenuActive(false);
                   GmManager.m_LightsManager.BlurBackground(false);
                   Time.timeScale = 1;
               });
        }

        public void PlayAudio(UISounds.Clip clip) => uiSounds.PlayClip(clip);

        IEnumerator PlayDelayedAudio(UISounds.Clip clip, float delay)
        {
            yield return new WaitForSeconds(delay);
            PlayAudio(clip);
        }

        void ScoreEarned(int points, Vector3 pos)
        {
            if (points == 0)
                return;

            Debug.Log("Score: " + points + ", pos: " + pos);
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
            GmManager.m_ScoreTextUI.gameObject.SetActive(show);
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
                .setOnComplete(() => _displayPointsPool.ReturnToPool(pointsObj));

            var clip = (points > 0) ? UISounds.Clip.scorePlus : UISounds.Clip.scoreMinus;
            GmManager.StartCoroutine(PlayDelayedAudio(clip, .2f));
        }

        void TweenColor(Color begin, Color end, float time, float delay = default)
        {
            LeanTween.value(UiScore.gameObject, 0.1f, 1f, time).setDelay(delay)
                .setOnUpdate((value) => UiScore.color = Color.Lerp(begin, end, value));
        }

        void SetScore(int points)
        {
            if (UiScore)
                UiScore.text = string.Format(scoreFormat, points);
        }

        void BuildPools() => _displayPointsPool = GameObjectPool.Build(prefabDisplayPoints, 1);

    }
}
