namespace ECS.Events;

public struct ActionEvent : IEvent
{
    public string ActionName;
    public Entity Entity;
    public bool IsStarted;
    public bool IsEnded;
    public bool IsHeld;
}