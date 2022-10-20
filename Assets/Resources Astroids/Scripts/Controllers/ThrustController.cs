using System;
using UnityEditor.Media;
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
        float _thrustInPercentageOfMax;
        readonly float _minThrust = 0f;
        readonly float _maxThrust = 1f;

        public event Action<float> EventThrustChanged = delegate { };

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
            _currentThrust = Mathf.Clamp(_currentThrust + changeBy, _minThrust, _maxThrust);
            RaiseThrustChangedEvent();
        }
        void RaiseThrustChangedEvent()
        {
            _thrustInPercentageOfMax = (_currentThrust - _minThrust) / (_maxThrust - _minThrust);
            SetVolume(_thrustInPercentageOfMax);
            EventThrustChanged(_thrustInPercentageOfMax);
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
            EventThrustChanged(_minThrust);
        }

        [ContextMenu("SetToMaxThrust")]
        void SetToMaxThrust() => EventThrustChanged(_maxThrust);

    }
}