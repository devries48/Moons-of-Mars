using System.Collections.Generic;
using UnityEngine;

namespace Game.SpaceShooter
{
    public class CockpitController : MonoBehaviour
    {

        [SerializeField] Transform _joystick;
        [SerializeField] List<Transform> _throttles;
        [SerializeField] float _joystickRange = 30f;
        [SerializeField] float _throttleRange = 35f;

        float _currentThrust;
        float _thrustInPercentage, _prevThrust;

        public void UpdateControls(IMovementControls ctrls)
        {
            _currentThrust = Mathf.Lerp(_currentThrust, ctrls.Thrust, .1f);

            _joystick.localRotation = Quaternion.Euler(
                Mathf.Clamp(ctrls.Pitch * _joystickRange, -_joystickRange, _joystickRange),
                Mathf.Clamp(ctrls.Yaw * _joystickRange, -_joystickRange, _joystickRange),
                Mathf.Clamp(ctrls.Roll * _joystickRange, -_joystickRange, _joystickRange)
            );

            var rot = _throttles[0].localRotation.eulerAngles;
            rot.x = _currentThrust * _throttleRange;

            foreach (var throttle in _throttles)
            {
                throttle.localRotation = Quaternion.Euler(rot);
            }

            _thrustInPercentage = _currentThrust / 1f;

            if (_thrustInPercentage == _prevThrust)
                return;

            _prevThrust = _thrustInPercentage;
        }
    }
}