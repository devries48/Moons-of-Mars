using UnityEngine;

namespace Game.Astroids
{
    public class RotateAround : MonoBehaviour
    {
        [SerializeField] GameObject target;
        [SerializeField]Vector3 targetVector = new();
        [SerializeField]float degreesPerSecond;
        [SerializeField] Vector3 pivot = Vector3.down;


        void Update()
        {
            var vector = target != null ? target.transform.position : targetVector;
            transform.RotateAround(vector, pivot, degreesPerSecond * Time.deltaTime);
        }
    }
}
