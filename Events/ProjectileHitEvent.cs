namespace ECS.Events;

public struct ProjectileHitEvent : IEvent
{
    public Entity Attacker;
    public Entity Target;
}