using System;
using UnityEngine;

namespace Game.SpaceShooter
{
    // Send out thrust percentage of max thrust as event 
    [ExecuteAlways]
    public class EngineController : MonoBehaviour
    {
        [SerializeField] float _changePerSecondByInput;
        [SerializeField] AudioSource _engineAudio;

        ThrusterController[] _thrusters;

        internal float m_CurrentThrust;
        float _thrustInPercentage;
        float _prevThrust;

        public event Action<float> ThrustChangedEvent = delegate { };

        void Start()
        {
            _thrusters = GetComponentsInChildren<ThrusterController>(true);
        }

        void OnEnable() => SetToMinThrust();

        void OnDisable() => SetToMinThrust();

        public void SetThrust(float thrust)
        {
            m_CurrentThrust = thrust;
            RaiseThrustChangedEvent();
        }

        /// <summary>
        /// Returns the thrust force relative to the health of the thrusters
        /// </summary>
        public float GetThrustForce(float thrust)
        {
            float totalHealth = 0;
            foreach (var thruster in _thrusters)
                totalHealth += thruster.m_ThrusterHealthPercentage;

            return thrust * totalHealth / (_thrusters.Length * 100f);
        }

        public void IncreaseThrust() => ChangeThrust(_changePerSecondByInput * Time.deltaTime);

        public void DecreaseThrust() => ChangeThrust(-_changePerSecondByInput * Time.deltaTime);

        void ChangeThrust(float changeBy)
        {
            if (m_CurrentThrust + changeBy < 0)
                m_CurrentThrust =  0;
            else
                m_CurrentThrust = Mathf.Clamp(m_CurrentThrust + changeBy, 0, 1f);

            RaiseThrustChangedEvent();
        }
        void RaiseThrustChangedEvent()
        {
            _thrustInPercentage = m_CurrentThrust / 1f;

            if (_thrustInPercentage == _prevThrust)
                return;

            _prevThrust = _thrustInPercentage;

            SetVolume(_thrustInPercentage);
            ThrustChangedEvent(_thrustInPercentage);
        }

        void SetVolume(float vol)
        {
            if (_engineAudio)
                _engineAudio.volume = vol;
        }

        [ContextMenu("SetToMinThrust")]
        void SetToMinThrust()
        {
            SetVolume(0);
            ThrustChangedEvent(0);
        }

        [ContextMenu("SetToMaxThrust")]
        void SetToMaxThrust() => ThrustChangedEvent(1f);

    }
}