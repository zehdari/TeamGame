using ECS.Components.AI;
using ECS.Components.State;

namespace ECS.Systems.Blocking;
public class BlockRegenerationSystem : SystemBase
{
    const int SHIELD_REGENERATION_RATE = 5;

    public override void Initialize(World world)
    {
        base.Initialize(world);
    }

    private bool isBlocking(Entity entity)
    {
        if (!HasComponents<PlayerStateComponent>(entity))
            return false;

        ref var state = ref GetComponent<PlayerStateComponent>(entity);
        return state.CurrentState == PlayerState.Block;
    }

    public override void Update(World world, GameTime gameTime) 
    {

        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        foreach (Entity entity in world.GetEntities())
        {
            if (!isBlocking(entity))
                continue;

            if (!HasComponents<BlockInfo>(entity))
                continue;

            ref var blockInfo = ref GetComponent<BlockInfo>(entity);

            if(blockInfo.CurrentHealth < blockInfo.MaxHealth)
            {
                blockInfo.CurrentHealth -= SHIELD_REGENERATION_RATE * deltaTime;
            }
            
        }
    }
}
