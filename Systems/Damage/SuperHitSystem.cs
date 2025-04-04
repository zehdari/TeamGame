using ECS.Components.AI;
using ECS.Components.Collision;
using ECS.Components.Physics;
using ECS.Components.Projectiles;
using ECS.Components.State;
using ECS.Events;

namespace ECS.Systems.Hitbox;
/*
 * Super class for some hit management systems. Inheritance was a much cleaner 
 * alternative to what I was about to do.
 * 
 * This will hold some shared code and some helpful methods
 * for its children.
 */
public class SuperHitSystem : SystemBase
{
    public override void Initialize(World world)
    {
        base.Initialize(world);
    }

    protected void SendHitEvent(Entity attacker, Entity target)
    {
        // Stop early if current target already got hit
        ref var state = ref GetComponent<PlayerStateComponent>(target);
        if (state.CurrentState == PlayerState.Stunned)
            return;

        ref var positionTarget = ref GetComponent<Position>(target);
        ref var positionAttacker = ref GetComponent<Position>(attacker);
        ref var attackerAttack = ref GetComponent<AttackInfo>(attacker);

        var currentAttack = attackerAttack.ActiveAttack;

        // Get the current attack struct so we can access the damage, kb, etc
        var attack = attackerAttack.AvailableAttacks.First(attack => attack.Type.Equals(currentAttack));

        // Get the direction vector between attacker and target
        var difference = positionTarget.Value - positionAttacker.Value;
        difference.Normalize();

        Publish<HitEvent>(new HitEvent
        {
            Attacker = attacker,
            Target = target,
            Damage = attack.Damage,
            Knockback = attack.Knockback,
            StunDuration = attack.StunDuration,
            ContactPoint = difference
        });
    }

    protected bool isBlocking(Entity entity)
    {
        if (!HasComponents<PlayerStateComponent>(entity))
            return false;

        ref var state = ref GetComponent<PlayerStateComponent>(entity);
        return state.CurrentState == PlayerState.Block;
    }

    public override void Update(World world, GameTime gameTime) { }
}
