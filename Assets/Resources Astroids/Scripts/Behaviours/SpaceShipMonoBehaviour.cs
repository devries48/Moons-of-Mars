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

        [SerializeField, Tooltip("Select model only when ship can explode")]
        GameObject model;

        [SerializeField, Tooltip("Select shield gameobject")]
        protected ShieldController shield;

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

        protected virtual void OnEnable()
        {
            ShowModel();

            m_isAlive = true;
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            var c = other;
            var o = other.gameObject;

            if (m_isAlive && !AreShieldsUp)
                if (c.CompareTag("Bullet") && m_isEnemy || c.CompareTag("AlienBullet") && !m_isEnemy)
                    HitByBullet(o);
                else if (c.CompareTag("Enemy"))
                    HitByShield(o);
        }

        public void PlayAudioClip(SpaceShipSounds.Clip clip) => sounds?.PlayClip(clip);

        public void Explode()
        {
            m_isAlive = false;
            StartCoroutine(ExplodeShip());
        }

        protected virtual void FireWeapon() => StartCoroutine(Shoot());

        /// <summary>
        /// Handle bullet hits
        /// </summary>
        protected virtual void HitByBullet(GameObject other)
        {
            RemoveFromGame(other);

            if (m_isEnemy)
            {
                GameManager.UfoDestroyed();
                Explode();
            }
            else
                GameManager.PlayerDestroyed(gameObject);
        }

        /// <summary>
        /// Only a player ship can be hit by a shield. (Astroids are handled in the AstroidsController)
        /// Enemy ship can only be hit by bullets!
        /// </summary>
        void HitByShield(GameObject other)
        {
            other.TryGetComponent(out ShieldController otherShield);

            if (otherShield == null)
                return;

            GameManager.PlayerDestroyed(gameObject);
        }

        void BuilPool()
        {
            if (bulletPrefab)
                _bulletPool = GameObjectPool.Build(bulletPrefab, 8, 16);
        }

        BulletController Bullet() => _bulletPool.GetComponentFromPool<BulletController>(weapon.transform.position, quaternion.identity);

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

        IEnumerator ExplodeShip()
        {
            HideModel();

            if (m_isEnemy)
                PlayEffect(EffectsManager.Effect.greenExplosion, transform.position, 1.2f);
            else
                PlayEffect(EffectsManager.Effect.bigExplosion, transform.position);

            PlayAudioClip(SpaceShipSounds.Clip.ShipExplosion);

            while (Audio.isPlaying)
                yield return null;

            RemoveFromGame();
        }

        void ShowModel(bool show = true)
        {
            if (model != null)
            {
                foreach (var rend in model.GetComponentsInChildren<Renderer>())
                    rend.enabled = show;
            }

        }

        void HideModel() => ShowModel(false);
    }
}