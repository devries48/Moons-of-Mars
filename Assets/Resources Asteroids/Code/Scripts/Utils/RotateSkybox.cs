using UnityEngine;

namespace Game.Asteroids
{
    public class RotateSkybox : MonoBehaviour
    {
        [SerializeField] float speed;

        void Update() => RenderSettings.skybox.SetFloat("_Rotation", speed * Time.time);
    }
}