using ECS.Components.State;
using ECS.Components.Animation;
using ECS.Components.Physics;
using ECS.Components.Tags;
using ECS.Events;

namespace ECS.Systems.Attacking;

public class HitSystem : SystemBase
{
    public override void Initialize(World world)
    {
        base.Initialize(world);
        Subscribe<HitEvent>(HandleHit);
    }

    private void HandleProjectileHit(HitEvent hitEvent)
    {
        // Despawn the projectile
        Publish<DespawnEvent>(new DespawnEvent
        {
            Entity = hitEvent.Attacker
        });

        // Right now, this is specifically for peas, as we don't have any other projectiles. 
        // This will need to change.
        ref var attackerPosition = ref GetComponent<Position>(hitEvent.Target);
        Publish<ProjectileHitEvent>(new ProjectileHitEvent
        {
            type = "splat_pea",
            hitPoint = attackerPosition.Value,
            World = World
        });
    }

    private void DealWithHitPhysics(HitEvent hitEvent)
    {

        ref var percent = ref GetComponent<Percent>(hitEvent.Target);

        // Increment percent by the amount of damage that an attack did
        percent.Value += hitEvent.Damage;

        Vector2 impulse = new Vector2(1000, 1000);

        Vector2 flippedContact = new Vector2(hitEvent.ContactPoint.X, -hitEvent.ContactPoint.Y);

        // Give slight upwards trajectory, no matter what for now
        flippedContact.Y -= 1;

        // Make sure that we just have a direction vector, strength should be determined by knockback 
        flippedContact.Normalize();

        // Get the correct strength of the hit
        flippedContact *= hitEvent.Knockback;
        flippedContact *= (percent.Value / 10000);

        impulse *= flippedContact;

        // Apply damage to the target
        ref var targetHealth = ref GetComponent<Percent>(hitEvent.Target);
        targetHealth.Value += (float)hitEvent.Damage;

        ref var targetVelocity = ref GetComponent<Velocity>(hitEvent.Target);

        System.Diagnostics.Debug.WriteLine("There was a hit!");

        targetVelocity.Value += impulse;
    }

    private void HandleHit(IEvent evt)
    {
        var hitEvent = (HitEvent)evt;

        ref var state = ref GetComponent<PlayerStateComponent>(hitEvent.Target);

        if (state.CurrentState == PlayerState.Stunned)
            return;

        if(HasComponents<ProjectileTag>(hitEvent.Attacker)) HandleProjectileHit(hitEvent);

        DealWithHitPhysics(hitEvent);

        float totalDuration = 0.5f;

        Publish(new PlayerStateEvent
        {
            Entity = hitEvent.Target,
            RequestedState = PlayerState.Stunned,
            Force = false, // Force is true to ensure a new attack starts if not already attacking
            Duration = totalDuration
        });

    }

    public override void Update(World world, GameTime gameTime) { }
}
