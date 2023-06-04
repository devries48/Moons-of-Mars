using MoonsOfMars.Shared;
using System.Collections;
using TMPro;
using UnityEngine;

namespace MoonsOfMars.Game.Asteroids
{
    [ExecuteInEditMode]
    public class GameResultsController : MonoBehaviour
    {
        public enum GameResult { stageCleared, gameOver, gameComplete }

        #region editor fields
        [SerializeField] RectTransform canvas;
        [SerializeField] GameObject gameContinue;

        [Header("Stage Result Canvas")]

        [SerializeField] Utils.ObjectLayer stageLayer = Utils.ObjectLayer.Background;
        [SerializeField] Vector3 stagePosition;
        [SerializeField] Quaternion stageRotation;

        [Header("Game Result Canvas")]
        [SerializeField] Utils.ObjectLayer gameLayer = Utils.ObjectLayer.Default;
        [SerializeField] Vector3 gamePosition;
        [SerializeField] Quaternion gameRotation;

        [Header("UI Statistics Elements")]
        [SerializeField] TextMeshProUGUI completionHeader;
        [SerializeField] TextMeshProUGUI title;
        [SerializeField] TextMeshProUGUI playtime;
        [SerializeField] TextMeshProUGUI shotsFired;
        [SerializeField] TextMeshProUGUI shotsHit;
        [SerializeField] TextMeshProUGUI hitPercentage;
        [SerializeField] TextMeshProUGUI astroidsDestroyed;
        [SerializeField] TextMeshProUGUI ufosSpawned;
        [SerializeField] TextMeshProUGUI ufosDestroyed;
        [SerializeField] TextMeshProUGUI powerupsSpawned;
        [SerializeField] TextMeshProUGUI powerupsPickedUp;

        [Header("UI Bonus Elements")]
        [SerializeField] GameObject stageBonusHeader;
        [SerializeField] TextMeshProUGUI stage;
        [SerializeField] TextMeshProUGUI timeBonus;
        [SerializeField] TextMeshProUGUI efficiencyBonus;
        [SerializeField] TextMeshProUGUI destructionBonus;
        [SerializeField] TextMeshProUGUI pickupBonus;
        [SerializeField] TextMeshProUGUI totalBonus;

        [Header("UI Game Over Elements")]
        [SerializeField] GameObject gameOverHeader;
        [SerializeField] TextMeshProUGUI level;
        [SerializeField] TextMeshProUGUI points;

        #endregion

        #region properties
        GameManager GameManager
        {
            get
            {
                if (__gameManager == null)
                    __gameManager = GameManager.GmManager;

                return __gameManager;
            }
        }
        GameManager __gameManager;

        LevelManager LevelManager
        {
            get
            {
                if (__levelManager == null)
                    __levelManager = GameManager.m_LevelManager;

                return __levelManager;
            }
        }
        LevelManager __levelManager;

        #endregion

        public void DisplayResults(GameResult result) => StartCoroutine(DisplayResultsCore(result));

        IEnumerator DisplayResultsCore(GameResult result)
        {
            GameStatistics stats;
            if (result == GameResult.stageCleared)
            {
                var stageStats = GetStageStats();
                stats = stageStats;
                completionHeader.text = "completion";
                title.text = $"{stageStats.Name} stage completed".ToLower();
                stage.text = $"x  {stageStats.StageNr}";
                totalBonus.text = FmtInt(stageStats.TotalBonus);
                timeBonus.text = FmtInt(stageStats.TimeBonus);
                efficiencyBonus.text = FmtInt(stageStats.EfficiencyBonus);
                destructionBonus.text = FmtInt(stageStats.DestrucionBonus);
                pickupBonus.text = FmtInt(stageStats.PickupBonus);
            }
            else
            {
                stats = GetGameStats();
                completionHeader.text = "gameplay";
                title.text = "game over";
            }

            stageBonusHeader.SetActive(result == GameResult.stageCleared);
            gameOverHeader.SetActive(result != GameResult.stageCleared);

            shotsFired.text = FmtFloat(stats.ShotsFired);
            shotsHit.text = FmtFloat(stats.ShotsHit);
            hitPercentage.text = FmtPct(stats.ShotsHit, stats.ShotsFired);
            ufosSpawned.text = FmtInt(stats.UfosSpawned);
            ufosDestroyed.text = FmtInt(stats.UfosDestroyed);
            powerupsSpawned.text = FmtInt(stats.PowerupsSpawned);
            powerupsPickedUp.text = FmtInt(stats.PowerupsPickedUp);
            astroidsDestroyed.text = FmtInt(stats.AsteroidsDestroyed);
            playtime.text = FmtTime(stats.Playtime);

            if (result == GameResult.stageCleared)
            {
                Utils.SetGameObjectLayer(gameObject, stageLayer);
                canvas.position = stagePosition;
                canvas.rotation = stageRotation;
                canvas.pivot = new Vector2(.5f, .5f);
                canvas.gameObject.SetActive(true);
            }
            else
            {
                Utils.SetGameObjectLayer(gameObject, stageLayer);
                canvas.position = gamePosition;
                canvas.rotation = gameRotation;
                TweenUtil.TweenPivot(canvas, new Vector2(.5f, .5f), new Vector2(.5f, -1f));
                ShowContinueButton();
            }

            yield return null;
        }

        public void HideResults(GameResult result)
        {
            if (result == GameResult.stageCleared)
                Utils.MoveToCamAndHide(canvas.gameObject, 8);
            else
                TweenUtil.TweenPivot(canvas, new Vector2(.5f, -1f), default, LeanTweenType.easeOutBack, .5f);

            HideContinueButton();
        }

        public void ShowContinueButton()
        {
            gameContinue.SetActive(true);
            GameManager.GmManager.InputManager.WaitForAnyKey();
        }

        void HideContinueButton() => gameContinue.SetActive(false);

        string FmtInt(int value) => value == 0 ? "0" : value.ToString();
        string FmtFloat(float value) => value == 0 ? "0" : ((int)value).ToString();
        string FmtPct(float value, float total) => total > 0 ? (value / total * 100).ToString("0.0") : "0.0";

        string FmtTime(float t)
        {
            int d = (int)(t * 100.0f);
            int minutes = d / (60 * 100);
            int seconds = d % (60 * 100) / 100;
            int tenths = d % 10;

            return $"{minutes:0} min {seconds:0}.{tenths:0} sec";
        }

        StageStatistics GetStageStats()
        {
            Debug.Log("Get Stage Stats");
            if (Application.isPlaying)
                return LevelManager.GetStageStatistics();
            else
                return new StageStatistics(1, new Stage() { Name = "test stage" }, new GameStatistics());
        }

        GameStatistics GetGameStats()
        {
            Debug.Log("Get Game Stats");

            if (Application.isPlaying)
                return LevelManager.GetGameStatistics();
            else
                return new GameStatistics();
        }

        [ContextMenu("SetStageCleared")]
        void SetStageCleared() => DisplayResults(GameResult.stageCleared);

        [ContextMenu("SetGameOver")]
        void SetGameOver() => DisplayResults(GameResult.gameOver);

        [ContextMenu("SetGameComplete")]
        void SetGameComplete() => DisplayResults(GameResult.gameComplete);

    }
}