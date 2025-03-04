using ECS.Components.AI;
using ECS.Components.Collision;
using ECS.Components.Physics;

namespace ECS.Systems.Hitbox;

public class HitboxSystem : SystemBase
{
    public override void Initialize(World world)
    {
        base.Initialize(world);
        Subscribe<CollisionEvent>(HandleCollisionEvent);
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

        System.Diagnostics.Debug.WriteLine("after isHitboxCollision check");

        // Identify the attacker (hitbox) and the target (hurtbox)
        Entity attacker = (contact.LayerA == CollisionLayer.Hurtbox) ? contact.EntityA : contact.EntityB;
        Entity target = (contact.LayerA == CollisionLayer.Hitbox) ? contact.EntityA : contact.EntityB;

        ref var positionTarget = ref GetComponent<Position>(target);
        ref var positionAttacker = ref GetComponent<Position>(attacker);
        ref var attackerAttack = ref GetComponent<AttackInfo>(attacker);

        var currentAttack = attackerAttack.ActiveAttack;
        System.Diagnostics.Debug.WriteLine(currentAttack);
        
        // Debug loop to see what's actually in my loop
        foreach(var thing in attackerAttack.AvailableAttacks)
        {
            System.Diagnostics.Debug.WriteLine("The types are ");
            System.Diagnostics.Debug.WriteLine(thing.Type);
            System.Diagnostics.Debug.WriteLine(thing.Damage);
            System.Diagnostics.Debug.WriteLine(thing.Knockback);
        }

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
