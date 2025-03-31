namespace ECS.Events;

public struct HitEvent : IEvent
{
    public Entity Attacker;
    public Entity Target;
    public int Damage;
    public float Knockback;
    public Vector2 ContactPoint;
}