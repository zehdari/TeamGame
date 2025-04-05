using ECS.Components.Collision;

namespace ECS.Components.Physics;

public struct IsGrounded
{
    public bool Value;
    public bool WasGrounded;
    public float UngroundedTimer;
    public Vector2 GroundNormal;
}