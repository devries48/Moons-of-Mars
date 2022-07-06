using System.Collections;
using UnityEngine;

namespace Game.Astroids
{
    [SelectionBase]
    [RequireComponent(typeof(Rigidbody))]
    public class RocketController : BaseSpaceShipController
    {
        float _thrust = 6f;
        float _rotationSpeed = 180f;
        float _maxSpeed = 4.5f; 
        
        void Start()
        {
            m_rb = GetComponent<Rigidbody>();
        }

        void FixedUpdate()
        {
            ControlRocket();
        }

        private void Update()
        {
            if (!m_canShoot)
                return;

            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
                StartCoroutine(Shoot());

            Gameplay.RePosition(gameObject);
        }

        private void ControlRocket()
        {
            transform.Rotate(0, 0, Input.GetAxis("Horizontal") * _rotationSpeed * Time.deltaTime);
 
            m_rb.AddForce(Input.GetAxis("Vertical") * _thrust * transform.up);
            m_rb.velocity = new Vector2(Mathf.Clamp(m_rb.velocity.x, -_maxSpeed, _maxSpeed), Mathf.Clamp(m_rb.velocity.y, -_maxSpeed, _maxSpeed));
        }
    }
}
