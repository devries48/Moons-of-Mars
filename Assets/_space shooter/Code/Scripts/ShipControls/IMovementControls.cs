using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.SpaceShooter
{
    public interface IMovementControls
    {
        float Yaw { get; }
        float Pitch { get; }
        float Roll { get; }
        float Thrust { get; }
    }
}