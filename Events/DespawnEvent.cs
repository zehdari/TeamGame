namespace ECS.Events;

public struct DespawnEvent : IEvent
{
    public Entity Entity;
}