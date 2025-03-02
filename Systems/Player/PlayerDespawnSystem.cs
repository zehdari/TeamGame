using ECS.Components.Lives;
using ECS.Components.Physics;
using ECS.Core;

namespace ECS.Systems.Player;

public class PlayerDespawnSystem : SystemBase
{
    public override void Initialize(World world)
    {
        base.Initialize(world);
        Subscribe<CollisionEvent>(HandleCollision); // Listen for collision events
    }

    // Handles collisions and checks if an entity needs to be despawned
    private void HandleCollision(IEvent evt)
    {
        var collisionEvent = (CollisionEvent)evt;
        Entity entity = default;

        if (HasComponents<LivesCount>(collisionEvent.Contact.EntityA))
        {
            entity = collisionEvent.Contact.EntityA;
        }
        else if (HasComponents<LivesCount>(collisionEvent.Contact.EntityB))
        {
            entity = collisionEvent.Contact.EntityB;
        }

        // Ignore non-player entities
        if (entity.Equals(default(Entity)))
            return;

        if (IsOutOfBounds(entity))
        {
            Publish(new DespawnEvent { Entity = entity }); // Trigger despawn event if out of bounds
        }
    }

    // Determines if an entity is out of bounds based on predefined screen limits
    private bool IsOutOfBounds(Entity entity)
    {
        ref var position = ref GetComponent<Position>(entity);

        // Use GraphicsManager.Instance to get screen size
        Point windowSize = GraphicsManager.Instance.GetWindowSize();
        int screenWidth = windowSize.X;
        int screenHeight = windowSize.Y;
        int boundaryBuffer = 50;

        return position.Value.X < -boundaryBuffer || position.Value.X > screenWidth + boundaryBuffer ||
               position.Value.Y < -boundaryBuffer || position.Value.Y > screenHeight + boundaryBuffer;
    }

    public override void Update(World world, GameTime gameTime)
    {
        // No logic needed for PlayerDespawnSystem
    }
}
