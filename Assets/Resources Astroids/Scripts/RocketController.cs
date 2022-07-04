using System.Collections;
using UnityEngine;

namespace Game.Astroids
{
    [SelectionBase]
    [RequireComponent(typeof(Rigidbody))]
    public class RocketController : MonoBehaviour
    {
        #region editor fields
        [Header("Spaceship")]

        [Header("Weapon & bullet")]
        [SerializeField, Tooltip("Select a weapon prefab")]
        GameObject weapon; 
        
        [SerializeField, Tooltip("Select a bullet prefab")] 
        GameObject bullet; 
       
        [SerializeField, Tooltip("Lifetime in seconds")] 
        float bulletLifetime = 10f; 
       
        [SerializeField, Tooltip("Fire rate in seconds")] 
        float fireRate = 0.5f; 
        
        [SerializeField] 
        float fireForce = 350f;
        #endregion

        float _thrust = 6f;
        float _rotationSpeed = 180f;
        float _maxSpeed = 4.5f; 
        Rigidbody _rb; 
        bool _canShoot = true;
        
        void Start()
        {
            _rb = GetComponent<Rigidbody>();
        }

        void FixedUpdate()
        {
            ControlRocket();
        }

        private void Update()
        {
            if (!_canShoot)
                return;
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
                StartCoroutine(Shoot());

            Gameplay.RePosition(gameObject);
        }

        private void ControlRocket()
        {
            transform.Rotate(0, 0, Input.GetAxis("Horizontal") * _rotationSpeed * Time.deltaTime);
 
            _rb.AddForce(Input.GetAxis("Vertical") * _thrust * transform.up);
            _rb.velocity = new Vector2(Mathf.Clamp(_rb.velocity.x, -_maxSpeed, _maxSpeed), Mathf.Clamp(_rb.velocity.y, -_maxSpeed, _maxSpeed));
        }

        public void ResetRocket()
        {
            transform.position = new Vector2(0f, 0f);
            transform.eulerAngles = new Vector3(0, 180f, 0);

            _rb.velocity = new Vector3(0f, 0f, 0f);
            _rb.angularVelocity = new Vector3(0f, 0f, 0f);
        }

        IEnumerator Shoot()
        {
            _canShoot = false;

            var bullet_obj = Instantiate(bullet, weapon.transform.position, weapon.transform.rotation) as GameObject;
            var bullet_rb = bullet_obj.GetComponent<Rigidbody>();
            
            bullet_rb.AddForce(transform.up * fireForce);
            Destroy(bullet_obj, bulletLifetime);
            yield return new WaitForSeconds(fireRate);
            
            _canShoot = true;
        }
    }
}
