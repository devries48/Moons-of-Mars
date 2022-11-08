using System;
using UnityEngine;

namespace Game.Astroids
{
    // Send out thrust percentage of max thrust as event 
    [ExecuteAlways]
    public class ThrustController : MonoBehaviour
    {
        [SerializeField] float changePerSecondByInput;
        [SerializeField] AudioSource engineAudio;

        [Range(0f, 1f)] float _currentThrust;
        float _thrustInPercentage;
        float _prevThrust;

        readonly float _maxThrust = 1f;

        public event Action<float> ThrustChangedEvent = delegate { };

        void OnEnable() => SetToMinThrust();

        void OnDisable() => SetToMinThrust();

        public void SetThrust(float thrust)
        {
            _currentThrust = thrust;
            RaiseThrustChangedEvent();
        }

        public void IncreaseThrust() => ChangeThrust(changePerSecondByInput * Time.deltaTime);

        public void DecreaseThrust() => ChangeThrust(-changePerSecondByInput * Time.deltaTime);

        void ChangeThrust(float changeBy)
        {
            _currentThrust = Mathf.Clamp(_currentThrust + changeBy, 0, _maxThrust);
            RaiseThrustChangedEvent();
        }
        void RaiseThrustChangedEvent()
        {
            _thrustInPercentage = _currentThrust / _maxThrust;
            
            if (_thrustInPercentage == _prevThrust)
                return;

            _prevThrust = _thrustInPercentage;

            SetVolume(_thrustInPercentage);
            ThrustChangedEvent(_thrustInPercentage);
        }

        void SetVolume(float vol)
        {
            if (engineAudio)
                engineAudio.volume = vol;
        }

        [ContextMenu("SetToMinThrust")]
        void SetToMinThrust()
        {
            SetVolume(0);
            ThrustChangedEvent(0);
        }

        [ContextMenu("SetToMaxThrust")]
        void SetToMaxThrust() => ThrustChangedEvent(_maxThrust);

    }
}