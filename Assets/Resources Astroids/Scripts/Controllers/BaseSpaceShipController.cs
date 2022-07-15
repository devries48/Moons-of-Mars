using System.Collections;
using UnityEngine;

namespace Game.Astroids
{
    [SelectionBase]
    [RequireComponent(typeof(Rigidbody))]
    public class BaseSpaceShipController : GameMonoBehaviour
    {
        #region editor fields
        [Header("Spaceship")]

        [SerializeField, Tooltip("Select shield gameobject")]
        protected ShieldBehaviour shield;

        [Header("Weapon & bullet")]
        [SerializeField, Tooltip("Select a weapon gameobject")]
        protected GameObject weapon;

        [SerializeField, Tooltip("Select a bullet prefab")]
        protected GameObject bullet;

        [SerializeField, Tooltip("Lifetime in seconds")]
        protected float bulletLifetime = 10f;

        [SerializeField, Tooltip("Fire rate in seconds")]
        protected float fireRate = 0.5f;

        [SerializeField]
        protected float fireForce = 350f;

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

        #endregion

        protected bool m_canShoot = true;

        protected IEnumerator Shoot()
        {
            m_canShoot = false;

            var bullet_obj = Instantiate(bullet, weapon.transform.position, weapon.transform.rotation) as GameObject;
            var bullet_rb = bullet_obj.GetComponent<Rigidbody>();
            
            PlayAudioClip(SpaceShipSounds.Clip.ShootBullet);
            bullet_rb.AddForce(transform.up * fireForce);

            Destroy(bullet_obj, bulletLifetime);
            yield return new WaitForSeconds(fireRate);

            m_canShoot = true;
        }

        public void PlayAudioClip(SpaceShipSounds.Clip clip)
        {
            var sound = sounds.GetClip(clip);

            PlaySound(sound);
        }

        public void ResetRocket()
        {
            transform.position = new Vector2(0f, 0f);
            transform.eulerAngles = new Vector3(0, 180f, 0);

            Rb.velocity = new Vector3(0f, 0f, 0f);
            Rb.angularVelocity = new Vector3(0f, 0f, 0f);
        }

    }
}