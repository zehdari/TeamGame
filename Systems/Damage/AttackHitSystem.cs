using ECS.Components.AI;
using ECS.Components.Collision;
using ECS.Components.Physics;
using ECS.Components.Projectiles;
using ECS.Components.State;
using ECS.Events;

namespace ECS.Systems.Hitbox;

public class AttackHitSystem : SuperHitSystem
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

        // Stop early if current target is blocking, no damage should be applied
        if (isBlocking(punchHitEvent.Target))
            return;

        SendHitEvent(punchHitEvent.Attacker, punchHitEvent.Target);

    }

    public override void Update(World world, GameTime gameTime) { }
}
