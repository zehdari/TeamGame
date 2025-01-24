namespace ECS.Events;

public struct GameExitEvent : IEvent 
{
    public Entity Entity;
}