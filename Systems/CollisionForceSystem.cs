namespace ECS.Systems;

public class CollisionForceSystem : SystemBase
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

        CollisionFlags sides = collisionEvent.Sides;

        ref var posA = ref GetComponent<Position>(a);
        ref var shapeA = ref GetComponent<CollisionShape>(a);

        ref var posB = ref GetComponent<Position>(b);
        ref var shapeB = ref GetComponent<CollisionShape>(b);

        // Handle physical response
        if (!shapeA.IsPhysical || !shapeB.IsPhysical)
            return;

        bool aHasVelocity = HasComponents<Velocity>(a);
        bool bHasVelocity = HasComponents<Velocity>(b);

        if (!aHasVelocity && !bHasVelocity)
            return;

        // Calculate mass ratios for collision response (This will be changed once we have mass, then we can do proper physics)
        // Both moving objects get half the correction
        float massRatioA = aHasVelocity && bHasVelocity ? 0.5f : aHasVelocity ? 1f : 0f;
        float massRatioB = aHasVelocity && bHasVelocity ? 0.5f : bHasVelocity ? 1f : 0f;

        // Apply position corrections
        if (aHasVelocity)
        {
            posA.Value += normal * (penetration * massRatioA);
        }
        if (bHasVelocity)
        {
            posB.Value -= normal * (penetration * massRatioB);
        }

        // Apply velocity and force corrections
        if (aHasVelocity)
        {
            ref var velA = ref GetComponent<Velocity>(a);
            
            // Velocity correction based on collision normal
            if (Math.Abs(normal.X) > 0.0f)
            {
                velA.Value.X = 0;
                // Also zero out the force to prevent immediate re-application
                if (HasComponents<Force>(a))
                {
                    ref var force = ref GetComponent<Force>(a);
                    force.Value.X = 0;
                }
            }
            if (Math.Abs(normal.Y) > 0.0f)
            {
                velA.Value.Y = 0;
                if (HasComponents<Force>(a))
                {
                    ref var force = ref GetComponent<Force>(a);
                    force.Value.Y = 0;
                }
            }
        }

        if (bHasVelocity)
        {
            ref var velB = ref GetComponent<Velocity>(b);
            
            // Velocity correction based on collision normal
            if (Math.Abs(normal.X) > 0.0f)
            {
                velB.Value.X = 0;
                if (HasComponents<Force>(b))
                {
                    ref var force = ref GetComponent<Force>(b);
                    force.Value.X = 0;
                }
            }
            if (Math.Abs(normal.Y) > 0.0f)
            {
                velB.Value.Y = 0;
                if (HasComponents<Force>(b))
                {
                    ref var force = ref GetComponent<Force>(b);
                    force.Value.Y = 0;
                }
            }
        }
    }

    public override void Update(World world, GameTime gameTime) { }
}