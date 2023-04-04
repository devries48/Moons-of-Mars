using UnityEngine;
using UnityEngine.UI;

namespace Game.SpaceShooter
{
    public class DashboardController : MonoBehaviour
    {
        [SerializeField] TelemetryManager _telemetry;
        [SerializeField] Image laserHeatImage;
        [SerializeField] Image boostingImage;

        void Update()
        {
            laserHeatImage.fillAmount = _telemetry.LaserHeat;
            boostingImage.fillAmount = _telemetry.Boosting;
        }
    }
}