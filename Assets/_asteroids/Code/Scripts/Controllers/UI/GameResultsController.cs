using System.Collections;
using TMPro;
using UnityEngine;

namespace MoonsOfMars.Game.Asteroids
{
    public class GameResultsController : MonoBehaviour
    {
        public enum GameResult { stage, gameOver, gameComplete}

        #region editor fields
        [Header("Stage Result Canvas")]
        [SerializeField] Vector3 stagePosition;
        [SerializeField] Quaternion stageRotation;

        [Header("Game Result Canvas")]
        [SerializeField] Vector3 gamePosition;
        [SerializeField] Quaternion gameRotation;

        [Header("UI Statistics Elements")]
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
        [SerializeField] TextMeshProUGUI stage;
        [SerializeField] TextMeshProUGUI timeBonus;
        [SerializeField] TextMeshProUGUI efficiencyBonus;
        [SerializeField] TextMeshProUGUI destructionBonus;
        [SerializeField] TextMeshProUGUI pickupBonus;
        [SerializeField] TextMeshProUGUI totalBonus;
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
            var r = LevelManager.GetStageResults();
            print("Ufo spawn:" + r.UfosSpawned);
            print("Pwr spawn:" + r.PowerupsSpawned);

            title.text = $"{r.Name} stage completed".ToLower();
            shotsFired.text = FmtFloat(r.ShotsFired);
            shotsHit.text = FmtFloat(r.ShotsHit);
            hitPercentage.text = FmtPct(r.ShotsHit, r.ShotsFired);
            ufosSpawned.text = FmtInt(r.UfosSpawned);
            ufosDestroyed.text = FmtInt(r.UfosDestroyed);
            powerupsSpawned.text = FmtInt(r.PowerupsSpawned);
            powerupsPickedUp.text = FmtInt(r.PowerupsPickedUp);
            astroidsDestroyed.text = FmtInt(r.AsteroidsDestroyed);
            playtime.text = FmtTime(r.Playtime);

            stage.text = $"x  {r.StageNr}";

            totalBonus.text = FmtInt(r.TotalBonus);
            timeBonus.text = FmtInt(r.TimeBonus);
            efficiencyBonus.text = FmtInt(r.EfficiencyBonus);
            destructionBonus.text = FmtInt(r.DestrucionBonus);
            pickupBonus.text = FmtInt(r.PickupBonus);

            gameObject.SetActive(true);

            yield return null;
        }

        string FmtInt(int value) => value == 0 ? "0" : value.ToString();
        string FmtFloat(float value) => value == 0 ? "0" : ((int)value).ToString();
        string FmtPct(float value, float total) => total > 0 ? (value / total * 100).ToString("0.0") : "0.0";

        string FmtTime(float t)
        {
            int d = (int)(t * 100.0f);
            int minutes = d / (60 * 100);
            int seconds = (d % (60 * 100)) / 100;
            int tenths = d % 10;

            return $"{minutes:0} min {seconds:0}.{tenths:0} sec";
        }

    }
}