using ECS.Components.State;
using ECS.Components.Animation;
using ECS.Components.Physics;

namespace ECS.Systems.Attacking;

public class HitSystem : SystemBase
{
    public override void Initialize(World world)
    {
        base.Initialize(world);
        Subscribe<HitEvent>(HandleHit);
    }

    private void HandleHit(IEvent evt)
    {
        var hitEvent = (HitEvent)evt;

        Vector2 impulse = new Vector2(100, -100);
        impulse *= hitEvent.Knockback;

        ref var targetVelocity = ref GetComponent<Velocity>(hitEvent.Target);

        System.Diagnostics.Debug.WriteLine("There was a hit!");

        targetVelocity.Value += impulse;
    }

    public override void Update(World world, GameTime gameTime) { }
}
