using ECS.Components.Collision;

namespace ECS.Events;

public enum CollisionEventType
{
    Begin,
    Stay,
    End
}

public struct CollisionEvent : IEvent 
{
    public Contact Contact;
    public CollisionEventType EventType;
}