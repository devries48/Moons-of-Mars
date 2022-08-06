using UnityEngine;
using TMPro;

namespace Game.Astroids
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class ScoreController : MonoBehaviour
    {
        [SerializeField]
        Color textColor = Color.white;

        [SerializeField]
        string scoreFormat = "{0:000000}";

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
            SetScore(Score.Earned);

            LeanTween.scale(gameObject, new Vector3(1.5f, 1.5f, 1.5f),.5f).setEase(LeanTweenType.easeOutElastic);
            LeanTween.scale(gameObject, new Vector3(1f, 1f, 1f), .2f).setDelay(.5f).setEase(LeanTweenType.easeInOutCubic);
        }

        void SetScore(int points)
        {
            _scoreText.text = string.Format(scoreFormat, points);
        }

        void SetColor(Color color)
        {
            _scoreText.color = color;
        }

    }
}