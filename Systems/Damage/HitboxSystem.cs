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

        // Components could be using something like
        // public enum AttackType
        // {
        //     None,
        //     Light,
        //     Heavy,
        //     Special
        // }

        // public struct AttackData
        // {
        //     public AttackType Type;
        //     public int Damage;
        //     public float Knockback;
        // }

        // public struct AttackComponent
        // {
        //     // All possible attacks.
        //     public List<AttackData> AvailableAttacks;   
        //     // Index or type of the currently active attack.
        //     public AttackType ActiveAttack;
        // }

        ref var positionTarget = ref GetComponent<Position>(target);
        ref var positionAttacker = ref GetComponent<Position>(attacker);

        // Get the direction vector between attacker and target
        var difference = positionTarget.Value - positionAttacker.Value;
        difference.Normalize();

        Publish<HitEvent>(new HitEvent
        {
            Attacker = attacker,
            Target = target,
            Damage = 10,
            Knockback = 10f,
            ContactPoint = difference
        });

        System.Diagnostics.Debug.WriteLine("published the event");
    }
    
    public override void Update(World world, GameTime gameTime) { }
}
