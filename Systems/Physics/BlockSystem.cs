using ECS.Components.State;

public class BlockSystem : SystemBase
{

    public override void Initialize(World world)
    {
        base.Initialize(world);
        World.EventBus.Subscribe<ActionEvent>(HandleBlockAction);
    }

    private void HandleBlockAction(IEvent evt)
    {
        var blockEvent = (ActionEvent)evt;

        if (!blockEvent.ActionName.Equals("block"))
            return;

        if (!HasComponents<PlayerStateComponent>(blockEvent.Entity))
        {
            ref var player = ref GetComponent<PlayerStateComponent>(blockEvent.Entity);
            player.currentState = PlayerState.Block;
        }

        // Send an event to trigger the jump animation
        World.EventBus.Publish(new AnimationStateEvent
        {
            Entity = blockEvent.Entity,
            NewState = "jump"
        });


    }

    public override void Update(World world, GameTime gameTime)
    {
        
           
    }
}
