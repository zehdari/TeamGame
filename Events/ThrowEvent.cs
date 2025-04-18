using ECS.Core;
namespace ECS.Events;

public struct ThrowEvent : IEvent
{
    public Entity Thrower;

    public ThrowEvent(Entity thrower)
    {
        Thrower = thrower;
    }
}

