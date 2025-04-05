namespace ECS.Events;

public struct PunchHitEvent : IEvent
{
    public Entity Attacker;
    public Entity Target;
}