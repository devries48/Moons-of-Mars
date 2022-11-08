using UnityEngine;

namespace Game.Astroids
{
    public class RotateSkybox : MonoBehaviour
    {
        [SerializeField] float speed;

        void Update()
        {
            RenderSettings.skybox.SetFloat("_Rotation", speed * Time.time);
        }
    }
}