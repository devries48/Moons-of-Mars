using System.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Game.Astroids
{
    [SelectionBase]
    public class SpaceShipMonoBehaviour : GameMonoBehaviour
    {
        public enum ShipType
        {
            player,
            ufoGreen,
            ufoRed
        }

        #region editor fields
        [Header("Spaceship")]

        [Tooltip("Select model only when ship can explode")]
        public GameObject m_Model;

        [Tooltip("Select shield gameobject")]
        public ShieldController m_Shield;

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
        protected PowerupManager PwrManager
        {
            get
            {
                if (__pwrManager == null)
                    __pwrManager = GameManager.m_PowerupManager;

                return __pwrManager;
            }
        }
        PowerupManager __pwrManager;

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

        protected bool AreShieldsUp => m_Shield != null && m_Shield.ShieldsUp;

        internal bool IsEnemy => m_shipType is ShipType.ufoGreen or ShipType.ufoRed;
        #endregion

        #region fields
        internal bool m_isAlive;
        protected bool m_canShoot = true;

        internal ShipType m_shipType = ShipType.player;

        internal float m_pwrFireRateTime;
        internal float m_pwrShieldTime;
        internal float m_pwrWeaponTime;

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
                if (c.CompareTag("Enemy") && !IsEnemy)
                    HitByAlienShip();
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (!m_isAlive) return;

            var c = other;
            var o = other.gameObject;

            if (!AreShieldsUp)
                if (c.CompareTag("Bullet") && IsEnemy || c.CompareTag("AlienBullet") && !IsEnemy)
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
                    StartCoroutine(PowerupShieldLoop());
                    break;
                default:
                    break;
            }
        }

        IEnumerator PowerupFireRateLoop()
        {
            if (m_pwrFireRateTime > 0)
            {
                m_pwrFireRateTime += GameManager.m_PowerupManager.m_PowerDuration;
                yield return null;
            }
            else
            {
                var orgVal = fireRate;
                fireRate *= .25f;
                m_pwrFireRateTime = GameManager.m_PowerupManager.m_PowerDuration;

                while (m_isAlive && m_pwrFireRateTime > 0)
                {
                    m_pwrFireRateTime -= Time.deltaTime;
                    yield return null;
                }

                fireRate = orgVal;
                m_pwrFireRateTime = 0;
            }
        }

        IEnumerator PowerupShieldLoop()
        {
            if (m_pwrShieldTime > 0)
            {
                m_pwrShieldTime += GameManager.m_PowerupManager.m_PowerDuration;
                yield return null;
            }
            else
            {
                print("powerup shield");
                m_Shield.ShieldsUp = true;
                m_pwrShieldTime = GameManager.m_PowerupManager.m_PowerDuration;

                while (m_isAlive && m_pwrShieldTime > 0)
                {
                    m_pwrShieldTime -= Time.deltaTime;
                    yield return null;
                }
                m_pwrShieldTime = 0;
                m_Shield.ShieldsUp = false;
            }
        }

        public void PlayAudioClip(SpaceShipSounds.Clip clip) => sounds?.PlayClip(clip);

        public void Explode()
        {
            m_isAlive = false;
            StartCoroutine(ExplodeShipCore());
        }

        protected virtual void FireWeapon() => StartCoroutine(Shoot());

        protected virtual void HideModel() => ShowModel(false);

        void ShowModel(bool show = true)
        {
            if (m_Model != null)
            {
                foreach (var rend in m_Model.GetComponentsInChildren<Renderer>())
                    rend.enabled = show;
            }
        }

        /// <summary>
        /// Handle bullet hits.
        /// </summary>
        protected virtual void HitByBullet(GameObject other)
        {
            print("schip door: " + other.name);
            RemoveFromGame(other);

            if (IsEnemy)
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
            Bullet().Fire(weapon.transform.up, m_shipType);

            yield return new WaitForSeconds(fireRate);

            m_canShoot = true;
        }

        IEnumerator ExplodeShipCore()
        {
            HideModel();

            switch (m_shipType)
            {
                case ShipType.player:
                    PlayEffect(EffectsManager.Effect.bigExplosion, transform.position, .8f);
                    break;
                case ShipType.ufoGreen:
                    PlayEffect(EffectsManager.Effect.greenExplosion, transform.position, 1.4f);
                    break;
                case ShipType.ufoRed:
                    PlayEffect(EffectsManager.Effect.redExplosion, transform.position, 1.4f);
                    break;
                default:
                    break;
            }

            PlayAudioClip(SpaceShipSounds.Clip.shipExplosion);

            while (Audio.isPlaying)
                yield return null;

            RemoveFromGame();
        }


    }
}