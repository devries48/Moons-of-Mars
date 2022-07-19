using UnityEngine;

namespace Game.Astroids
{
    public class RocketController : BaseSpaceShipController
    {
        float _thrust = 6f;
        float _rotationSpeed = 180f;
        float _maxSpeed = 4.5f; 
        
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

            AstroidsGameManager.Instance.RePosition(gameObject);
        }

        private void ControlRocket()
        {
            transform.Rotate(0, 0, Input.GetAxis("Horizontal") * _rotationSpeed * Time.deltaTime);
 
            Rb.AddForce(Input.GetAxis("Vertical") * _thrust * transform.up);
            Rb.velocity = new Vector2(Mathf.Clamp(Rb.velocity.x, -_maxSpeed, _maxSpeed), Mathf.Clamp(Rb.velocity.y, -_maxSpeed, _maxSpeed));
        }
    }
}
