using ECS.Components.State;

namespace ECS.Systems.Physics;
public class BlockSystem : SystemBase
{
    public override void Initialize(World world)
    {
        base.Initialize(world);
        Subscribe<ActionEvent>(HandleBlockAction);
    }

    private void HandleBlockAction(IEvent evt)
    {
        var blockEvent = (ActionEvent)evt;

        if (!blockEvent.ActionName.Equals("block"))
            return;

        if (!HasComponents<PlayerStateComponent>(blockEvent.Entity))
            return;

        ref var stateComp = ref GetComponent<PlayerStateComponent>(blockEvent.Entity);
        if (stateComp.CurrentState == PlayerState.Stunned)
            return;

        // Check if the block action is just starting and only trigger block if we're not already in block state
        if (blockEvent.IsStarted && stateComp.CurrentState != PlayerState.Block)
        {
            Publish(new PlayerStateEvent
            {
                Entity = blockEvent.Entity,
                RequestedState = PlayerState.Block,
                Force = true
            });
        }
        // Handle the case when the block input is released and only revert if we are currently blocking
        else if (blockEvent.IsEnded && stateComp.CurrentState == PlayerState.Block)
        {
            Publish(new PlayerStateEvent
            {
                Entity = blockEvent.Entity,
                RequestedState = PlayerState.Idle,
                Force = true
            });
        }
    }

    public override void Update(World world, GameTime gameTime) { }
}
