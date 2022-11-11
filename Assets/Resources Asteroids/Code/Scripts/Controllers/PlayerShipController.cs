using System;
using UnityEngine;

namespace Game.Astroids
{
    public class PlayerShipController : SpaceShipMonoBehaviour
    {
        #region editor fields
        [Header("Player")]
        public ThrustController m_ThrustController;
        [SerializeField] float thrust = 500f;
        [SerializeField] float rotationSpeed = 180f;
        [SerializeField] float maxSpeed = 4.5f;
        [SerializeField] float fuelPerSecond = 1f;
        #endregion

        #region fields
        bool _canMove = true;
        float _thrustInput;
        float _turnInput;
        float _speedInPercentage;
        float _prevSpeed;
        float _fuelUsed;

        MaterialFader _spawnFader;

        Quaternion _initialRotation = Quaternion.Euler(0, 186, 0);
        #endregion

        public event Action<float> SpeedChangedEvent = delegate { };
        public event Action<float> FuelChangedEvent = delegate { };

        #region unity events
        protected override void OnEnable()
        {
            _thrustInput = 0f;
            _turnInput = 0f;
            _fuelUsed = 0f;
            _spawnFader ??= new MaterialFader(m_Model);

            RaiseFuelChangedEvent();
            SpawnIn();
            base.OnEnable();
        }

        void Update()
        {
            GameManager.ScreenWrapObject(gameObject);

            _turnInput = ShipInput.GetTurnAxis();
            _thrustInput = ShipInput.GetForwardThrust();

            if (m_ThrustController)
            {
                if (_fuelUsed >= 100)
                    m_ThrustController.SetThrust(0);
                else if (_thrustInput > 0)
                    m_ThrustController.IncreaseThrust();
                else
                    m_ThrustController.DecreaseThrust();
            }

            if (!m_canShoot)
                return;

            if (ShipInput.IsShooting())
                FireWeapon();
        }

        void FixedUpdate()
        {
            if (!_canMove)
                return;

            Move();
            Turn();
            ClampSpeed();
        }

        #endregion

        public void Spawn()
        {
            PlayEffect(EffectsManager.Effect.spawn, transform.position, .7f);
            Recover();
        }

        public void Refuel()
        {
            _fuelUsed = 0f;
        }

        public void EnableControls()
        {
            _canMove = true;
            m_canShoot = true;
        }

        public void DisableControls()
        {
            _canMove = false;
            m_canShoot = false;
        }

        void SpawnIn()
        {
            PlayAudioClip(SpaceShipSounds.Clip.spawn);
            StartCoroutine(_spawnFader.FadeIn(true, 0));
            StartCoroutine(_spawnFader.FadeIn(false, 0));
        }

        // Create a vector in the direction the ship is facing.
        // Magnitude based on the input, speed and the time between frames.
        void Move()
        {
            if (_thrustInput == 0)
                return;

            _fuelUsed += _thrustInput * fuelPerSecond * Time.deltaTime;
            RaiseFuelChangedEvent();

            if (_fuelUsed >= 100f)
                return;

            var thrustForce = _thrustInput * thrust * Time.deltaTime * transform.up;
            Rb.AddForce(thrustForce);
        }

        void Turn()
        {
            transform.Rotate(0, 0, _turnInput * rotationSpeed * Time.deltaTime);
        }

        void ClampSpeed()
        {
            Rb.velocity = new Vector2(
                Mathf.Clamp(Rb.velocity.x, -maxSpeed, maxSpeed),
                Mathf.Clamp(Rb.velocity.y, -maxSpeed, maxSpeed));

            RaiseSpeedChangedEvent();
        }

        void Recover()
        {
            if (!m_isAlive)
            {
                ResetRigidbody();
                ResetTransform();
                gameObject.SetActive(true);
            }
        }

        void RaiseSpeedChangedEvent()
        {
            var max = Math.Sqrt(maxSpeed * maxSpeed + maxSpeed * maxSpeed);

            _speedInPercentage = Rb.velocity.magnitude / (maxSpeed * 1.1f);
            if (_speedInPercentage == _prevSpeed)
                return;

            _prevSpeed = _speedInPercentage;
            SpeedChangedEvent(_speedInPercentage);
        }

        void RaiseFuelChangedEvent()
        {
            if (m_shipType == ShipType.player)
            {
                if (_fuelUsed < 0)
                    _fuelUsed = 0;
                if (_fuelUsed > 100)
                    _fuelUsed = 100;

                FuelChangedEvent((100 - _fuelUsed) * .01f);
            }
        }

        void ResetTransform() => transform.SetPositionAndRotation(Vector3.zero, _initialRotation);

        void ResetRigidbody() => RigidbodyUtil.Reset(GetComponent<Rigidbody>());

    }
}