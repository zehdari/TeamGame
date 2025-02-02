namespace ECS.Events;

public struct InputEvent : IEvent
{
    public string ActionName;
    public Entity Entity;
    public bool IsStarted;
    public bool IsEnded;
    public bool IsHeld;
}