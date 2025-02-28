using ECS.Components.Collision;
using ECS.Components.Physics;

namespace ECS.Systems.Physics;

public class GroundedSystem : SystemBase
{
    public override void Initialize(World world)
    {
        base.Initialize(world);
        Subscribe<CollisionEvent>(HandleCollision);
    }

    private void HandleCollision(IEvent evt)
    {
        var collision = (CollisionEvent)evt;
        UpdateEntityGroundedState(collision.Contact.EntityA, collision);
        UpdateEntityGroundedState(collision.Contact.EntityB, collision);
    }

    private void UpdateEntityGroundedState(Entity entity, CollisionEvent collision)
    {
        if (!HasComponents<IsGrounded>(entity)) return;

        ref var grounded = ref GetComponent<IsGrounded>(entity);
        grounded.WasGrounded = grounded.Value;

        // Only check physics vs world collisions
        if ((collision.Contact.LayerA & CollisionLayer.Physics) == 0 &&
            (collision.Contact.LayerB & CollisionLayer.Physics) == 0)
            return;

        // Get the normal from the entity's perspective
        bool isEntityA = collision.Contact.EntityA.Equals(entity);
        Vector2 normal = isEntityA ? -collision.Contact.Normal : collision.Contact.Normal;

        // Threshold for what counts as ground
        const float SLOPE_THRESHOLD = 0.7f; // About 60 degrees
        bool isOnGround = normal.Y < -SLOPE_THRESHOLD;

        if (isOnGround)
        {
            grounded.Value = collision.EventType == CollisionEventType.End ? false : true;
        }  
    }

    public override void Update(World world, GameTime gameTime)
    {
        // Handle entities that are no longer grounded due to moving away from ground
        foreach (var entity in World.GetEntities())
        {
            if (!HasComponents<IsGrounded>(entity) || 
                !HasComponents<Velocity>(entity))
                continue;

            ref var grounded = ref GetComponent<IsGrounded>(entity);
            ref var velocity = ref GetComponent<Velocity>(entity);

            // If moving upward, lose grounded state
            if (grounded.Value && velocity.Value.Y < 0)
            {
                grounded.WasGrounded = grounded.Value;
                grounded.Value = false;
            }
        }
    }
}