using UnityEngine;

namespace Game.Astroids
{
    public class UfoController : SpaceShipMonoBehaviour
    {
        [Header("UFO")]
        [SerializeField]
        float rotationSpeed = 50f;

        void FixedUpdate()
        {
            transform.Rotate(new Vector3(0, rotationSpeed * Time.fixedDeltaTime, 0));
        }
    }
}