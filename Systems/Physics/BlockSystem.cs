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
            
        Publish(new PlayerStateEvent
        {
            Entity = blockEvent.Entity,
            RequestedState = PlayerState.Block
        });
    }

    public override void Update(World world, GameTime gameTime) { }
}
