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

        // Stop early if current target is blocking, no damage should be applied
        if (isBlocking(punchHitEvent.Target))
            return;

        base.SendHitEvent(punchHitEvent.Attacker, punchHitEvent.Target);

    }

    public override void Update(World world, GameTime gameTime) { }
}
