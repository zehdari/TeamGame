using ECS.Components.Collision;

namespace ECS.Components.AI;

public struct AssociatedHitbox
{
    public Polygon box;
    public AttackType type;
}

public struct Hitboxes
{
    public List<AssociatedHitbox> availableHitboxes;
}