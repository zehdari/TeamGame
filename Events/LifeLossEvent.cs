namespace ECS.Events;

public struct LifeLossEvent : IEvent
{
    public Entity Entity;
}