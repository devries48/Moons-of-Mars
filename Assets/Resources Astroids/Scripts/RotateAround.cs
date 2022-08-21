using UnityEngine;

namespace Game.Astroids
{
    public class RotateAround : MonoBehaviour
    {
        [SerializeField]
        GameObject target;

        [SerializeField]
        Vector3 targetVector = new();

        [SerializeField]
        float degreesPerSecond = 20;

        void Update()
        {
            var vector = target != null ? target.transform.position : targetVector;
            transform.RotateAround(vector, Vector3.down, degreesPerSecond * Time.deltaTime);
        }
    }
}
