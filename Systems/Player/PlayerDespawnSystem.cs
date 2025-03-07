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
        ref var position = ref GetComponent<Position>(entity);

        // Use GraphicsManager.Instance to get screen size
        Point windowSize = graphicsManager.GetWindowSize();
        int screenWidth = windowSize.X;
        int screenHeight = windowSize.Y;
        int boundaryBuffer = 500;

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
                Publish(new LifeLossEvent { Entity = entity }); // Trigger despawn event if out of bounds
            }
        }
    }
}
