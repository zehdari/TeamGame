namespace ECS.Components;

[Flags]              // Allows combining of multiple flags (i.e. 1011)
public enum CollisionFlags
{
    None = 0,        // 0000
    Top = 1,         // 0001
    Bottom = 2,      // 0010
    Left = 4,        // 0100
    Right = 8        // 1000
}

public struct CollisionState
{
    public CollisionFlags Sides;
    public HashSet<Entity> CollidingWith;
}