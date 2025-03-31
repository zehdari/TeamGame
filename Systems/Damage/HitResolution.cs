using ECS.Components.State;
using ECS.Components.Animation;
using ECS.Components.Physics;
using ECS.Components.Tags;
using ECS.Events;

namespace ECS.Systems.Damage;

public class HitResolutionSystem : SystemBase
{
    public override void Initialize(World world)
    {
        base.Initialize(world);
        Subscribe<HitEvent>(HandleHit);
    }

    private void StunTarget(HitEvent hitEvent)
    {
        Publish(new PlayerStateEvent
        {
            Entity = hitEvent.Target,
            RequestedState = PlayerState.Stunned,
            Force = false, // Force is true to ensure a new attack starts if not already attacking
            Duration = hitEvent.StunDuration
        });
    }

    private void DealWithDamage(HitEvent hitEvent)
    {
        ref var percent = ref GetComponent<Percent>(hitEvent.Target);

        // Apply damage to the target
        percent.Value += hitEvent.Damage;

    }

    private void DealWithHitPhysics(HitEvent hitEvent)
    {
        const int KB_STRENGTH = 100_000;
        const int PERCENT_SCALAR = 10_000;

        ref var percent = ref GetComponent<Percent>(hitEvent.Target);
        Vector2 impulse = new Vector2(KB_STRENGTH, KB_STRENGTH);

        Vector2 flippedContact = new Vector2(hitEvent.ContactPoint.X, -hitEvent.ContactPoint.Y);

        // Give slight upwards trajectory, no matter what for now
        flippedContact.Y -= 1;

        // Make sure that we just have a direction vector, strength should be determined by knockback 
        flippedContact.Normalize();

        // Get the correct strength of the hit
        flippedContact *= hitEvent.Knockback;
        flippedContact *= percent.Value / PERCENT_SCALAR;
        impulse *= flippedContact;

        // Apply KB as a force, let physics handle the rest
        ref var targetForce = ref GetComponent<Force>(hitEvent.Target);
        targetForce.Value += impulse;
    }

    private void HandleHit(IEvent evt)
    {
        var hitEvent = (HitEvent)evt;

        /* Note: Damage should go first here */
        DealWithDamage(hitEvent);
        DealWithHitPhysics(hitEvent);
        StunTarget(hitEvent);

    }

    public override void Update(World world, GameTime gameTime) { }
}
