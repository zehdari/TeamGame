
using ECS.Components.State;
using ECS.Core;

public class AttackSystem : SystemBase
{

    public override void Initialize(World world)
    {
        base.Initialize(world);
        World.EventBus.Subscribe<ActionEvent>(HandleAttackAction);
    }

    private void HandleAttackAction(IEvent evt)
    {
        var attackEvent = (ActionEvent)evt;

        if (!attackEvent.ActionName.Equals("attack"))
            return;

        if (!HasComponents<PlayerStateComponent>(attackEvent.Entity))
        {
            ref var player = ref GetComponent<PlayerStateComponent>(attackEvent.Entity);
            player.currentState = PlayerState.Attack;
        }
        
        
    }

    public override void Update(World world, GameTime gameTime)
    {
        
           
    }
}
