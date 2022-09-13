using System.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Astroids
{
    [SelectionBase]
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
                    __rb = GetComponentInChildren<Rigidbody>();

                return __rb;
            }
        }
        Rigidbody __rb;

        protected bool AreShieldsUp => shield != null && shield.ShieldsUp;

        #endregion

        #region fields

        internal bool m_isAlive;
        internal bool m_isEnemy = false;
        protected bool m_isAllied = false;
        protected bool m_canShoot = true;

        GameObjectPool _bulletPool;
        #endregion

        protected virtual void Awake() => BuilPool();

        protected virtual void OnEnable() => m_isAlive = true;

        protected virtual void OnTriggerEnter(Collider collider)
        {
            if (m_isAlive && !AreShieldsUp)
            {
                if (collider.CompareTag("Bullet"))
                    HitByBullet(collider.gameObject);
            }
        }

        public void PlayAudioClip(SpaceShipSounds.Clip clip)
        {
            sounds?.PlayClip(clip);
        }

        public void PlayAudioClip(AudioClip clip)
        {
            sounds?.PlayClip(clip);
        }

        protected virtual void FireWeapon() => StartCoroutine(Shoot());

        protected virtual void HitByBullet(GameObject bullet)
        {
            m_isAlive = false;

            RemoveFromGame(bullet);
            StartCoroutine(ExplodeUfo());
        }

        void BuilPool()
        {
            if (bulletPrefab)
                _bulletPool = GameObjectPool.Build(bulletPrefab, 8, 16);
        }

        BulletController Bullet()
        {
            return _bulletPool.GetComponentFromPool<BulletController>(
                weapon.transform.position, quaternion.identity);
        }

        IEnumerator Shoot()
        {
            if (!bulletPrefab || !m_isAlive)
                yield break;

            m_canShoot = false;

            PlayAudioClip(SpaceShipSounds.Clip.ShootBullet);
            Bullet().Fire(weapon.transform.up);

            yield return new WaitForSeconds(fireRate);

            m_canShoot = true;
        }

        IEnumerator ExplodeUfo()
        {
            PlayEffect(EffectsManager.Effect.greenExplosion, transform.position, 1.2f);
            PlayAudioClip(SpaceShipSounds.Clip.ShipExplosion);

            while (Audio.isPlaying)
            {
                yield return null;
            }

            RemoveFromGame();
        }
    }
}