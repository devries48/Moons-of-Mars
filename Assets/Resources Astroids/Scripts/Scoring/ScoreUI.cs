using UnityEngine;
using TMPro;

namespace Game.Astroids
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class ScoreUI : MonoBehaviour
    {
        [SerializeField]
        Color textColor = Color.white;

        [SerializeField]
        Color highlightColor = Color.yellow;

        [SerializeField]
        Color negativeColor = Color.red;

        [SerializeField]
        string scoreFormat = "{0:000000}";

        protected UIManager UIManager
        {
            get
            {
                if (__uiManager == null)
                    __uiManager = AstroidsGameManager.Instance.m_uiManager;

                return __uiManager;
            }
        }
        UIManager __uiManager;


        TextMeshProUGUI _scoreText;

        void Awake()
        {
            _scoreText = GetComponent<TextMeshProUGUI>();

            SetColor(textColor);
        }

        void OnEnable()
        {
            SetScore(Score.Earned);
            Score.OnEarn += ScoreEarned;
        }

        void OnDisable()
        {
            Score.OnEarn -= ScoreEarned;
        }
        void ScoreEarned(int points)
        {
            if (points == 0)
                return;

            SetScore(Score.Earned);

            LeanTween.scale(gameObject, new Vector3(1.5f, 1.5f, 1.5f), .5f).setEasePunch();
            LeanTween.scale(gameObject, new Vector3(1f, 1f, 1f), .2f).setDelay(.5f).setEase(LeanTweenType.easeInOutCubic);

            TweenColor(textColor, points > 0 ? highlightColor : negativeColor, .5f);
            TweenColor(textColor, textColor, .1f, .5f);

            if (points > 0)
                UIManager.PlayDelayedAudio(UISounds.Clip.ScorePlus, .2f);
            else
                UIManager.PlayDelayedAudio(UISounds.Clip.ScoreMinus,.2f);

        }

        void SetScore(int points)
        {
            _scoreText.text = string.Format(scoreFormat, points);
        }

        void SetColor(Color color)
        {
            _scoreText.color = color;
        }

        void TweenColor(Color begin, Color end, float time, float delay = default)
        {
            LeanTween.value(gameObject, 0.1f, 1f, time).setDelay(delay)
                .setOnUpdate((value) =>
                {
                    _scoreText.color = Color.Lerp(begin, end, value);
                });
        }
    }
}