namespace ECS.Events;

public struct CollisionEvent : IEvent
{
    public Entity EntityA;
    public Entity EntityB;
    public Vector2 Normal;         // Direction of collision
    public float Penetration;      // Overlap of shapes
    public CollisionFlags Sides;
}