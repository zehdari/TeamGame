namespace ECS.Components.Map;

public struct PlatformDirectionState
{
    public bool WasMovingUp;              // Direction in the previous frame
    public bool IsMovingUp;               // Current direction
    public bool JustChangedDirection;     // Flag for when direction changes
    public int DirectionChangeFrames;     // Counter for frames since direction change
    public float LastVelocityY;           // Previous frame's Y velocity
}