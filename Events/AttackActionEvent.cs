using ECS.Components.AI;

namespace ECS.Events;

public struct AttackActionEvent : IEvent
{
    public AttackType AttackType;
    public AttackDirection Direction;
    public Entity Entity;
}