namespace ECS.Events;

public struct PvZDeathEvent : IEvent
{
    public Entity Entity;
    public Entity Grid;
}