using UnityEngine;

namespace Game.Astroids
{
    public class RotateAround : MonoBehaviour
    {
        [SerializeField] GameObject target;
        [SerializeField] Vector3 targetVector = new();
        [SerializeField] float degreesPerSecond = 20;
        [SerializeField] Vector3 pivot = Vector3.down;

        Vector3 TargetVector => target != null ? target.transform.position : targetVector;

        void Update() => transform.RotateAround(TargetVector, pivot, degreesPerSecond * Time.deltaTime);
    }
}
