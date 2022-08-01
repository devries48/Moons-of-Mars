using System;
using System.Collections;
using UnityEngine;

namespace Game.Astroids
{
    [SelectionBase]
    [RequireComponent(typeof(Rigidbody), typeof(AudioSource))]
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

        protected bool AreShieldsUp => shield != null && shield.ShieldsUp;

        #endregion

        internal bool m_isAlive;
        protected bool m_isEnemy = false;
        protected bool m_canShoot = true;

        GameObjectPool _bulletPool;

        protected virtual void Awake()
        {
            BuilPool();
        }

        protected virtual void OnEnable()
        {
            m_isAlive = true;
        }

        protected virtual void OnTriggerEnter(Collider collider)
        {
            if (m_isAlive && !AreShieldsUp)
            {
                if (collider.CompareTag("Bullet"))
                {
                    HitByBullet(collider.gameObject);
                    print("trigger: bullet");
                }
            }
        }

        void BuilPool()
        {
            if (bulletPrefab)
                _bulletPool = GameObjectPool.Build(bulletPrefab, 8, 16);
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
            Bullet().Fire(weapon.transform.up);

            yield return new WaitForSeconds(fireRate);

            m_canShoot = true;
        }

        BulletController Bullet()
        {
            return _bulletPool.GetComponentFromPool<BulletController>(
                weapon.transform.position,
                weapon.transform.rotation);
        }

        public void PlayAudioClip(SpaceShipSounds.Clip clip) => sounds.PlayClip(clip);

        protected virtual void HitByBullet(GameObject bullet)
        {
            m_isAlive = false;

            RemoveFromGame(bullet);
            StartCoroutine(ExplodeUfo());
        }

        IEnumerator ExplodeUfo()
        {
            PlayEffect(EffectsManager.Effect.greenExplosion, transform.position, 1.2f);
            PlayAudioClip(SpaceShipSounds.Clip.ShipExplosion);

            while (Audio.isPlaying)
            {
                yield return null;
            }

            print("ExplodeUFO");
            RemoveFromGame();
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