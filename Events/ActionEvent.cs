namespace ECS.Events;

public struct InputEvent : IEvent
{
    public string ActionName;
    public Entity Entity;
    public bool IsStared;
    public bool IsEnded;
    public bool IsHeld;
}