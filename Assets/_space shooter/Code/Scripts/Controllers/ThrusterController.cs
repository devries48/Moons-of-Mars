using UnityEngine;

namespace Game.SpaceShooter
{
    /// <summary>
    /// Listen to change in thrust, adjust particle values
    /// </summary>
    [ExecuteAlways]
    public class ThrusterController : MonoBehaviour
    {
        [SerializeField] ParticleSystem sys;
        [SerializeField] EngineController engineController;

        [Header("=== Particle System settings ===")]
        [SerializeField] float minStartSpeed;
        [SerializeField] float maxStartSpeed;

        [Header("=== Hide thruster when inactive ===")]
        [SerializeField] GameObject _thruster;

        ParticleSystem.MainModule _sysMain;
        ParticleSystem.EmissionModule _sysEmission;

        internal float m_ThrusterHealthPercentage = 100f;

        void OnEnable()
        {
            if (sys)
            {
                _sysMain = sys.main;
                _sysEmission = sys.emission;
            }

            if (engineController == null)
                return;

            engineController.ThrustChangedEvent += SetStartSpeed;
        }

        void OnDisable()
        {
            if (engineController == null)
                return;

            engineController.ThrustChangedEvent -= SetStartSpeed;
        }

        void SetStartSpeed(float thrustInPercent)
        {
            if (_thruster != null)
            {
                if (Mathf.Approximately(0, thrustInPercent))
                {
                    if (_thruster.activeInHierarchy)
                        _thruster.SetActive(false);
                }
                else
                {
                    if (!_thruster.activeInHierarchy)
                        _thruster.SetActive(true);
                }
            }
            var speed = thrustInPercent * maxStartSpeed + (1f - thrustInPercent) * minStartSpeed;
            speed = speed * m_ThrusterHealthPercentage / 100;
            _sysMain.startSpeed = speed;
        }
    }
}