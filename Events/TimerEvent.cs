using ECS.Components.Timer;

namespace ECS.Events;

public struct UpdateTimerEvent : IEvent 
{
    public Entity Entity;
    public TimerType Type;
    public Timer Timer;
}