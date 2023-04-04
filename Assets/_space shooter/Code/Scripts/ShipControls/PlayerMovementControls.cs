using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.SpaceShooter
{
    [Serializable]
    public class PlayerMovementControls : MovementControlsBase
    {
        PlayerInput _playerInput;

        public override float Yaw => throw new NotImplementedException();

        public override float Pitch => throw new NotImplementedException();

        public override float Roll => throw new NotImplementedException();

        public override float Thrust => throw new NotImplementedException();

    }
}