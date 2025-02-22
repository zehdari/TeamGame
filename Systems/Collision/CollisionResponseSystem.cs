using ECS.Components.Collision;
using ECS.Components.Physics;

namespace ECS.Systems.Collision;

/// <summary>
/// Handles physical responses to collisions between entities using impulse-based resolution.
/// Implements position correction and velocity response for realistic collision behavior.
/// </summary>
public class CollisionResponseSystem : SystemBase
{
    /// <summary>
    /// Initializes the collision response system and subscribes to collision events.
    /// </summary>
    /// <param name="world">The game world containing all entities.</param>
    public override void Initialize(World world)
    {
        base.Initialize(world);
        Subscribe<CollisionEvent>(HandleCollision);
    }

    /// <summary>
    /// Handles collision events by calculating and applying appropriate physical responses.
    /// Implements impulse-based collision response with position correction.
    /// </summary>
    /// <param name="evt">The collision event to handle.</param>
    /// <remarks>
    /// This method:
    /// - Filters out trigger collisions (hitbox/hurtbox)
    /// - Handles both static and dynamic object collisions
    /// - Applies position correction to prevent object penetration
    /// - Calculates and applies impulses for velocity response
    /// </remarks>
    private void HandleCollision(IEvent evt)
    {
        var collisionEvent = (CollisionEvent)evt;
        if (collisionEvent.EventType == CollisionEventType.End)
            return;
        
        var contact = collisionEvent.Contact;

        // Skip physics response if either collision involves a hitbox or hurtbox
        if ((contact.LayerA == CollisionLayer.Hitbox || contact.LayerA == CollisionLayer.Hurtbox) ||
            (contact.LayerB == CollisionLayer.Hitbox || contact.LayerB == CollisionLayer.Hurtbox))
        {
            return;
        }

        Entity entityA = contact.EntityA;
        Entity entityB = contact.EntityB;

        // Ensure required components exist
        if (!HasComponents<Position>(entityA) || !HasComponents<Position>(entityB))
            return;

        ref var posA = ref GetComponent<Position>(entityA);
        ref var posB = ref GetComponent<Position>(entityB);

        // Determine which objects are dynamic (have velocity)
        bool isDynamicA = HasComponents<Velocity>(entityA);
        bool isDynamicB = HasComponents<Velocity>(entityB);

        // At least one object needs to be dynamic for collision response
        if (!isDynamicA && !isDynamicB)
            return;

        // Calculate inverse masses - 0 for static or infinite mass objects
        float invMassA = 0f;
        float invMassB = 0f;

        if (isDynamicA)
        {
            if (HasComponents<Mass>(entityA))
                invMassA = 1f / GetComponent<Mass>(entityA).Value;
            else
                invMassA = 0f; // Infinite mass
        }

        if (isDynamicB)
        {
            if (HasComponents<Mass>(entityB))
                invMassB = 1f / GetComponent<Mass>(entityB).Value;
            else
                invMassB = 0f; // Infinite mass
        }

        float totalInvMass = invMassA + invMassB;
        if (totalInvMass <= 0f) return; // Both objects have infinite mass

        // Get current velocities
        Vector2 velA = isDynamicA ? GetComponent<Velocity>(entityA).Value : Vector2.Zero;
        Vector2 velB = isDynamicB ? GetComponent<Velocity>(entityB).Value : Vector2.Zero;

        Vector2 relativeVel = velB - velA;
        float normalVel = Vector2.Dot(relativeVel, contact.Normal);

        // Position correction to prevent sinking
        if (contact.Penetration > 0)
        {
            // Constants for position correction
            const float PENETRATION_SLOP = 0.1f;      // Ignore small penetrations for stability
            const float BAUMGARTE = 0.7f;             // Baumgarte factor for smooth correction

            // Calculate correction amount
            float penetrationError = Math.Max(contact.Penetration - PENETRATION_SLOP, 0);
            float bias = BAUMGARTE * penetrationError;
            
            // Distribute correction based on inverse mass ratio
            float moveA = invMassA / totalInvMass * bias;
            float moveB = invMassB / totalInvMass * bias;

            // Apply position corrections
            posA.Value -= contact.Normal * moveA;
            posB.Value += contact.Normal * moveB;
        }

        // Skip impulse if objects are separating
        if (normalVel > 0)
            return;

        // Calculate impulse magnitude using conservation of momentum
        float j = -normalVel / totalInvMass;
        Vector2 impulse = contact.Normal * j;
        
        // Apply impulses to update velocities
        if (isDynamicA)
        {
            ref var velocityA = ref GetComponent<Velocity>(entityA);
            velocityA.Value -= impulse * invMassA;
        }
        
        if (isDynamicB)
        {
            ref var velocityB = ref GetComponent<Velocity>(entityB);
            velocityB.Value += impulse * invMassB;
        }
    }

    public override void Update(World world, GameTime gameTime) { }
}