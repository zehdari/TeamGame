using ECS.Components.State;
using ECS.Components.Animation;
using ECS.Components.AI;

namespace ECS.Systems.Attacking;

public class AttackSystem : SystemBase
{
    private AttackHandlingManager handler;

    public AttackSystem(World world)
    {
        handler = new AttackHandlingManager(world);
    }

    public override void Initialize(World world)
    {
        base.Initialize(world);
        Subscribe<AttackActionEvent>(HandleAttackAction);
    }

    private void HandleAttackAction(IEvent evt)
    {
        var attackEvent = (AttackActionEvent)evt;

        System.Diagnostics.Debug.WriteLine($"Attack event: Type = {attackEvent.Type}, Direction = {attackEvent.Direction}");

        var info = GetComponent<Attacks>(attackEvent.Entity).AvailableAttacks
            [attackEvent.Type][attackEvent.Direction];

        System.Diagnostics.Debug.WriteLine($"AttackInfo: Enum = {info.AttackHandlerEnum}");

        // Throw the attack to the handler and let it do its job
        handler.AttackHandlerLookup[info.AttackHandlerEnum](attackEvent.Entity);

    }

    public override void Update(World world, GameTime gameTime) { }
}
