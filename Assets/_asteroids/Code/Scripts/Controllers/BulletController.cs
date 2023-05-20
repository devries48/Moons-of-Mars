using UnityEngine;

namespace MoonsOfMars.Game.Asteroids
{
    using static GameManager;
    using static SpaceShipMonoBehaviour;

    [SelectionBase]
    [RequireComponent(typeof(Rigidbody))]
    public class BulletController : GameMonoBehaviour
    {
        [SerializeField, Tooltip("Lifetime in seconds")]
        float bulletLifetime = 10f;

        [SerializeField]
        int bulletSpeed = 25;

        Rigidbody Rb
        {
            get
            {
                if (__rb == null)
                    __rb = GetComponentInChildren<Rigidbody>();

                return __rb;
            }
        }
        Rigidbody __rb;

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

        void OnEnable()
        {
            transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            InvokeRemoveFromGame(bulletLifetime);
        }

        public virtual void Fire(Vector3 direction, ShipType type)
        {
            if (type == ShipType.ufoGreen || type == ShipType.ufoRed)
                GmManager.UfoManager.SetBulletMaterial(this, type);

            Rb.velocity = direction * bulletSpeed;
        }
    }
}