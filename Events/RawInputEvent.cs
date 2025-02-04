namespace ECS.Events;

public struct RawInputEvent : IEvent
{
    public Entity Entity;
    public Keys RawKey;
    public bool IsPressed;
}