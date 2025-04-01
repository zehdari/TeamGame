using ECS.Components.AI;
using ECS.Components.State;

namespace ECS.Systems.Blocking;
public class BlockRegenerationSystem : SystemBase
{
    const int SHIELD_REGENERATION_RATE = 20;

    public override void Initialize(World world)
    {
        base.Initialize(world);
    }

    private bool isBlocking(Entity entity)
    {
        ref var state = ref GetComponent<PlayerStateComponent>(entity);
        return state.CurrentState == PlayerState.Block;
    }

    public override void Update(World world, GameTime gameTime) 
    {
        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        foreach (var entity in world.GetEntities())
        {
            if (!HasComponents<PlayerStateComponent>(entity))
                continue;

            if (!HasComponents<BlockInfo>(entity))
                continue;

            ref var blockInfo = ref GetComponent<BlockInfo>(entity);

            System.Diagnostics.Debug.WriteLine("About to regen shield, cur and max health are " + blockInfo.CurrentHealth + " " + blockInfo.MaxHealth);
            if(blockInfo.CurrentHealth < blockInfo.MaxHealth)
            {
                blockInfo.CurrentHealth += SHIELD_REGENERATION_RATE * deltaTime;
                System.Diagnostics.Debug.WriteLine("Regened Shield");
            }
            
        }
    }
}
