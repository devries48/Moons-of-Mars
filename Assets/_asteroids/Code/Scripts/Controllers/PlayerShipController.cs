using MoonsOfMars.Shared;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MoonsOfMars.Game.Asteroids
{
    using static GameManager;
    using static HudManager;

    public class PlayerShipController : SpaceShipMonoBehaviour
    {
        #region constants
        const float JUMP_MOVE_OUT_ANIMATION_TIME = 3;
        const float JUMP_SELECT_TIME = 5;
        const float JUMP_MOVE_IN_ANIMATION_TIME = 1.5f;
        #endregion

        #region editor fields
        [Header("Player")]
        public ThrustController m_ThrustController;

        [SerializeField] float thrust = 500f;
        [SerializeField] float rotationSpeed = 180f;
        [SerializeField] float maxSpeed = 4.5f;
        [SerializeField] float fuelPerSecond = 1f;
        #endregion

        #region properties
        protected Collider Cl
        {
            get
            {
                if (__cl == null)
                    __cl = GetComponentInChildren<Collider>();

                return __cl;
            }
        }
        Collider __cl;

        InputManager InputManager => GmManager.InputManager;
        HudManager HudManager => GmManager.m_HudManager;
        #endregion

        #region fields
        bool _isJumping = false;
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
        public event Action<HudAction> HudActionEvent = delegate { };

        #region unity events
        protected override void OnEnable()
        {
            m_ScreenWrap = true;

            _thrustInput = 0f;
            _turnInput = 0f;
            _fuelUsed = 0f;
            _spawnFader ??= new MaterialFader(m_Model);

            InputManager.OnHyperJumpPressed += HandleHyperJump;
            InputManager.OnPausePressed += HandlePauseGame;

            RaiseFuelChangedEvent();
            SpawnIn();
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            InputManager.OnHyperJumpPressed -= HandleHyperJump;
            InputManager.OnPausePressed -= HandlePauseGame;
        }

        void Update()
        {
            if (GmManager.IsGamePaused || _disableControls) return;

            //_turnInput = ShipInput.GetTurnAxis();
            //_thrustInput = ShipInput.GetForwardThrust();
            _turnInput = InputManager.TurnInput;
            _thrustInput = InputManager.Thrust;

            if (m_ThrustController)
            {
                if (_fuelUsed >= 100)
                    m_ThrustController.SetThrust(0);
                else if (_thrustInput > 0)
                    m_ThrustController.IncreaseThrust();
                else
                    m_ThrustController.DecreaseThrust();
            }

            if (InputManager.IsShooting)
                FireWeapon();

            //if (ShipInput.IsHyperspacing() && HudManager.HyperJumps > 0)
            //    Jump();

            //if (InputManager.IsPauseGame)
            //    GmManager.GamePause();
        }

        protected override void FixedUpdate()
        {
            if (_isJumping || _disableControls)
                return;

            Move();
            Turn();
            ClampSpeed();
            base.FixedUpdate();
        }
        #endregion

        #region InputManager
        void HandlePauseGame() => GmManager.GamePause();

        void HandleHyperJump()
        {
            if (HudManager.HyperJumps > 0)
                HyperJump();
        }
        #endregion

        public void Spawn()
        {
            PlayEffect(EffectsManager.Effect.Spawn, transform.position, .7f);
            Recover();
        }

        public void Refuel() => _fuelUsed = 0f;

        /// <summary>
        /// Move ship to top, out of view.
        /// </summary>
        public void HyperJump()
        {
            _isJumping = true;
            m_ScreenWrap = false;
            Cl.enabled = false;

            DisableControls();
            RaiseHudActionEvent(HudAction.hyperjumpStart);

            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);

            var target = new Vector3(transform.position.x, GmManager.m_camBounds.TopEdge + 1f, 0);
            var currentPos = transform.position;

            LeanTween.move(gameObject, target, 1f)
                .setOnComplete(() =>
                {
                    StartCoroutine(HyperJump(currentPos));
                    HideModel();
                })
                .setEaseOutQuad();

            if (m_ThrustController != null)
                LeanTween.value(gameObject, 1f, 0f, 1f).setOnUpdate((float val)
                    => m_ThrustController.SetThrust(val)).setEaseInQuint();
        }

        IEnumerator HyperJump(Vector3 currentPos)
        {
            // Rocket moves towards camera out-of-view
            var ctrl = GmManager.HyperJump(JUMP_MOVE_OUT_ANIMATION_TIME);
            yield return new WaitForSeconds(JUMP_MOVE_OUT_ANIMATION_TIME);

            // Cursor selects new position of rocket
            RaiseHudActionEvent(HudAction.hyperjumpSelect);
            GmManager.JumpSelect(currentPos, JUMP_SELECT_TIME);

            while (!GmManager.JumpLaunched())
                yield return null;

            var jumpPos = GmManager.JumpPosition();

            // Rocket move to new position in a straight line
            RaiseHudActionEvent(HudAction.none);
            ctrl.transform.position = new Vector3(jumpPos.x, jumpPos.y, ctrl.transform.position.z);
            ctrl.PlayerShipJumpIn(JUMP_MOVE_IN_ANIMATION_TIME);
            yield return new WaitForSeconds(JUMP_MOVE_IN_ANIMATION_TIME * .2f);

            GmManager.PlayEffect(EffectsManager.Effect.JumpPortal, jumpPos, .5f);
            yield return new WaitForSeconds(JUMP_MOVE_IN_ANIMATION_TIME * .8f);

            // Activate rocket at new position
            GmManager.JumpDeactivate();
            transform.position = jumpPos;

            _isJumping = false;
            m_ScreenWrap = true;
            Cl.enabled = true;
            ShowModel();
            EnableControls();

            HudManager.PlayClip(HudSounds.Clip.hyperJumpComplete);
        }

        /// <summary>
        /// Stage complete, hide or show ship
        /// </summary>
        public void Teleport(bool show = false)
        {
            if (show)
            {
                ShowModel();
                EnableControls();
            }
            else
            {
                HideModel();
                DisableControls();
            }
        }

        public void EnableControls() => _disableControls = false;

        public void DisableControls() => _disableControls = true;

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

        void Turn() => transform.Rotate(0, 0, _turnInput * rotationSpeed * Time.deltaTime);

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
                ResetPosition();
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

        void RaiseHudActionEvent(HudAction action) => HudActionEvent(action);

        public void ResetPosition() => transform.SetPositionAndRotation(Vector3.zero, _initialRotation);

        void ResetRigidbody() => RigidbodyUtil.Reset(GetComponent<Rigidbody>());

    }
}