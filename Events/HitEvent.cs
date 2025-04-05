namespace ECS.Events;

public struct HitEvent : IEvent
{
    public Entity Attacker;
    public Entity Target;
    public int Damage;
    public float Knockback;
    public float StunDuration;
    public Vector2 ContactPoint;
}