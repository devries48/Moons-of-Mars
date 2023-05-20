using MoonsOfMars.Shared;
using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MoonsOfMars.Game.Asteroids
{
    [SuppressMessage("", "IDE0051", Justification = "Methods used by Player Input SendMessages")]
    public class InputManager : InputManagerBase
    {
        public event Action OnHyperJumpPressed;
        public event Action OnPausePressed;

        public float TurnInput { get; private set; }
        public bool IsShooting { get; private set; }
        public float Thrust { get; private set; }
        public Vector2 MoveCursorInput { get; private set; }

        void OnTurn(InputValue value) => TurnInput = value.Get<float>();

        void OnThrust(InputValue value) => Thrust = value.isPressed ? 1 : 0;

        void OnMoveCursor(InputValue value) => MoveCursorInput = value.Get<Vector2>();

        void OnFire(InputValue value) => IsShooting = value.isPressed;

        void OnHyperJump() => OnHyperJumpPressed?.Invoke();

        void OnPause() => OnPausePressed?.Invoke();
    }
}