using UnityEngine;

namespace Game.Astroids
{
    [SelectionBase]
    [RequireComponent(typeof(Rigidbody))]
    public class BulletController : GameMonoBehaviour
    {
        [SerializeField, Tooltip("Lifetime in seconds")]
        float bulletLifetime = 10f;

        [SerializeField]
        int bulletSpeed = 25;

        Rigidbody _rb;

        void Awake() => _rb = GetComponent<Rigidbody>();

        void OnEnable() => InvokeRemoveFromGame(bulletLifetime);

        public virtual void Fire(Vector3 direction)
        {
            _rb.velocity = direction * bulletSpeed;
        }
    }
}