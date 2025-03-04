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
        
        Vector2 impulse = new Vector2(1000, 1000);

        Vector2 flippedContact = new Vector2(hitEvent.ContactPoint.X, -hitEvent.ContactPoint.Y);

        // Give slight upwards trajectory, no matter what for now
        flippedContact.Y -= 1;

        // Make sure that we just have a direction vector, strength should be determined by knockback 
        flippedContact.Normalize();

        flippedContact *= hitEvent.Knockback;
        impulse *= flippedContact;

        // Apply damage to the target
        ref var targetHealth = ref GetComponent<Damage>(hitEvent.Target);
        targetHealth.Value += (float)hitEvent.Damage;
        
        ref var targetVelocity = ref GetComponent<Velocity>(hitEvent.Target);

        System.Diagnostics.Debug.WriteLine("There was a hit!");

        targetVelocity.Value += impulse;
    }

    public override void Update(World world, GameTime gameTime) { }
}
