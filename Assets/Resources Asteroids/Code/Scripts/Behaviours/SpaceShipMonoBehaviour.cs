using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using static Game.Astroids.PowerupManager;

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

        internal ShipType m_shipType = ShipType.player;

        protected bool m_canShoot = true;
        internal bool m_isAlive;

        internal float m_pwrShieldTime;
        internal float m_pwrWeaponTime;

        GameObjectPool _bulletPool;
        PowerupWeapon _weaponPowerup;
        int _hyperJumps;
        #endregion

        public event Action<float, Powerup, PowerupWeapon?> PowerUpActivatedEvent = delegate { };

        #region unity events
        protected virtual void Awake() => BuilPool();

        protected virtual void OnEnable()
        {
            ShowModel();

            m_isAlive = true;
            m_pwrShieldTime = 0;
            m_pwrWeaponTime = 0;

            _weaponPowerup = PowerupWeapon.normal;
            _hyperJumps = 0;
        }

        protected virtual void OnCollision(Collision other)
        {
            if (!m_isAlive) return;

            var c = other.collider;

            if (!AreShieldsUp)
                if (c.CompareTag("Enemy") && !IsEnemy)
                {
                    var ctrl = c.GetComponent<SpaceShipMonoBehaviour>();
                    if (ctrl.m_isAlive)
                        HitByAlienShip();
                }
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

        #region Powerups
        public void ActivateShield()
        {
            if (m_shipType == ShipType.player)
                StartCoroutine(PowerupShieldLoop());
            else
                RaisePowerUpShield();
        }

        public void ActivateWeaponPowerup(PowerupWeapon weapon)
        {
            _weaponPowerup = weapon;
            StartCoroutine(PowerupWeaponLoop());
        }

        public void AddHyperJump()
        {
            RaisePowerUpJump();
        }

        IEnumerator PowerupWeaponLoop()
        {
            if (m_pwrWeaponTime > 0)
            {
                m_pwrWeaponTime += GameManager.m_PowerupManager.m_PowerDuration;
                RaisePowerUpWeapon();

                yield return null;
            }
            else
            {
                float orgRate = fireRate;
                if (_weaponPowerup == PowerupWeapon.firerate)
                    fireRate *= .25f;

                m_pwrWeaponTime = GameManager.m_PowerupManager.m_PowerDuration;
                RaisePowerUpWeapon();

                while (m_isAlive && m_pwrWeaponTime > 0)
                {
                    m_pwrWeaponTime -= Time.deltaTime;
                    yield return null;
                }

                if (_weaponPowerup == PowerupWeapon.firerate)
                    fireRate = orgRate;

                _weaponPowerup = PowerupWeapon.normal;
                m_pwrWeaponTime = 0;
            }
        }

        IEnumerator PowerupShieldLoop()
        {
            if (m_pwrShieldTime > 0)
            {
                m_pwrShieldTime += GameManager.m_PowerupManager.m_PowerDuration;
                RaisePowerUpShield();

                yield return null;
            }
            else
            {
                m_Shield.ShieldsUp = true;
                m_pwrShieldTime = GameManager.m_PowerupManager.m_PowerDuration;
                RaisePowerUpShield();

                while (m_isAlive && m_pwrShieldTime > 0)
                {
                    m_pwrShieldTime -= Time.deltaTime;
                    yield return null;
                }
                m_pwrShieldTime = 0;
                m_Shield.ShieldsUp = false;
            }
        }

        protected virtual void RaisePowerUpShield()
        {
            PowerUpActivatedEvent(m_pwrShieldTime, Powerup.shield, null);
        }

        void RaisePowerUpWeapon()
        {
            if (m_shipType == ShipType.player)
                PowerUpActivatedEvent(m_pwrWeaponTime, Powerup.weapon, _weaponPowerup);
        }

        void RaisePowerUpJump(bool remove = false)
        {
            if (m_shipType == ShipType.player)
            {
                if (remove)
                    _hyperJumps--;
                else
                    _hyperJumps++;

                PowerUpActivatedEvent(_hyperJumps, Powerup.jump, null);
            }
        }

        #endregion

        public void PlayAudioClip(SpaceShipSounds.Clip clip) => sounds?.PlayClip(clip);

        public void Explode()
        {
            m_isAlive = false;
            StartCoroutine(ExplodeShipCore());
        }

        protected virtual void FireWeapon() => StartCoroutine(Shoot());

        protected virtual void HideModel() => ShowModel(false);

        protected void ShowModel(bool show = true)
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
            RemoveFromGame(other);

            if (IsEnemy)
            {
                GameManager.UfoDestroyed();
                Explode();
            }
            else
                GameManager.PlayerDestroyed();
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

            GameManager.PlayerDestroyed();
        }

        void HitByAlienShip()
        {
            GameManager.PlayerDestroyed();
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

            if (_weaponPowerup == PowerupWeapon.shotSpread)
                ShootSpread();
            else
                ShootDefault();

            yield return new WaitForSeconds(fireRate);

            m_canShoot = true;
        }

        void ShootDefault()
        {
            Bullet().Fire(weapon.transform.up, m_shipType);
        }

        void ShootSpread()
        {
            for (int i = -1; i <= 1; i++)
            {
                float zDegrees = 15f;
                Vector3 direction = Quaternion.Euler(0, 0, i * zDegrees) * weapon.transform.up;
                Bullet().Fire(direction, m_shipType);
            }
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