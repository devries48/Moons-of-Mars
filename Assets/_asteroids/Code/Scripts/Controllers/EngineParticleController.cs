using UnityEngine;

namespace MoonsOfMars.Game.Asteroids
{
    /// <summary>
    /// Listen to change in thrust, adjust particle values
    /// </summary>
    [ExecuteAlways]
    public class EngineParticleController : MonoBehaviour
    {
        [SerializeField] ParticleSystem sys;
        [SerializeField] ThrustController thrustController;
        [SerializeField] float minStartSpeed, maxStartSpeed;
        [SerializeField] float minEmission, maxEmission;
        [SerializeField] bool useBurstEmission;
        [SerializeField] Vector3 minPosition, maxPosition;

        ParticleSystem.MainModule _sysMain;
        ParticleSystem.EmissionModule _sysEmission;

        void OnEnable()
        {
            if (sys)
            {
                _sysMain = sys.main;
                _sysEmission = sys.emission;
            }

            if (thrustController == null)
                return;

            thrustController.ThrustChangedEvent += SetStartSpeed;

            if (minPosition != maxPosition)
                thrustController.ThrustChangedEvent += SetPosition;

            if (minEmission != maxEmission)
                thrustController.ThrustChangedEvent += SetEmission;
        }

        void OnDisable()
        {
            if (thrustController == null)
                return;

            thrustController.ThrustChangedEvent -= SetStartSpeed;

            if (minPosition != maxPosition)
                thrustController.ThrustChangedEvent -= SetPosition;

            if (minEmission != maxEmission)
                thrustController.ThrustChangedEvent -= SetEmission;
        }

        void SetStartSpeed(float thrustInPercent)
        {
            var speed = thrustInPercent * maxStartSpeed + (1f - thrustInPercent) * minStartSpeed;
            _sysMain.startSpeed = speed;
        }

        void SetPosition(float thrustInPercent)
        {
            var pos = thrustInPercent * maxPosition + (1f - thrustInPercent) * minPosition;
            sys.transform.localPosition = pos;
        }

        void SetEmission(float thrustInPercent)
        {
            var rate = thrustInPercent * maxEmission + (1f - thrustInPercent) * minEmission;
            if (useBurstEmission)
            {
                for (int i = 0; i < _sysEmission.burstCount; i++)
                {
                    var burst = _sysEmission.GetBurst(i);
                    burst.maxCount = (short)rate;
                    _sysEmission.SetBurst(i, burst);
                }
            }
            else
                _sysEmission.rateOverTime = rate;
        }

        [ContextMenu("SetMinEmission")]
        void SetMinEmission() => SetEmission(0);

        [ContextMenu("SetMaxEmission")]
        void SetMaxEmission() => SetEmission(1);
    }
}