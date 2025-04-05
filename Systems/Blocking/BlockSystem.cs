using ECS.Components.AI;
using ECS.Components.State;

namespace ECS.Systems.Blocking;
public class BlockSystem : SystemBase
{
    const int SHIELD_DAMAGE_RATE = 50;
    const float STUN_TIME_ON_SHIELD_BREAK = 0.5f; // in seconds

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

            blockInfo.CurrentHealth -= SHIELD_DAMAGE_RATE * deltaTime;

            if(blockInfo.CurrentHealth < 0)
            {
                Publish<PlayerStateEvent>(new PlayerStateEvent
                {
                    Entity = entity,
                    RequestedState = PlayerState.Stunned,
                    Force = true, 
                    Duration = STUN_TIME_ON_SHIELD_BREAK,
                });
            }
        }
    }
}
