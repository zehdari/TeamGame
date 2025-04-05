using ECS.Components.Lives;
using ECS.Components.Physics;
using ECS.Components.State;
using ECS.Core;

namespace ECS.Systems.Player;

public class PlayerDespawnSystem : SystemBase
{
    private GraphicsManager graphicsManager;

    public PlayerDespawnSystem(GraphicsManager graphics)
    {
        graphicsManager = graphics;
    }
    
    public override void Initialize(World world)
    {
        base.Initialize(world);
    }

    // Determines if an entity is out of bounds based on predefined screen limits
    private bool IsOutOfBounds(Entity entity)
    {
        const int SAFE_SPAWN_X = 400;
        const int SAFE_SPAWN_Y = 300;

        ref var position = ref GetComponent<Position>(entity);

        // Check for NaN values and fix them
        if (float.IsNaN(position.Value.X) || float.IsNaN(position.Value.Y))
        {
            Logger.Log($"PlayerDespawnSystem: {entity.Id} had a NaN position, resetting to a safe pos.");
            position.Value = new Vector2(SAFE_SPAWN_X, SAFE_SPAWN_Y); // Reset to a safe position
            return false; // Don't trigger despawn for NaN values
        }

        // Use GraphicsManager to get screen size
        Point windowSize = graphicsManager.GetWindowSize();
        int screenWidth = windowSize.X;
        int screenHeight = windowSize.Y;
        const int BOUNDARY_BUFFER = 500;

        return position.Value.X < -BOUNDARY_BUFFER || position.Value.X > screenWidth + BOUNDARY_BUFFER ||
            position.Value.Y < -BOUNDARY_BUFFER || position.Value.Y > screenHeight + BOUNDARY_BUFFER;
    }

    public override void Update(World world, GameTime gameTime)
    {
        foreach (var entity in World.GetEntities())
        {
            if (!HasComponents<LivesCount>(entity))
                continue;

            if (IsOutOfBounds(entity))
            {
                Publish(new LifeLossEvent { Entity = entity }); // Trigger despawn event if out of bounds
            }
        }
    }
}
