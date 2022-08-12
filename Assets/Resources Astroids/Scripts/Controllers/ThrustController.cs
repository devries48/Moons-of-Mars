using System;
using System.Collections;
using System.Collections.Generic;
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

        float _currentThrust;
        float _thrustInPercentageOfMax;

        public event Action<float> EventThrustChanged = delegate { };


        // Start is called before the first frame update
        void Start()
        {
            SetToMinThrust();
        }

        void Update()
        {
            if (Input.GetKey(KeyCode.Space))
                ChangeThrust(changePerSecondByInput * Time.deltaTime);
            else if (Input.GetKey(KeyCode.B))
                ChangeThrust(-changePerSecondByInput * Time.deltaTime);
        }

        void ChangeThrust(float changeBy)
        {
            _currentThrust = Mathf.Clamp(_currentThrust + changeBy, minThrust, maxThrust);
            _thrustInPercentageOfMax = (_currentThrust - minThrust) / (maxThrust - minThrust);

            EventThrustChanged(_thrustInPercentageOfMax);
        }

        [ContextMenu("SetToMinThrust")]
        void SetToMinThrust()
        {
            EventThrustChanged(minThrust);
        }

        [ContextMenu("SetToMaxThrust")]
        void SetToMaxThrust()
        {
            EventThrustChanged(maxThrust);
        }


    }
}