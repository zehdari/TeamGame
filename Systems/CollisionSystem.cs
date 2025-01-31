namespace ECS.Systems;

public class CollisionSystem : SystemBase
{
    public override void Initialize(World world)
    {
        base.Initialize(world);
    }

    public override void Update(World world, GameTime gameTime)
    {
        // Reset collision states
        foreach (var entity in World.GetEntities())
        {
            if (!HasComponents<CollisionState>(entity)) continue;
            ref var state = ref GetComponent<CollisionState>(entity);
            state.Sides = CollisionFlags.None;
            state.CollidingWith.Clear();
        }

        // Check collisions between all entities with collision shapes
        var entities = World.GetEntities()
            .Where(e => HasComponents<Position>(e) && 
                       HasComponents<CollisionShape>(e) && 
                       HasComponents<CollisionState>(e))
            .ToList();

        // Compare each pair of entities
        for (int i = 0; i < entities.Count; i++)
        {
            for (int j = i + 1; j < entities.Count; j++)
            {
                CheckCollision(entities[i], entities[j]);
            }
        }

        // Update IsGrounded component based on collision state
        foreach (var entity in entities)
        {
            if (!HasComponents<IsGrounded>(entity)) continue;

            ref var grounded = ref GetComponent<IsGrounded>(entity);
            ref var collisionState = ref GetComponent<CollisionState>(entity);

            grounded.WasGrounded = grounded.Value;
            grounded.Value = (collisionState.Sides & CollisionFlags.Bottom) != 0;
        }
    }

    private void CheckCollision(Entity a, Entity b)
    {
        ref var posA = ref GetComponent<Position>(a);
        ref var shapeA = ref GetComponent<CollisionShape>(a);
        ref var stateA = ref GetComponent<CollisionState>(a);

        ref var posB = ref GetComponent<Position>(b);
        ref var shapeB = ref GetComponent<CollisionShape>(b);
        ref var stateB = ref GetComponent<CollisionState>(b);

        // Only handle rectangle-rectangle collisions for now
        if (shapeA.Type != ShapeType.Rectangle || shapeB.Type != ShapeType.Rectangle)
            return;

        // Calculate AABBs (Axis-Aligned Bounding Box)
        var rectA = new Rectangle(
            (int)(posA.Value.X + shapeA.Offset.X),
            (int)(posA.Value.Y + shapeA.Offset.Y),
            (int)shapeA.Size.X,
            (int)shapeA.Size.Y
        );

        var rectB = new Rectangle(
            (int)(posB.Value.X + shapeB.Offset.X),
            (int)(posB.Value.Y + shapeB.Offset.Y),
            (int)shapeB.Size.X,
            (int)shapeB.Size.Y
        );

        // Check for intersection
        if (!rectA.Intersects(rectB))
            return;

        // Calculate overlap depths
        float overlapX = Math.Min(rectA.Right, rectB.Right) - Math.Max(rectA.Left, rectB.Left);
        float overlapY = Math.Min(rectA.Bottom, rectB.Bottom) - Math.Max(rectA.Top, rectB.Top);

        // Determine collision normal and penetration
        Vector2 normal;
        float penetration;
        
        if (overlapX < overlapY)
        {
            // Horizontal collision
            penetration = overlapX;
            normal = new Vector2(rectA.Center.X < rectB.Center.X ? -1 : 1, 0);
            
            if (normal.X < 0)
            {
                stateA.Sides |= CollisionFlags.Right;
                stateB.Sides |= CollisionFlags.Left;
            }
            else
            {
                stateA.Sides |= CollisionFlags.Left;
                stateB.Sides |= CollisionFlags.Right;
            }
        }
        else
        {
            // Vertical collision
            penetration = overlapY;
            normal = new Vector2(0, rectA.Center.Y < rectB.Center.Y ? -1 : 1);
            
            if (normal.Y < 0)
            {
                stateA.Sides |= CollisionFlags.Bottom;
                stateB.Sides |= CollisionFlags.Top;
            }
            else
            {
                stateA.Sides |= CollisionFlags.Top;
                stateB.Sides |= CollisionFlags.Bottom;
            }
        }

        // Record collision
        stateA.CollidingWith.Add(b);
        stateB.CollidingWith.Add(a);

        // Publish collision event
        World.EventBus.Publish(new CollisionEvent
        {
            EntityA = a,
            EntityB = b,
            Normal = normal,
            Penetration = penetration,
            Sides = stateA.Sides
        });

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
}