using ECS.Core;
namespace ECS.Events;

public struct GrabCandidateEvent : IEvent
{
    public Entity Grabber;
    public Entity Target;

    public GrabCandidateEvent(Entity grabber, Entity target)
    {
        Grabber = grabber;
        Target = target;
    }
}

