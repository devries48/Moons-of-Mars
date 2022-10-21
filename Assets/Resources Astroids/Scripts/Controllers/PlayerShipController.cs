using UnityEngine;

namespace Game.Astroids
{
    public class PlayerShipController : SpaceShipMonoBehaviour
    {
        #region editor fields
        [Header("Player")]
        [SerializeField] ThrustController thrustController;
        [SerializeField] float thrust = 500f;
        [SerializeField] float rotationSpeed = 180f;
        [SerializeField] float maxSpeed = 4.5f;
        #endregion

        #region fields
        bool _canMove = true;
        float _thrustInput;
        float _turnInput;
        MaterialFader _spawnFader;

        Quaternion _initialRotation = Quaternion.Euler(0, 186, 0);
        #endregion

        protected override void OnEnable()
        {
            _thrustInput = 0f;
            _turnInput = 0f;

            _spawnFader ??= new MaterialFader(m_Model);
            SpawnIn();

            base.OnEnable();
        }

        void Update()
        {
            GameManager.ScreenWrapObject(gameObject);

            _turnInput = ShipInput.GetTurnAxis();
            _thrustInput = ShipInput.GetForwardThrust();

            if (thrustController)
            {
                if (_thrustInput > 0)
                    thrustController.IncreaseThrust();
                else
                    thrustController.DecreaseThrust();
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
        }

        public void Spawn()
        {
            PlayEffect(EffectsManager.Effect.spawn, transform.position, .7f);
            Recover();
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

        void ResetTransform() => transform.SetPositionAndRotation(Vector3.zero, _initialRotation);

        void ResetRigidbody() => RigidbodyUtil.Reset(GetComponent<Rigidbody>());

    }
}