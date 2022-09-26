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

        internal float m_pwrFireRateTime;

        GameObjectPool _bulletPool;
        #endregion

        #region unity events
        protected virtual void Awake() => BuilPool();

        protected virtual void OnEnable()
        {
            ShowModel();

            m_isAlive = true;
        }

        protected virtual void OnCollision(Collision other)
        {
            if (!m_isAlive) return;

            var c = other.collider;

            if (!AreShieldsUp)
                if (c.CompareTag("Enemy") && !m_isEnemy)
                    HitByAlienShip();
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (!m_isAlive) return;

            var c = other;
            var o = other.gameObject;

            if (!AreShieldsUp)
                if (c.CompareTag("Bullet") && m_isEnemy || c.CompareTag("AlienBullet") && !m_isEnemy)
                    HitByBullet(o);

                else if (c.CompareTag("Shield"))
                    HitByShield(o);
        }
        #endregion

        public void ActivatePowerup(PowerupManager.Powerup powerup)
        {
            switch (powerup)
            {
                case PowerupManager.Powerup.fireRate:
                    StartCoroutine(PowerupFireRateLoop());
                    break;
                case PowerupManager.Powerup.shield:
                    break;
                default:
                    break;
            }
        }

        IEnumerator PowerupFireRateLoop()
        {
            if (m_pwrFireRateTime > 0)
            {
                m_pwrFireRateTime += GameManager.m_powerupManager.m_powerDuration;
                yield return null;
            }
            else
            {
                PlayAudioClip(SpaceShipSounds.Clip.powerupPickup);

                var orgVal = fireRate;
                fireRate *= .5f;
                m_pwrFireRateTime = GameManager.m_powerupManager.m_powerDuration;

                while (m_isAlive && m_canShoot & m_pwrFireRateTime > 0)
                {
                    m_pwrFireRateTime -= Time.deltaTime;
                    yield return null;
                }

                fireRate = orgVal;
                m_pwrFireRateTime = 0;
            }
        }

        public void PlayAudioClip(SpaceShipSounds.Clip clip) => sounds?.PlayClip(clip);

        public void Explode()
        {
            m_isAlive = false;
            StartCoroutine(ExplodeShip());
        }

        protected virtual void FireWeapon() => StartCoroutine(Shoot());

        protected virtual void HideModel() => ShowModel(false);

        void ShowModel(bool show = true)
        {
            if (model != null)
            {
                foreach (var rend in model.GetComponentsInChildren<Renderer>())
                    rend.enabled = show;
            }
        }

        /// <summary>
        /// Handle bullet hits.
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

        void HitByAlienShip()
        {
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

            PlayAudioClip(SpaceShipSounds.Clip.shootBullet);
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
                PlayEffect(EffectsManager.Effect.bigExplosion, transform.position, .8f);

            PlayAudioClip(SpaceShipSounds.Clip.shipExplosion);

            while (Audio.isPlaying)
                yield return null;

            RemoveFromGame();
        }


    }
}