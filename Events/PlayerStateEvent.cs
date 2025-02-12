using ECS.Components.State;

namespace ECS.Events;


public struct PlayerStateEvent : IEvent
{
    public Entity Entity;
    public PlayerState RequestedState;
}