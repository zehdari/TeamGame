using ECS.Components.AI;
using ECS.Components.Collision;
using ECS.Components.Physics;
using ECS.Components.Projectiles;
using ECS.Components.State;
using ECS.Events;

namespace ECS.Systems.Hitbox;

public class AttackHitSystem : SystemBase
{
    public override void Initialize(World world)
    {
        base.Initialize(world);
        Subscribe<PunchHitEvent>(HandleHitEvent);
    }

    private void HandleHitEvent(IEvent evt)
    {
        PunchHitEvent punchHitEvent = (PunchHitEvent)evt;

        /* 
         * Lots of repeated code between ProjectileHitSystem and this, maybe pull out and make 
         * another event to handle this? Might be a little bit of an abuse of events though.
         */

        // Stop early if current target already got hit
        ref var state = ref GetComponent<PlayerStateComponent>(punchHitEvent.Target);
        if (state.CurrentState == PlayerState.Stunned)
            return;

        ref var positionTarget = ref GetComponent<Position>(punchHitEvent.Target);
        ref var positionAttacker = ref GetComponent<Position>(punchHitEvent.Attacker);
        ref var attackerAttack = ref GetComponent<AttackInfo>(punchHitEvent.Attacker);

        var currentAttack = attackerAttack.ActiveAttack;

        // Get the current attack struct so we can access the damage, kb, etc
        var attack = attackerAttack.AvailableAttacks.First(attack => attack.Type.Equals(currentAttack));

        // Get the direction vector between attacker and target
        var difference = positionTarget.Value - positionAttacker.Value;
        difference.Normalize();

        Publish<HitEvent>(new HitEvent
        {
            Attacker = punchHitEvent.Attacker,
            Target = punchHitEvent.Target,
            Damage = attack.Damage,
            Knockback = attack.Knockback,
            StunDuration = attack.StunDuration,
            ContactPoint = difference
        });

    }

    public override void Update(World world, GameTime gameTime) { }
}
