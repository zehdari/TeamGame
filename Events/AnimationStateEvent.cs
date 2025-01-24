namespace ECS.Events;

public struct AnimationStateEvent : IEvent
{
    public string NewState;
    public Entity Entity;
}