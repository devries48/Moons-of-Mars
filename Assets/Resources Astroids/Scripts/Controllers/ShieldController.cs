using UnityEngine;

namespace Game.Astroids
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
        [Range(0.1f, 5f)]
        [SerializeField] float dampenTime = 1.5f;

        // maximum displacement on impact
        [Range(0.002f, 0.1f)]
        [SerializeField] float impactRippleAmplitude = 0.005f;
        [Range(0.05f, 0.5f)]
        [SerializeField] float impactRippleMaxRadius = 0.35f;

        MeshRenderer Renderer
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
            get
            {
                return Renderer != null && Renderer.enabled;
            }
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
                HitByBullet(o,true);
        }

        void HitByAstroid(GameObject other)
        {
            if (!autoActivate) return;

            SetShieldsUp(true);

            // Player shield hit destroys astroid, handled by AstroidController
            if (!m_spaceShip.m_isEnemy)
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
            SetShieldsUp(true);
        }

        void HitByBullet(GameObject bullet, bool isEnemy)
        {
            if (m_spaceShip.m_isEnemy && isEnemy)
            {
                PoolableMonoBehaviour.RemoveFromGame(bullet);
                SetShieldsUp(true);
            } else if(ShieldsUp)
                PoolableMonoBehaviour.RemoveFromGame(bullet);
        }

        void SetShieldsUp(bool isAuto)
        {
            if (m_spaceShip == null)
                return;

            if (isAuto)
                m_spaceShip.PlayAudioClip(SpaceShipSounds.Clip.shieldsHit);
            else
                m_spaceShip.PlayAudioClip(SpaceShipSounds.Clip.shieldsUp);


            if (autoActivate)
                Renderer.enabled = true;

            _visibleTimer = shieldVisibleTimer;
        }

        void SetShieldsDown()
        {
            Renderer.enabled = false;

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