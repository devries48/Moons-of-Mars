using UnityEngine;

namespace MoonsOfMars.Game.Asteroids
{
    public static class ShipInput
    {
        public static bool IsShooting() => Input.GetButton("Fire1");

        public static bool IsHyperspacing() => Input.GetButtonDown("Jump");

        public static float GetTurnAxis() => Input.GetAxis("Horizontal");

        public static float GetForwardThrust()
        {
            float axis = Input.GetAxis("Vertical");
            return Mathf.Clamp01(axis);
        }

        public static bool IsPauseGame() => Input.GetKeyDown(KeyCode.P);
    }
}