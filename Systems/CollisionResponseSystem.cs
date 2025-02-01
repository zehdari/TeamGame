namespace ECS.Systems;

public class CollisionResponseSystem : SystemBase
{
    public override void Initialize(World world)
    {
        base.Initialize(world);
        World.EventBus.Subscribe<CollisionEvent>(HandleCollision);
    }

    public void HandleCollision(IEvent evt)
    {
        var collisionEvent = (CollisionEvent)evt;
        Entity a = collisionEvent.EntityA;
        Entity b = collisionEvent.EntityB;
        Vector2 normal = collisionEvent.Normal;
        float penetration = collisionEvent.Penetration;

        if (!HasComponents<Position>(a) || !HasComponents<CollisionShape>(a) ||
            !HasComponents<Position>(b) || !HasComponents<CollisionShape>(b))
            return;

        ref var posA = ref GetComponent<Position>(a);
        ref var shapeA = ref GetComponent<CollisionShape>(a);
        ref var posB = ref GetComponent<Position>(b);
        ref var shapeB = ref GetComponent<CollisionShape>(b);

        // Handle physical response only
        if (!shapeA.IsPhysical || !shapeB.IsPhysical)
            return;

        bool aHasVelocity = HasComponents<Velocity>(a);
        bool bHasVelocity = HasComponents<Velocity>(b);

        // At least one Entity must have velocity
        if (!aHasVelocity && !bHasVelocity)
            return;

        // Calculate mass ratios for collision response
        float massRatioA = 1f;
        float massRatioB = 1f;

        if (HasComponents<Mass>(a) && HasComponents<Mass>(b))
        {
            ref var massA = ref GetComponent<Mass>(a);
            ref var massB = ref GetComponent<Mass>(b);
            float totalMass = massA.Value + massB.Value;
            massRatioA = massB.Value / totalMass;
            massRatioB = massA.Value / totalMass;
        }
        else
        {
            // If no mass components, use simple ratios based on mobility
            massRatioA = aHasVelocity && bHasVelocity ? 0.5f : aHasVelocity ? 1f : 0f;
            massRatioB = aHasVelocity && bHasVelocity ? 0.5f : bHasVelocity ? 1f : 0f;
        }

        // Apply position corrections
        if (aHasVelocity)
        {
            posA.Value += normal * (penetration * massRatioA);
        }
        if (bHasVelocity)
        {
            posB.Value -= normal * (penetration * massRatioB);
        }

        // Apply velocity corrections - full stop in collision direction (subject to change)
        if (aHasVelocity)
        {
            ref var velA = ref GetComponent<Velocity>(a);
            
            // Project velocity onto collision normal
            float velAlongNormal = Vector2.Dot(velA.Value, normal);
            if (velAlongNormal < 0) // Only cancel velocity if moving into collision
            {
                // Remove velocity in the normal direction
                velA.Value -= normal * velAlongNormal;
            }
        }

        if (bHasVelocity)
        {
            ref var velB = ref GetComponent<Velocity>(b);
            
            // Project velocity onto collision normal
            float velAlongNormal = Vector2.Dot(velB.Value, -normal);
            if (velAlongNormal < 0) // Only cancel velocity if moving into collision
            {
                // Remove velocity in the normal direction
                velB.Value -= -normal * velAlongNormal;
            }
        }
    }

    public override void Update(World world, GameTime gameTime) { }
}