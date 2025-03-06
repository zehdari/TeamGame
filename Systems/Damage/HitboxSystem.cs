using ECS.Components.AI;
using ECS.Components.Collision;
using ECS.Components.Physics;
using ECS.Components.Projectiles;
using ECS.Components.State;
using ECS.Events;

namespace ECS.Systems.Hitbox;

public class HitboxSystem : SystemBase
{
    public override void Initialize(World world)
    {
        base.Initialize(world);
        Subscribe<CollisionEvent>(HandleCollisionEvent);
    }

    private bool isCollidingWithParent(Entity attacker, Entity target)
    {
        if (!HasComponents<ParentID>(attacker)) return false;

        ref var attackerParent = ref GetComponent<ParentID>(attacker);
        return attackerParent.Value == target.Id ? true : false;
    }

    private void HandleCollisionEvent(IEvent evt)
    {
        var collisionEvent = (CollisionEvent)evt;
        if (collisionEvent.EventType == CollisionEventType.End)
            return;

        var contact = collisionEvent.Contact;

        // Determine if this collision is between a hitbox and a hurtbox.
        bool isHitboxCollision = (contact.LayerA == CollisionLayer.Hitbox && contact.LayerB == CollisionLayer.Hurtbox) ||
                                    (contact.LayerA == CollisionLayer.Hurtbox && contact.LayerB == CollisionLayer.Hitbox);

        if (!isHitboxCollision)
            return;

        // Identify the attacker (hitbox) and the target (hurtbox)
        Entity attacker = (contact.LayerA == CollisionLayer.Hurtbox) ? contact.EntityA : contact.EntityB;
        Entity target = (contact.LayerA == CollisionLayer.Hitbox) ? contact.EntityA : contact.EntityB;

        // Stop early if current target already got hit
        ref var state = ref GetComponent<PlayerStateComponent>(target);
        if (state.CurrentState == PlayerState.Stunned)
            return;

        if (isCollidingWithParent(attacker, target)) return;

        ref var positionTarget = ref GetComponent<Position>(target);
        ref var positionAttacker = ref GetComponent<Position>(attacker);
        ref var attackerAttack = ref GetComponent<AttackInfo>(attacker);

        var currentAttack = attackerAttack.ActiveAttack;
        System.Diagnostics.Debug.WriteLine(currentAttack);

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
            ContactPoint = difference
        });

    }
    
    public override void Update(World world, GameTime gameTime) { }
}
