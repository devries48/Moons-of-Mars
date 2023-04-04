using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.SpaceShooter
{
    public class ShipController : MonoBehaviour, IMovementControls
    {
        [SerializeField] CockpitController _cockpitController;
        [SerializeField] EngineController _engineController;
        [SerializeField] WeaponsController _weaponController;
        [SerializeField] TelemetryManager _telemetry;

        [Header("=== Ship Movement Settings ===")]
        [SerializeField] float _forwardThrustForce = 200f;
        [SerializeField]
        float _upThrustForce = 50f,
            _strafeThrustForce = 50f,
            _pitchTorque = 200f,
            _rollTorque = 200f,
            _yawTorque = 300;

        [Header("=== Bost Settings ===")]
        [SerializeField] float _maxBoostAmount = 2f;
        [SerializeField] float _boostDeprecationRate = .25f;
        [SerializeField] float _boostChargeRate = .5f;
        [SerializeField] float _boostMultiplier = 5f;

        [Header("=== Glide Reduction Settings ===")]
        [SerializeField, Range(.001f, .999f)] float _thrustGlideReduction = .111f;
        [SerializeField, Range(.001f, .999f)]
        float _upDownGlideReduction = .111f,
            _leftRightGlideReduction = .111f;

        float _roll, _strafe, _upDown;
        Vector2 _pitchYaw;
        float _pitch, _yaw;
        bool _isBoosting;
        float _forwardGlide, _verticalGlide, _horizontalGlide;
        float _currentBoostAmount;
        bool _thrustIncPressed, _thrustDecPressed;

        internal Rigidbody m_RbShip;

        public float Thrust => _engineController != null ? _engineController.m_CurrentThrust : 0;
        public float Yaw => _yaw;
        public float Pitch => _pitch;
        public float Roll => _roll;

        void Start()
        {
            m_RbShip = GetComponent<Rigidbody>();
            _currentBoostAmount = _maxBoostAmount;
        }

        void FixedUpdate()
        {
            ShipBoosting(Time.fixedDeltaTime);
            ShipMovement();
            _weaponController.UpdateWeapons(Time.fixedDeltaTime);
        }

        void ShipBoosting(float dt)
        {
            if (_isBoosting && _currentBoostAmount > 0f)
            {
                _currentBoostAmount -= _boostDeprecationRate * dt;
                if (_currentBoostAmount <= 0f)
                    _isBoosting = false;
            }
            else
            {
                if (_currentBoostAmount < _maxBoostAmount)
                    _currentBoostAmount += _boostChargeRate * dt;
            }
            _telemetry.Boosting = _currentBoostAmount / _maxBoostAmount;
        }

        void ShipMovement()
        {
            _cockpitController.UpdateControls(this);

            // roll, pitch & yaw
            if (IsNotZero(_roll))
                m_RbShip.AddRelativeTorque(_roll * _rollTorque * Time.fixedDeltaTime * Vector3.forward);
            if (IsNotZero(_pitchTorque))
                m_RbShip.AddRelativeTorque(_pitchTorque * Mathf.Clamp(-_pitchYaw.y, -1f, 1f) * Time.fixedDeltaTime * Vector3.right);
            if (IsNotZero(_yawTorque))
                m_RbShip.AddRelativeTorque(_yawTorque * Mathf.Clamp(_pitchYaw.x, -1f, 1f) * Time.fixedDeltaTime * Vector3.up);

            // thrust
            _forwardGlide = IsNotZero(Thrust)
                ? (_isBoosting ? Thrust * _boostMultiplier : Thrust) * _forwardThrustForce
                : 0;

            _verticalGlide = IsNotZero(_upDown)
                ? _upDown * _upThrustForce
                : _verticalGlide * _upDownGlideReduction;

            _horizontalGlide = IsNotZero(_strafe)
                ? _strafe * _strafeThrustForce
                : _horizontalGlide * _leftRightGlideReduction;

            if (IsNotZero(_forwardGlide))
            {
                var force = _engineController.GetThrustForce(_forwardGlide);
                m_RbShip.AddRelativeForce(force * Time.fixedDeltaTime * Vector3.forward);
            }

            if (IsNotZero(_verticalGlide))
                m_RbShip.AddRelativeForce(_verticalGlide * Time.fixedDeltaTime * Vector3.up);

            if (IsNotZero(_horizontalGlide))
                m_RbShip.AddRelativeForce(_horizontalGlide * Time.fixedDeltaTime * Vector3.right);
        }

        IEnumerator IncreaseThrust()
        {
            while (_thrustIncPressed)
            {
                _engineController.IncreaseThrust();
                yield return null;
            }
        }

        IEnumerator DecreaseThrust()
        {
            while (_thrustDecPressed)
            {
                _engineController.DecreaseThrust();
                yield return null;
            }
        }


        // The "> .1f || < -.1f" is a necessary check.
        // Controllers can return small values when idle.
        bool IsNotZero(float value) => value > .1f || value < -.1f;

        #region Input Methods
        public void OnThrust(InputAction.CallbackContext context)
        {
            var thrust = context.ReadValue<float>();
            if (thrust > 0)
            {
                _thrustDecPressed = false;
                _thrustIncPressed = true;
                StartCoroutine(IncreaseThrust());
            }
            else if (thrust < 0)
            {
                _thrustIncPressed = false;
                _thrustDecPressed = true;
                StartCoroutine(DecreaseThrust());
            }
            else
            {
                _thrustIncPressed = false;
                _thrustIncPressed = false;
            }
        }

        public void OnStrafe(InputAction.CallbackContext context)
        {
            _strafe = context.ReadValue<float>();
        }

        public void OnUpDown(InputAction.CallbackContext context)
        {
            _upDown = context.ReadValue<float>();
        }

        public void OnRoll(InputAction.CallbackContext context)
        {
            _roll = context.ReadValue<float>();
        }

        public void OnPitchYaw(InputAction.CallbackContext context)
        {
            _pitchYaw = context.ReadValue<Vector2>();
            _pitch = Mathf.Lerp(_pitch, _pitchYaw.y, Time.deltaTime * 1f);
            _yaw = Mathf.Lerp(_yaw, _pitchYaw.x, Time.deltaTime * 1f);
        }

        public void OnBoost(InputAction.CallbackContext context)
        {
            _isBoosting = context.performed;
        }

        #endregion
    }
}