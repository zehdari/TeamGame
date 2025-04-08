using ECS.Components.Collision;

namespace ECS.Components.AI;

public delegate void AttackHandler(AttackStats stats);

 public enum AttackType
{
    Special,
    Normal
}

public enum AttackDirection
{
    Up,
    Down,
    Left,
    Right
}

public struct Attack
{
    AttackType Type;
    AttackDirection Direction;
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

public struct AttackInfo
{
    // All possible attacks, and their assigned handlers.
    public Dictionary<Attack, (AttackStats, AttackHandler)> AvailableAttacks;
    // Index or type of the currently active attack.
    public Attack ActiveAttack;
}