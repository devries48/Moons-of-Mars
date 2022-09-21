using UnityEngine;

namespace Game.Astroids
{
    public class ShieldController : MonoBehaviour
    {
        [SerializeField]SpaceShipMonoBehaviour spaceShip;

        [SerializeField, Tooltip("The magnitude of the force when hit by an astroid")]
        int magnitude = 2000;

        [SerializeField]float shieldVisibleTimer = 2f;

        [SerializeField, Tooltip("Shield will activate automaticly")]
        bool autoActivate = false;

        [Header("Impact settings")]
        [Range(0.1f, 5f)]
        [SerializeField] float dampenTime = 1.5f;

        // maximum displacement on impact
        [Range(0.002f, 0.1f)]
        [SerializeField]  float impactRippleAmplitude = 0.005f;
        [Range(0.05f, 0.5f)]
        [SerializeField]  float impactRippleMaxRadius = 0.35f;

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
            if (_visibleTimer > 0f)
            {
                _visibleTimer -= Time.deltaTime;

                if (_visibleTimer <= 0f)
                    SetShieldsDown();
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (!spaceShip.m_isAlive)
                return;

            if (other.CompareTag("Astroid"))
            {
                SetShieldsUp(true);

                var force = transform.position - other.transform.position;

                other.TryGetComponent<Rigidbody>(out var rb);
                if (rb == null)
                    return;

                force.Normalize();
                rb.AddForce(-force * magnitude);

                return;
            }

            if (other.CompareTag("Player"))
            {
                SetShieldsUp(true);

                //weg met de speler en ontplof maar
            }

            if (ShieldsUp)
            {
                if (other.CompareTag("Bullet") || other.CompareTag("AlienBullet"))
                {
                    GameMonoBehaviour.RemoveFromGame(other.gameObject);
                    print("kogel op schild");
                }
                else if (other.CompareTag("Player"))
                {

                }
                // ook astroide
            }

        }

        void SetShieldsUp(bool isAuto)
        {
            if (spaceShip == null)
                return;

            if (isAuto)
                spaceShip.PlayAudioClip(SpaceShipSounds.Clip.ShieldsHit);
            else
                spaceShip.PlayAudioClip(SpaceShipSounds.Clip.ShieldsUp);


            if (autoActivate)
                Renderer.enabled = true;

            _visibleTimer = shieldVisibleTimer;
        }

        void SetShieldsDown()
        {
            Renderer.enabled = false;

            if (spaceShip == null)
                Debug.LogWarning("SpaceShip on ShieldBehaviour is NULL");
            else
                spaceShip.PlayAudioClip(SpaceShipSounds.Clip.ShieldsDown);
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