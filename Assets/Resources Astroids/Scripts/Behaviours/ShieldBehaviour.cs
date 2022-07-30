using UnityEngine;

namespace Game.Astroids
{
    [RequireComponent(typeof(MeshRenderer))]
    public class ShieldBehaviour : MonoBehaviour
    {
        [SerializeField]
        SpaceShipMonoBehaviour spaceShip;

        [SerializeField,Tooltip("The magnitude of the force when hit by an astroid")]
        int magnitude = 2000;

        [SerializeField]
        float shieldVisibleTimer = 2f;

        [SerializeField, Tooltip("Shield will activate automaticly")]
        bool autoActivate = false;

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
            if (other.CompareTag("Astroid"))
            {
                SetShieldsUp(true);

                var force = transform.position - other.transform.position;

                other.TryGetComponent<Rigidbody>(out var rb);
                if (rb == null)
                    return;

                force.Normalize();
                rb.AddForce(-force * magnitude);
            }
            else if (ShieldsUp)
            {
                if (other.CompareTag("Bullet") || other.CompareTag("AlienBullet")){
                    GameMonoBehaviour.RemoveFromGame(other.gameObject);
                }
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
    }
}