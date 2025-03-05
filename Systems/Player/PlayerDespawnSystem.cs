using ECS.Components.Lives;
using ECS.Components.Physics;
using ECS.Components.State;
using ECS.Core;

namespace ECS.Systems.Player;

public class PlayerDespawnSystem : SystemBase
{
    public override void Initialize(World world)
    {
        base.Initialize(world);
    }

    // Determines if an entity is out of bounds based on predefined screen limits
    private bool IsOutOfBounds(Entity entity)
    {
        ref var position = ref GetComponent<Position>(entity);

        // Use GraphicsManager.Instance to get screen size
        Point windowSize = GraphicsManager.Instance.GetWindowSize();
        int screenWidth = windowSize.X;
        //int screenHeight = windowSize.Y;
        int screenHeight = 400;
        int boundaryBuffer = 50; // put boundaryBuffer into MapConfig, maybe separate to x buffer to y buffer

        return position.Value.X < -boundaryBuffer || position.Value.X > screenWidth + boundaryBuffer ||
               position.Value.Y < -boundaryBuffer || position.Value.Y > screenHeight + boundaryBuffer;
    }

    public override void Update(World world, GameTime gameTime)
    {
        foreach (var entity in World.GetEntities())
        {
            if (!HasComponents<LivesCount>(entity))
                continue;

            if (IsOutOfBounds(entity))
            {
                Publish(new DespawnEvent { Entity = entity }); // Trigger despawn event if out of bounds
            }
        }
    }
}
