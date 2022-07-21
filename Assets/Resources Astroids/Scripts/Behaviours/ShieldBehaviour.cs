using UnityEngine;

namespace Game.Astroids
{
    [RequireComponent(typeof(MeshRenderer))]
    public class ShieldBehaviour : MonoBehaviour
    {
        [SerializeField]
        SpaceShipMonoBehaviour spaceShip;

        [SerializeField]
        int magnitude = 2000;

        [SerializeField]
        float shieldVisibleTimer = 2f;

        [SerializeField,Tooltip("Shield will activate automaticly")]
        bool autoActivate = false;

        MeshRenderer MeshRend
        {
            get
            {
                if (__meshRend == null)
                    TryGetComponent(out __meshRend);

                return __meshRend;
            }
        }
        MeshRenderer __meshRend;

        float _visibleTimer;

        void Awake()
        {
            if (autoActivate)
                MeshRend.enabled = false;
        }

        void Update()
        {
            if (_visibleTimer > 0f)
            {
                _visibleTimer -= Time.deltaTime;

                if (_visibleTimer <= 0f)
                    ShieldsDown();
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Astroid"))
            {
                ShieldsUp(true);

                var force = transform.position - other.transform.position;

                other.TryGetComponent<Rigidbody>(out var rb);
                if (rb == null)
                    return;

                force.Normalize();
                rb.AddForce(-force * magnitude);
            }
        }

        void ShieldsUp(bool isAuto)
        {
            if (spaceShip == null)
                Debug.LogWarning("SpaceShip on ShieldBehaviour is NULL");
            else
            {
                if (isAuto)
                    spaceShip.PlayAudioClip(SpaceShipSounds.Clip.ShieldsHit);
                else
                    spaceShip.PlayAudioClip(SpaceShipSounds.Clip.ShieldsUp);
            }

            if (autoActivate)
                MeshRend.enabled = true;
            
            _visibleTimer = shieldVisibleTimer;
        }

        void ShieldsDown()
        {
            MeshRend.enabled = false;
            if (spaceShip == null)
                Debug.LogWarning("SpaceShip on ShieldBehaviour is NULL");
            else
                    spaceShip.PlayAudioClip(SpaceShipSounds.Clip.ShieldsDown);

        }
    }
}