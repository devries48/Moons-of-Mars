using MoonsOfMars.Shared;
using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace MoonsOfMars.Game.Asteroids
{
    using static PowerupManagerData;

    [SelectionBase]
    public class SpaceShipBase : GameBase
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
        internal bool m_isAlive;
        internal float m_pwrShieldTime;
        internal float m_pwrWeaponTime;

        protected bool _disableControls = true;

        GameObjectPool _bulletPool;
        PowerupWeapon _weaponPowerup;
        int _hyperJumps;
        bool _canShoot;
        #endregion

        public event Action<float, Powerup, PowerupWeapon?> PowerUpActivatedEvent = delegate { };

        #region unity events
        protected virtual void Start() => BuildObjectPools();

        protected virtual void OnEnable()
        {
            ShowModel();

            m_isAlive = true;
            m_pwrShieldTime = 0;
            m_pwrWeaponTime = 0;

            _weaponPowerup = PowerupWeapon.normal;
            _hyperJumps = 0;
            _canShoot = true;
        }

        protected virtual void OnCollision(Collision other)
        {
            if (!m_isAlive) return;

            var c = other.collider;

            if (!AreShieldsUp)
                if (c.CompareTag("Enemy") && !IsEnemy)
                {
                    var ctrl = c.GetComponent<SpaceShipBase>();
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

        void BuildObjectPools() => ManagerGame.CreateObjectPool(BuildPoolsAction);

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

        public void AddHyperJump() => RaisePowerUpJump();

        IEnumerator PowerupWeaponLoop()
        {
            if (m_pwrWeaponTime > 0)
            {
                m_pwrWeaponTime += ManagerPowerup.m_PowerDuration;
                RaisePowerUpWeapon();

                yield return null;
            }
            else
            {
                float orgRate = fireRate;
                if (_weaponPowerup == PowerupWeapon.firerate)
                    fireRate *= .25f;

                m_pwrWeaponTime = ManagerPowerup.m_PowerDuration;
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
                m_pwrShieldTime += ManagerPowerup.m_PowerDuration;
                RaisePowerUpShield();

                yield return null;
            }
            else
            {
                m_Shield.ShieldsUp = true;
                m_pwrShieldTime = ManagerPowerup.m_PowerDuration;
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

        protected virtual void RaisePowerUpShield() => PowerUpActivatedEvent(m_pwrShieldTime, Powerup.shield, null);

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
            m_pwrShieldTime = 0;
            m_pwrWeaponTime = 0;

            _weaponPowerup = PowerupWeapon.normal;
            _hyperJumps = 0;
            StartCoroutine(ExplodeShipCore());
        }

        protected virtual void FireWeapon() => StartCoroutine(Shoot());
        protected virtual void HideModel() => ShowModel(false);
        protected void ShowModel() => ShowModel(true);

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
            RemoveFromGame(other);

            if (IsEnemy)
            {
                TryGetComponent(out UfoController ufo);
                if (ufo != null)
                {
                    ManagerLevel.AddStatistic(LevelManager.Statistic.shotHit);
                    ManagerLevel.RemoveUfo(ufo.m_ufoType, true);
                    Explode();
                }
                else
                    Debug.LogWarning("UFO Controller not found!");
            }
            else
                ManagerGame.PlayerDestroyed();
        }

        /// <summary>
        /// Only a player ship can be hit by a shield. (Asteroids are handled in the AsteroidsController)
        /// Enemy ship can only be hit by bullets!
        /// </summary>
        void HitByShield(GameObject other)
        {
            other.TryGetComponent(out ShieldController otherShield);

            if (otherShield == null)
                return;

            ManagerGame.PlayerDestroyed();
        }

        void HitByAlienShip() => ManagerGame.PlayerDestroyed();

        void BuildPoolsAction()
        {
            if (bulletPrefab)
                _bulletPool = ManagerGame.CreateObjectPool(bulletPrefab, 8, 16);
        }

        BulletController Bullet() => _bulletPool.GetComponentFromPool<BulletController>(weapon.transform.position, quaternion.identity);

        IEnumerator Shoot()
        {
            if (!bulletPrefab || !m_isAlive || !_canShoot || m_shipType == ShipType.player && _disableControls)
                yield break;

            _canShoot = false;

            if (m_shipType == ShipType.player)
                ManagerLevel.AddStatistic(LevelManager.Statistic.shotFired);

            PlayAudioClip(SpaceShipSounds.Clip.shootBullet);

            if (_weaponPowerup == PowerupWeapon.shotSpread)
                ShootSpread();
            else
                ShootDefault();

            yield return new WaitForSeconds(fireRate);

            _canShoot = true;
        }

        void ShootDefault() => Bullet().Fire(weapon.transform.up, m_shipType);

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
                    PlayEffect(EffectsData.Effect.ExplosionBig, transform.position, .8f);
                    break;
                case ShipType.ufoGreen:
                    PlayEffect(EffectsData.Effect.ExplosionGreen, transform.position, 1.4f);
                    break;
                case ShipType.ufoRed:
                    PlayEffect(EffectsData.Effect.ExplosionRed, transform.position, 1.4f);
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