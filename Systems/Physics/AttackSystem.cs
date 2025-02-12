using ECS.Components.State;

namespace ECS.Systems.Physics;

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
            return;
        
        World.EventBus.Publish(new PlayerStateEvent
        {
            Entity = attackEvent.Entity,
            RequestedState = PlayerState.Attack
        });
    }

    public override void Update(World world, GameTime gameTime) { }
}