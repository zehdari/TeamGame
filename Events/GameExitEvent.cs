namespace ECSAttempt.Events;

public struct GameExitEvent : IEvent 
{
    public Entity Entity;
}