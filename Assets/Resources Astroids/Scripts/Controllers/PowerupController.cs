using System;
using System.Collections;
using UnityEngine;

namespace Game.Astroids
{
    [SelectionBase]
    [RequireComponent(typeof(Rigidbody), typeof(Renderer))]
    public class PowerupController : GameMonoBehaviour
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

        Renderer Renderer
        {
            get
            {
                if (__renderer == null)
                    __renderer = GetComponent<Renderer>();

                return __renderer;
            }
        }
        Renderer __renderer;

        PowerupManager Powerup
        {
            get
            {
                if (__powerup == null)
                    __powerup = GameManager.m_powerupManager;

                return __powerup;
            }
        }
        PowerupManager __powerup;
        #endregion

        #region fields
        bool _isAlive;
        PowerupManager.Powerup _powerup;

        internal bool m_isVisible;
        #endregion

        void OnEnable()
        {
            _isAlive = true;
            Renderer.enabled = true;
            Renderer.material.SetFloat("_Dissolve", 0);

            RigidbodyUtil.SetRandomForce2D(Rb, 100f);
            RigidbodyUtil.SetRandomTorque(Rb, 250f);

            SetRandomPowerUp();
            StartCoroutine(Powerup.PlayDelayedAudio(PowerupSounds.Clip.Eject, clipsAudioSource, .1f));
            StartCoroutine(KeepAliveLoop());
        }

        void Update()
        {
            GameManager.ScreenWrapObject(gameObject);
        }

        void OnCollisionEnter(Collision other)
        {
            var c = other.collider;
            var o = other.gameObject;

            if (c.CompareTag("Enemy") || c.CompareTag("Player"))
                HitByShip(o);

            else if (c.CompareTag("Bullet"))
                HitByBullet(o);

            else if (c.CompareTag("AlienBullet"))
                HitByAlienBullet(o);
        }

        IEnumerator KeepAliveLoop()
        {
            float timePassed = 0;

            while (_isAlive && timePassed < Powerup.m_showTime)
            {
                timePassed += Time.deltaTime;

                yield return null;
            }
            DissolvePowerup();
        }

        void HitByShip(GameObject o)
        {
            _isAlive = false;

            o.TryGetComponent(out SpaceShipMonoBehaviour ship);
            if (ship != null)
            {
                Score(Powerup.GetPickupScore(ship.m_isEnemy));
                ship.ActivatePowerup(_powerup);
            }
            RemoveFromGame();
        }

        void HitByAlienBullet(GameObject alienBullet)
        {
            RemoveFromGame(alienBullet);
            Score(Powerup.GetDestructionScore(true));
            StartCoroutine(ExplodePowerup());
        }

        void HitByBullet(GameObject bullet)
        {
            RemoveFromGame(bullet);
            Score(Powerup.GetDestructionScore(false));
            StartCoroutine(ExplodePowerup());
        }

        void DissolvePowerup()
        {
            if (!_isAlive) return;

            _isAlive = false;

            LeanTween.value(0f, 1f, 2f)
                .setOnUpdate((float val) =>
                    {
                        Renderer.material.SetFloat("_Dissolve", val);
                    })
                .setOnComplete(RemoveFromGame)
                .setEaseInQuint();
        }

        IEnumerator ExplodePowerup()
        {
            _isAlive = false;
            Renderer.enabled = false;

            PlayEffect(EffectsManager.Effect.smallExplosion, transform.position, .5f);
            Powerup.PlayAudio(PowerupSounds.Clip.Explode, clipsAudioSource);

            while (clipsAudioSource.isPlaying)
                yield return null;

            RemoveFromGame();
        }


        void SetRandomPowerUp()
        {
            _powerup = RandomEnumUtil<PowerupManager.Powerup>.Get();
            //set material for shader
            //throw new NotImplementedException();
        }

        //public GameObject box;
        //public Material red;
        //private void OnTriggerEnter(Collider other)
        //{
        //    if (other.gameObject.tag == "Pickupable")
        //    {
        //        box.GetComponent<Renderer>().material = red;
        //    }
        //}

    }
}