using ECS.Components.Timer;

namespace ECS.Events;

public struct TimerEvent : IEvent 
{
    public Entity Entity;
    public TimerType TimerType;
}