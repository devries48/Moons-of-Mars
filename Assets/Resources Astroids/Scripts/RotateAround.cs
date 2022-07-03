using UnityEngine;

namespace Game.Astroids
{
    public class RotateAround : MonoBehaviour
    {
        [SerializeField]
        GameObject target;

        [SerializeField]
        float degreesPerSecond = 20;

        void Update()
        {
            transform.RotateAround(target.transform.position, Vector3.up, degreesPerSecond * Time.deltaTime);
        }
    }
}
