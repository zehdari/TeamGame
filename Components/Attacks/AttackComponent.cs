namespace ECS.Components.AI;

 public enum AttackType
{
    None,
    Light,
    Heavy,
    Special
}

public struct Attack
{
    public AttackType Type;
    public int Damage;
    public float Knockback;
}

public struct AttackInfo
{
    // All possible attacks.
    public List<Attack> AvailableAttacks;
    // Index or type of the currently active attack.
    public AttackType ActiveAttack;
}