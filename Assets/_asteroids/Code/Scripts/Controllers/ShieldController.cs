using MoonsOfMars.Shared;
using UnityEngine;

namespace Game.Asteroids
{
    [SelectionBase]
    public class ShieldController : MonoBehaviour
    {
        public SpaceShipMonoBehaviour m_spaceShip;

        [SerializeField, Tooltip("The magnitude of the force when hit by an astroid")]
        int magnitude = 2000;

        [SerializeField] float shieldVisibleTimer = 2f;

        [SerializeField, Tooltip("Shield will activate automaticly")]
        bool autoActivate = false;

        [Header("Impact settings")]
        [SerializeField, Range(0.1f, 5f)] float dampenTime = 1.5f;
        // maximum displacement on impact
        [SerializeField, Range(0.002f, 0.1f)] float impactRippleAmplitude = 0.005f;
        [SerializeField, Range(0.05f, 0.5f)] float impactRippleMaxRadius = 0.35f;

        public MeshRenderer Renderer
        {
            get
            {
                if (__Renderer == null)
                    TryGetComponent(out __Renderer);

                return __Renderer;
            }
        }
        MeshRenderer __Renderer;

        public bool ShieldsUp
        {
            get => Renderer != null && Renderer.enabled;
            set
            {
                if (Renderer != null)
                    Renderer.enabled = value;
            }
        }

        float _visibleTimer;

        void Awake()
        {
            if (autoActivate)
                Renderer.enabled = false;
        }

        void Update()
        {
            if (!autoActivate)
                return;

            if (_visibleTimer > 0f)
            {
                _visibleTimer -= Time.deltaTime;

                if (_visibleTimer <= 0f)
                    SetShieldsDown();
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (!m_spaceShip.m_isAlive)
                return;

            var c = other;
            var o = other.gameObject;

            if (c.CompareTag("Astroid"))
                HitByAstroid(o);
            else if (other.CompareTag("Player"))
                HitByPlayer();
            else if (c.CompareTag("Bullet"))
                HitByBullet(o, false);
            else if (c.CompareTag("AlienBullet"))
                HitByBullet(o, true);
        }

        public void AutoShieldUp(float time)
        {
            if (autoActivate)
                _visibleTimer += time;
        }

        void HitByAstroid(GameObject other)
        {
            if (autoActivate)
                SetShieldsUp(true);

            // Player shield hit destroys astroid, handled by AstroidController
            if (!m_spaceShip.IsEnemy)
                return;

            other.TryGetComponent<Rigidbody>(out var rb);
            if (rb == null)
                return;

            var force = transform.position - other.transform.position;
            force.Normalize();
            rb.AddForce(-force * magnitude);
        }

        void HitByPlayer()
        {
            if (autoActivate)
                SetShieldsUp(true);
        }

        void HitByBullet(GameObject bullet, bool isAlien)
        {
            if (ShieldsUp)
            {
                if (isAlien || m_spaceShip.m_shipType != SpaceShipMonoBehaviour.ShipType.player)
                    Poolable_MonoBehaviour.RemoveFromGame(bullet);
            }
        }

        void SetShieldsUp(bool isAuto)
        {
            if (ShieldsUp)
                return;

            if (m_spaceShip == null)
                return;

            if (isAuto)
                m_spaceShip.PlayAudioClip(SpaceShipSounds.Clip.shieldsHit);
            else
                m_spaceShip.PlayAudioClip(SpaceShipSounds.Clip.shieldsUp);

            ShieldsUp = true;

            if (autoActivate)
                _visibleTimer = shieldVisibleTimer;
        }

        void SetShieldsDown()
        {
            ShieldsUp = false;

            if (m_spaceShip == null)
                Debug.LogWarning("SpaceShip on ShieldBehaviour is NULL");
            else
                m_spaceShip.PlayAudioClip(SpaceShipSounds.Clip.shieldsDown);
        }

        void ApplyImpact(Vector3 hitPoint, Vector3 rippleDirection)
        {
            if (Renderer != null)
            {
                EnableRipple(true);
                Renderer.material.SetFloat("_impactRippleMaxRadius", impactRippleMaxRadius);
                Renderer.material.SetFloat("_impactRippleAmplitude", impactRippleAmplitude);
                Renderer.material.SetVector("_impactRippleDirection", rippleDirection);
                Renderer.material.SetVector("_impactPoint", hitPoint);

            }
        }

        void EnableRipple(bool state = false)
        {
            int onOff = state ? 1 : 0;
            Renderer.material.SetFloat("_enableRipple", onOff);
        }
    }
}