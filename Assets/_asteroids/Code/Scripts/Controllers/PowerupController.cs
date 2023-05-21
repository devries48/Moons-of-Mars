using MoonsOfMars.Shared;
using System.Collections;
using UnityEngine;

namespace MoonsOfMars.Game.Asteroids
{
    [SelectionBase]
    [RequireComponent(typeof(Rigidbody), typeof(Renderer))]
    public class PowerupController : GameBase
    {
        #region editor fields

        [SerializeField] AudioSource clipsAudioSource;

        #endregion

        #region properties
        Rigidbody Rb
        {
            get
            {
                if (__rb == null)
                    __rb = GetComponent<Rigidbody>();

                return __rb;
            }
        }
        Rigidbody __rb;

        internal Renderer Renderer
        {
            get
            {
                if (__renderer == null)
                    __renderer = GetComponent<Renderer>();

                return __renderer;
            }
        }
        Renderer __renderer;

        #endregion

        #region fields
        bool _isAlive;

        internal PowerupManagerData.Powerup m_powerup;
        #endregion

        void OnEnable()
        {
            m_ScreenWrap = true;
            _isAlive = true;

            Renderer.enabled = true;
            Renderer.material.SetFloat("_Dissolve", 0);

            RigidbodyUtil.SetRandomForce2D(Rb, 100f);
            RigidbodyUtil.SetRandomTorque(Rb, 250f);

            SetRandomPowerUp();
            StartCoroutine(ManagerPowerup.PlayDelayedAudio(PowerupSounds.Clip.Eject, clipsAudioSource, .1f));
            StartCoroutine(KeepAliveLoop());

            ManagerLevel.AddStatistic(LevelManager.Statistic.powerupSpawn);
        }

        void OnCollisionEnter(Collision other)
        {
            if (!_isAlive) return;

            var c = other.collider;
            var o = other.gameObject;

            print("Powerup collider");
            if (c.CompareTag("Enemy") || c.CompareTag("Player"))
                HitByShip(o);
        }

        void OnTriggerEnter(Collider other)
        {
            if (!_isAlive) return;

            var c = other;
            var o = other.gameObject;

            if (c.CompareTag("Enemy") || c.CompareTag("Player"))
                HitByShip(o);

            else if (c.CompareTag("AlienBullet"))
                HitByAlienBullet(o);

            else if (c.CompareTag("Bullet"))
                HitByBullet(o);

            else if (c.CompareTag("Shield"))
                HitByShield(o);
        }

        IEnumerator KeepAliveLoop()
        {
            float timePassed = 0;

            while (_isAlive && timePassed < ManagerPowerup.m_ShowTime)
            {
                timePassed += Time.deltaTime;

                yield return null;
            }
            DissolvePowerup();
        }

        void HitByShield(GameObject o)
        {
            _isAlive = false;

            o.TryGetComponent(out ShieldController shield);
            if (shield != null)
                HitByShip(shield.m_spaceShip.gameObject);
        }

        void HitByShip(GameObject o)
        {
            _isAlive = false;
            o.TryGetComponent(out SpaceShipBase ship);

            if (ship != null)
                StartCoroutine(PickupPowerup(ship));
            else
                RemoveFromGame();
        }

        void HitByAlienBullet(GameObject alienBullet)
        {
            RemoveFromGame(alienBullet);
            Score(ManagerPowerup.GetDestructionScore(true), gameObject);
            StartCoroutine(ExplodePowerup());
        }

        void HitByBullet(GameObject bullet)
        {
            RemoveFromGame(bullet);
            Score(ManagerPowerup.GetDestructionScore(false), gameObject);
            ManagerLevel.AddStatistic(LevelManager.Statistic.powerupDestroyed);
            StartCoroutine(ExplodePowerup());
        }

        void DissolvePowerup()
        {
            if (!_isAlive) return;

            LeanTween.value(0f, 1f, 2f)
                .setOnUpdate((float val) =>
                    {
                        Renderer.material.SetFloat("_Dissolve", val);
                    })
                .setOnComplete(() =>
                {
                    if (_isAlive)
                    {
                        _isAlive = false;
                        RemoveFromGame();
                    }
                })
                .setEaseInQuint();
        }

        IEnumerator ExplodePowerup()
        {
            _isAlive = false;
            Renderer.enabled = false;

            PlayEffect(EffectsManager.Effect.ExplosionSmall, transform.position, .5f);
            ManagerPowerup.PlayAudio(PowerupSounds.Clip.Explode, clipsAudioSource);

            while (clipsAudioSource.isPlaying)
                yield return null;

            RemoveFromGame();
        }

        IEnumerator PickupPowerup(SpaceShipBase ship)
        {
            Renderer.enabled = false;

            Score(ManagerPowerup.GetPickupScore(ship.IsEnemy), gameObject);
            if (!ship.IsEnemy)
                ManagerLevel.AddStatistic(LevelManager.Statistic.powerupPickup);

            switch (m_powerup)
            {
                case PowerupManagerData.Powerup.jump:
                    ship.AddHyperJump();
                    break;
                case PowerupManagerData.Powerup.shield:
                    ship.ActivateShield();
                    break;
                case PowerupManagerData.Powerup.weapon:
                    var pwr = RandomEnumUtil<PowerupManagerData.PowerupWeapon>.Get(1);
                    ship.ActivateWeaponPowerup(pwr);
                    break;
                default:
                    break;
            }

            if (ship.IsEnemy)
            {
                ManagerPowerup.PlayAudio(PowerupSounds.Clip.PickupEnemy, clipsAudioSource);

                while (clipsAudioSource.isPlaying)
                    yield return null;
            }

            RemoveFromGame();
        }

        void SetRandomPowerUp()
        {
            m_powerup = RandomEnumUtil<PowerupManagerData.Powerup>.Get();
            ManagerPowerup.SetPowerupMaterial(this);
        }

    }
}