using System;
using UnityEngine;

namespace Game.SpaceShooter
{
    [RequireComponent(typeof(AudioSource))]
    public class BulletController : MonoBehaviour
    {
        //[SerializeField] Detonator _hitEffect;
        [SerializeField] AudioClip _impactSound;

        float _launchForce;
        float _damage;
        float _lifetime;

        Rigidbody _rigidBody;
        AudioSource _audioSource;

        void Awake()
        {
            _rigidBody = GetComponent<Rigidbody>();
            //_audioSource = SoundManager.Configure3DAudioSource(GetComponent<AudioSource>());
        }

        void OnEnable()
        {
            _rigidBody.AddForce(_launchForce * transform.forward);
            //AddForceAtAngle(_launchForce, -5f);
        }


        void OnDisable()
        {
            _rigidBody.velocity = Vector3.zero;
            _rigidBody.angularVelocity = Vector3.zero;
        }

        void Update()
        {
            _lifetime -= Time.deltaTime;

            if (_lifetime <= 0f)
                Destroy(gameObject);
        }

        public void Fire(float force, float lifetime, float damage, Rigidbody ship)
        {
            gameObject.SetActive(false);

            _launchForce = force;
            _damage = damage;
            _lifetime = lifetime;

            _rigidBody.velocity = ship.velocity;
            _rigidBody.angularVelocity = ship.angularVelocity;

            gameObject.SetActive(true);
        }


        void OnCollisionEnter(Collision collision)
        {
            print("collision enter");

            if (_impactSound) _audioSource.PlayOneShot(_impactSound);
            //IDamageable damageable = collision.collider.gameObject.GetComponent<IDamageable>();
            //if (damageable != null)
            //{
            //    Vector3 hitPosition = collision.GetContact(0).point;
            //    damageable.TakeDamage(_damage, hitPosition);
            //}

            //if (_hitEffect != null)
            //{
            //    Instantiate(_hitEffect, transform.position, Quaternion.identity);
            //}
            Destroy(gameObject);
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.name != gameObject.name)
                print("trigger enter: " + other.name);
        }

    }
}