using ECS.Components.AI;
using ECS.Components.Physics;
using ECS.Components.State;
using ECS.Core;

namespace ECS.Systems.Attacking;

/// <summary>
/// Temporary system to convert shoot events to attackActions. This needs to die asap.
/// </summary>
public class ShootConvertSystem : SystemBase
{
    public override void Initialize(World world)
    {
        base.Initialize(world);
        Subscribe<ActionEvent>(HandleAction);
    }

    private void HandleAction(IEvent evt)
    {
        var actionEvent = (ActionEvent)evt;

        if (!actionEvent.ActionName.Equals(MAGIC.ACTIONS.SHOOT))
        {
            return;
        }

        System.Diagnostics.Debug.WriteLine("Converting...");

        Publish<AttackActionEvent>(new AttackActionEvent
        {
            Direction = AttackDirection.Right,
            Type = AttackType.Special,
            Entity = actionEvent.Entity,
        });
    }

    public override void Update(World world, GameTime gameTime) { }
}