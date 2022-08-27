using System;
using UnityEngine;

namespace Game.Astroids
{
    // Send out thrust percentage of max thrust as event 
    [ExecuteAlways]
    public class ThrustController : MonoBehaviour
    {
        [SerializeField]
        float minThrust;

        [SerializeField]
        float maxThrust = 1f;

        [SerializeField]
        float changePerSecondByInput;

        [SerializeField]
        [Range(0f, 1f)]
        float _currentThrust;

        float _thrustInPercentageOfMax;

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
            _currentThrust = Mathf.Clamp(_currentThrust + changeBy, minThrust, maxThrust);
            RaiseThrustChangedEvent();
        }
        void RaiseThrustChangedEvent()
        {
            _thrustInPercentageOfMax = (_currentThrust - minThrust) / (maxThrust - minThrust);

            EventThrustChanged(_thrustInPercentageOfMax);
        }

        [ContextMenu("SetToMinThrust")]
        void SetToMinThrust() => EventThrustChanged(minThrust);

        [ContextMenu("SetToMaxThrust")]
        void SetToMaxThrust() => EventThrustChanged(maxThrust);

    }
}