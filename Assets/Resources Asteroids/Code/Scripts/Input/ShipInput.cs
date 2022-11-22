using UnityEngine;

namespace Game.Astroids
{
    public static class ShipInput
    {
        public static bool IsShooting()
        {
            return Input.GetButton("Fire1");
        }

        public static bool IsHyperspacing()
        {
            return Input.GetButtonDown("Jump");
        }

        public static float GetTurnAxis()
        {
            return Input.GetAxis("Horizontal");
        }

        public static float GetForwardThrust()
        {
            float axis = Input.GetAxis("Vertical");
            return Mathf.Clamp01(axis);
        }
    }
}