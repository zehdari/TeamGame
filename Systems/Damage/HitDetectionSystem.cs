using ECS.Components.AI;
using ECS.Components.Collision;
using ECS.Components.Physics;
using ECS.Components.Projectiles;
using ECS.Components.State;
using ECS.Components.Tags;
using ECS.Events;

namespace ECS.Systems.Hitbox;
/*
 * This system should delegate out specific hit events to other systems that
 * make decisions and send out generic hitEvents.
 */
public class HitDetectionSystem : SystemBase
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

        // Identify the attacker (hitbox) and the target (hurtbox)
        Entity attacker = (contact.LayerA == CollisionLayer.Hurtbox) ? contact.EntityA : contact.EntityB;
        Entity target = (contact.LayerA == CollisionLayer.Hitbox) ? contact.EntityA : contact.EntityB;

        // Differentiate between a projectile to player collision and a player to player collision
        if(HasComponents<ProjectileTag>(attacker))
        {
            Publish<ProjectileHitEvent>(new ProjectileHitEvent
            {
                Attacker = attacker,
                Target = target,
            });

        } else
        {
            Publish<PunchHitEvent>(new PunchHitEvent
            {
                Attacker = attacker,
                Target = target,
            });
        }

    }
    
    public override void Update(World world, GameTime gameTime) { }
}
