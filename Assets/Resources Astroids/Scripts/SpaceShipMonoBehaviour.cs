using System.Collections;
using UnityEngine;

namespace Game.Astroids
{
    [SelectionBase]
    [RequireComponent(typeof(Rigidbody))]
    public class SpaceShipMonoBehaviour : GameMonoBehaviour
    {
        #region editor fields
        [Header("Spaceship")]

        [SerializeField, Tooltip("Select shield gameobject")]
        protected ShieldBehaviour shield;

        [Header("Weapon & bullet")]
        [SerializeField, Tooltip("Select a weapon gameobject")]
        protected GameObject weapon;

        [SerializeField, Tooltip("Select a bullet prefab")]
        protected GameObject bulletPrefab;

        [SerializeField, Tooltip("Fire rate in seconds")]
        protected float fireRate = 0.25f;

        [SerializeField]
        protected SpaceShipSounds sounds = new();

        #endregion

        #region properties

        protected Rigidbody Rb
        {
            get
            {
                if (__rb == null)
                    TryGetComponent(out __rb);

                return __rb;
            }
        }
        Rigidbody __rb;

        #endregion

        protected bool m_canShoot = true;

        GameObjectPool _bulletPool;

        void Awake() => BuilPool();

        void BuilPool()
        {
            if (bulletPrefab)
                _bulletPool = GameObjectPool.Build(bulletPrefab, 10, 20);
        }

        protected void FireWeapon()
        {
            StartCoroutine(Shoot());
        }

        IEnumerator Shoot()
        {
            if (!bulletPrefab)
                yield break;

            m_canShoot = false;

            PlayAudioClip(SpaceShipSounds.Clip.ShootBullet);
            Bullet().Fire(transform.up);

            yield return new WaitForSeconds(fireRate);

            m_canShoot = true;
        }

        BulletController Bullet()
        {
            return _bulletPool.GetComponentFromPool<BulletController>(
                weapon.transform.position,
                weapon.transform.rotation);
        }

        public void PlayAudioClip(SpaceShipSounds.Clip clip)
        {
            var sound = sounds.GetClip(clip);

            PlaySound(sound);
        }

        //public void ResetRocket()
        //{
        //    transform.position = new Vector2(0f, 0f);
        //    transform.eulerAngles = new Vector3(0, 180f, 0);

        //    Rb.velocity = new Vector3(0f, 0f, 0f);
        //    Rb.angularVelocity = new Vector3(0f, 0f, 0f);
        //}

    }
}