namespace Game.SpaceShooter
{
    public abstract class MovementControlsBase : IMovementControls
    {
        public abstract float Yaw { get; }
        public abstract float Pitch { get; }
        public abstract float Roll { get; }
        public abstract float Thrust { get; }
    }
}