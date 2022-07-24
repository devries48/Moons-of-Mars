using UnityEngine;

namespace Game.Astroids
{
    public class PlayerShipController : SpaceShipMonoBehaviour
    {
        public bool IsAlive => gameObject.activeSelf;

        bool _canMove = true;
        float _thrust = 6f;
        float _rotationSpeed = 180f;
        float _maxSpeed = 4.5f;

        void FixedUpdate()
        {
            if (_canMove)
                ControlRocket();
        }

        void Update()
        {
            AstroidsGameManager.Instance.ScreenWrapObject(gameObject);

            if (!m_canShoot)
                return;

            if (Input.GetMouseButtonDown(0) || Input.GetKey(KeyCode.Space))
                FireWeapon();
        }

        public void Recover()
        {
            if (!IsAlive)
            {
                ResetTransform();
                gameObject.SetActive(true);
                ResetRigidbody();
            }
        }

        public void EnableControls()
        {
            _canMove = true;
            m_canShoot = true;
        }

        public void DisableControls()
        {
            _canMove = false;
            m_canShoot = false;
        }

        void ControlRocket()
        {
            transform.Rotate(0, 0, Input.GetAxis("Horizontal") * _rotationSpeed * Time.deltaTime);

            Rb.AddForce(Input.GetAxis("Vertical") * _thrust * transform.up);
            Rb.velocity = new Vector2(Mathf.Clamp(Rb.velocity.x, -_maxSpeed, _maxSpeed), Mathf.Clamp(Rb.velocity.y, -_maxSpeed, _maxSpeed));
        }

        void ResetTransform() => transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

        void ResetRigidbody() => RigidbodyUtil.Reset(GetComponent<Rigidbody>());

    }
}
