using ECS.Components.Animation;
using ECS.Components.Physics;
using ECS.Components.Projectiles;
using ECS.Components.PVZ;
using ECS.Components.Random;

namespace ECS.Systems.Spawning;

public class ZombiesEatingBrainsSystem : SystemBase
{
    private GameStateManager gameStateManager;
    private const float BrainsPosition = 300;

    public ZombiesEatingBrainsSystem(GameStateManager gameStateManager)
    {
        this.gameStateManager = gameStateManager;
    }
    public override void Update(World world, GameTime gameTime)
    {
       
        foreach (var entity in World.GetEntities())
        {
            if (!HasComponents<ZombieTag>(entity))
            {
                continue;
            }
            ref var position = ref GetComponent<Position>(entity);
            if(position.Value.X < BrainsPosition)
            {
                gameStateManager.ReturnToMainMenu();
            }
        }
    }
}