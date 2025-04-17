using ECS.Components.Grab;
namespace ECS.Events;

public struct GrabEvent : IEvent
{
    public Entity Grabber;
    public Entity Target;

    public GrabEvent(Entity grabber, Entity target)
    {
        Grabber = grabber;
        Target = target;
    }
}
