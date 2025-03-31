namespace ECS.Events;

public struct ProjectileHitEvent : IEvent
{
    public string type; // What is spawning
    public Vector2 hitPoint;
    public World World; // TODO this needs to die, not sure how to get around it yet
}