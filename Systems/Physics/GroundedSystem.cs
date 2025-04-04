using ECS.Components.Collision;
using ECS.Components.Physics;

namespace ECS.Systems.Physics;

public class GroundedSystem : SystemBase
{
    private const float UNGROUNDED_DELAY = 0.1f; // 100ms buffer to prevent flickering
    private const float SLOPE_THRESHOLD = 0.7f;  // About 60 degrees

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

        bool isOnGround = normal.Y < -SLOPE_THRESHOLD;

        if (isOnGround && collision.EventType != CollisionEventType.End)
        {
            grounded.Value = true;
            grounded.UngroundedTimer = 0f;
            grounded.GroundNormal = normal;
        }
    }

    public override void Update(World world, GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        foreach (var entity in World.GetEntities())
        {
            if (!HasComponents<IsGrounded>(entity) || 
                !HasComponents<Velocity>(entity))
                continue;

            ref var grounded = ref GetComponent<IsGrounded>(entity);
            ref var velocity = ref GetComponent<Velocity>(entity);

            grounded.WasGrounded = grounded.Value;

            if (grounded.Value && velocity.Value.Y < 0)
            {
                grounded.UngroundedTimer += deltaTime;

                if (grounded.UngroundedTimer >= UNGROUNDED_DELAY)
                {
                    grounded.Value = false;
                    grounded.UngroundedTimer = 0f;
                }
            }
            else if (!grounded.Value)
            {
                grounded.UngroundedTimer += deltaTime;
            }
        }
    }
}
