using ECS.Components.State;
using ECS.Components.Animation;
using ECS.Components.Collision;
using ECS.Components.Physics;

namespace ECS.Systems.Attacking;

public class HitSystem : SystemBase
{
    public override void Initialize(World world)
    {
        base.Initialize(world);
        Subscribe<CollisionEvent>(HandleHit);
    }

    private void HandleHit(IEvent evt)
    {
        var collisionEvent = (CollisionEvent)evt;
        if (collisionEvent.EventType == CollisionEventType.End)
            return;

        var contact = collisionEvent.Contact;

        // If we have a hitboxes or hurtboxes, we're good. DeMorgan's on CollisionResponseSystem check
        if (!(contact.LayerA == CollisionLayer.Hitbox || contact.LayerA == CollisionLayer.Hurtbox) &&
            !(contact.LayerB == CollisionLayer.Hitbox || contact.LayerB == CollisionLayer.Hurtbox))
        {
            return;
        }

        Entity entityA = contact.EntityA;
        Entity entityB = contact.EntityB;

        // Who was the hitbox and who was the hurtbox?
        

    }

    public override void Update(World world, GameTime gameTime) { }
}
