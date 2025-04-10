using ECS.Components.State;
using ECS.Components.Animation;
using ECS.Components.AI;

namespace ECS.Systems.Attacking;

public class AttackSystem : SystemBase
{
    private readonly AttackHandlingManager handler = new();

    public override void Initialize(World world)
    {
        base.Initialize(world);
        Subscribe<ActionEvent>(HandleAttackAction);
    }

    private void HandleAttackAction(IEvent evt)
    {
        var attackEvent = (AttackActionEvent)evt;

        var info = GetComponent<Attacks>(attackEvent.Entity).AvailableAttacks
            [attackEvent.AttackType][attackEvent.Direction];

        // Throw the attack to the handler and let it do its job
        handler.AttackHandlerLookup[info.AttackHandlerEnum](attackEvent.Entity);

    }

    public override void Update(World world, GameTime gameTime) { }
}
