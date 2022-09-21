using System.Collections;
using UnityEngine;

namespace Game.Astroids
{
    [SelectionBase]
    [RequireComponent(typeof(Rigidbody), typeof(Renderer))]
    public class PowerupController : GameMonoBehaviour
    {
        #region editor fields

        [SerializeField, Range(5, 30)] int showTime = 10;
        [SerializeField, Range(5, 30)] int powerDuration = 10;

        [Header("Score")]
        [SerializeField, Range(0, 200)] int pickupScore = 20;
        [SerializeField, Range(-200, 0)] int enemyPickupScore = -50;

        [Header("Audio")]
        [SerializeField] AudioSource clipsAudioSource;
        [SerializeField] AudioClip ejectedAudioClip;
        [SerializeField] AudioClip explodeClip;
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

        #endregion

        #region fields

        protected SpaceShipMonoBehaviour m_ship; // receiver of powerups

        public bool m_isVisible;

        #endregion

        void OnEnable()
        {
            RigidbodyUtil.SetRandomForce2D(Rb, 100f);
            RigidbodyUtil.SetRandomTorque(Rb, 250f);

            SetRandomPowerUp();
            InvokeRemoveFromGame(showTime);
            StartCoroutine(PlayAudio(ejectedAudioClip));
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
            {
                o.TryGetComponent(out m_ship);
                if (m_ship != null)
                {
                    Score(m_ship.m_isEnemy ? enemyPickupScore : pickupScore);
                    GrantPower();
                }
                RemoveFromGame(); // dissolve
            }
            else if (c.CompareTag("Bullet"))
                HitByBullet(o);

            else if (c.CompareTag("AlienBullet"))
                HitByAlienBullet(o);

            else if (c.CompareTag("Astroid"))
                HitByAstroid(o);

        }


        public virtual void GrantPower()
        {
            CancelInvoke(nameof(DenyPower)); // Allows power "refresh" if got again.
            Invoke(nameof(DenyPower), powerDuration);
        }

        public virtual void DenyPower() { }

        void HitByAstroid(GameObject o)
        {
            print("Power hit by astroid");
        }

        void HitByAlienBullet(GameObject alienBullet)
        {
            RemoveFromGame(alienBullet);
            StartCoroutine(ExplodePowerup());
            print("Power hit by alien bullet");
        }

        void HitByBullet(GameObject bullet)
        {
            RemoveFromGame(bullet);
            StartCoroutine(ExplodePowerup());

            print("Power hit by bullet");
        }

        IEnumerator PlayAudio(AudioClip clip)
        {
            yield return new WaitForSeconds(.1f);

            if (clipsAudioSource && clip)
                PlaySound(clip, clipsAudioSource);
        }

        IEnumerator ExplodePowerup()
        {
            Renderer.enabled = false;

            PlayEffect(EffectsManager.Effect.smallExplosion, transform.position, .5f);
            PlaySound(explodeClip, clipsAudioSource);

            while (Audio.isPlaying)
                yield return null;

            RemoveFromGame();
        }


        void SetRandomPowerUp()
        {
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