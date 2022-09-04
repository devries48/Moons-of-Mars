using UnityEngine;

namespace Game.Astroids
{
    public class Powerup : GameMonoBehaviour
    {
        [SerializeField, Range(5, 30)]
        int showTime = 10;

        [SerializeField, Range(5, 30)]
        int powerDuration = 10;

        [SerializeField, Range(0, 200)]
        int pickupScore = 20;

        [SerializeField, Range(-200, 0)]
        int enemyPickupScore = -50;

        [SerializeField]
        float spawnForce = 100f;

        [SerializeField]
        float spawnTorque = 100f;

        public bool m_isVisible;

        protected SpaceShipMonoBehaviour m_ship; // receiver of powerups

        void OnCollisionEnter(Collision collisionInfo)
        {
            if (collisionInfo.collider.CompareTag("SpaceShip"))
            {
                collisionInfo.gameObject.TryGetComponent(out m_ship);
                if (m_ship != null)
                {
                    Score(m_ship.m_isEnemy ? enemyPickupScore : pickupScore);
                    GrantPower();
                }

                RemoveFromGame();
            }
        }

        public virtual void GrantPower()
        {
            //TODO: add time if active
            CancelInvoke(nameof(DenyPower)); // Allows power "refresh" if got again.
            Invoke(nameof(DenyPower), powerDuration);
        }

        public virtual void DenyPower() { }

        public void ActivatePower(Vector3 pos)
        {
            Spawn(pos);

            InvokeRemoveFromGame(showTime);
        }

        void Spawn(Vector3 pos)
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb)
            {
                RigidbodyUtil.SetRandomForce(rb, spawnForce);
                RigidbodyUtil.SetRandomTorque(rb, spawnTorque);
            }
            transform.position = pos;
        }


    }
}