using System.Collections;
using TMPro;
using UnityEngine;

namespace Game.Astroids
{
    public class StageResultController : MonoBehaviour
    {
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
        [SerializeField] TextMeshProUGUI powerupsDestroyed;
        [SerializeField] TextMeshProUGUI powerupsPickedUp;

        [Header("UI Bonus Elements")]
        [SerializeField] TextMeshProUGUI stage;
        [SerializeField] TextMeshProUGUI timeBonus;
        [SerializeField] TextMeshProUGUI efficiencyBonus;
        [SerializeField] TextMeshProUGUI destructionBonus;
        [SerializeField] TextMeshProUGUI pickupBonus;
        [SerializeField] TextMeshProUGUI totalBonus;

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

        void OnEnable()
        {
            StartCoroutine(DisplayResults());
        }

        IEnumerator DisplayResults()
        {
            var r = LevelManager.GetStageResults();
            print("Ufo spawn:" + r.UfosSpawned);
            print("Pwr spawn:" + r.PowerupsSpawned);

            title.text = $"{r.Name} stage completed";
            shotsFired.text = FmtFloat(r.ShotsFired);
            shotsHit.text = FmtFloat(r.ShotsHit);
            hitPercentage.text = FmtPct(r.ShotsHit, r.ShotsFired);
            ufosSpawned.text = FmtInt(r.UfosSpawned);
            ufosDestroyed.text = FmtInt(r.UfosDestroyed);
            powerupsSpawned.text = FmtInt(r.PowerupsSpawned);
            powerupsDestroyed.text = FmtInt(r.PowerupsDestroyed);
            powerupsPickedUp.text = FmtInt(r.PowerupsPickedUp);
            astroidsDestroyed.text = FmtInt(r.AstroidsDestroyed);
            playtime.text = FmtTime(r.Playtime);
            
            yield return null;

            stage.text = $"x  {r.StageNr}";

            r.CalculateBonus();
            timeBonus.text = FmtInt(r.TimeBonus);
            efficiencyBonus.text = FmtInt(r.EfficiencyBonus);
            destructionBonus.text = FmtInt(r.DestrucionBonus);
            pickupBonus.text = FmtInt(r.PickupBonus);
            totalBonus.text = FmtInt(r.TotalBonus);

            yield return null;

        }

        string FmtInt(int value)
        {
            return value == 0 ? "0" : value.ToString();
        }

        string FmtFloat(float value)
        {
            return value == 0 ? "0" : ((int)value).ToString();
        }

        string FmtPct(float value, float total)
        {
            return total > 0 ? (value / total * 100).ToString("0.0") : "0.0";
        }

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