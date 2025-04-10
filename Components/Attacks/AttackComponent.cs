using ECS.Components.Collision;
using ECS.Systems.Attacking;

namespace ECS.Components.AI;

 public enum AttackType
{
    Special,
    Jab
}

public enum AttackDirection
{
    Up,
    Down,
    Left,
    Right
}

public struct AttackStats
{
    /* Some special attacks will have polygons, but this is mainly
     * for normal attacks. Special attacks will likely be handled specially
     * with delegates so we can have very unique behavior for each type of 
     * attack. 
     */
    public Polygon? Hitbox;

    public int Damage;
    public float Knockback;
    public float StunDuration;
}

public delegate void AttackHandler(Entity attacker);

public struct AttackInfo
{
    public AttackStats AttackStats;
    public AttackHandlerEnum AttackHandlerEnum;
}

public struct Attacks
{
    // All possible attacks, associated info and handler
    public Dictionary<AttackType, Dictionary<AttackDirection, AttackInfo>> AvailableAttacks;
    public AttackType LastType;
    public AttackDirection LastDirection;
}