using ECS.Components.State;
using ECS.Components.Animation;
using ECS.Components.AI;

namespace ECS.Systems.Attacking;

public class AttackSystem : SystemBase
{

    private AttackHandling handler = new();

    public override void Initialize(World world)
    {
        base.Initialize(world);
        Subscribe<ActionEvent>(HandleAttackAction);
    }

    private void DealWithAttack(Entity entity)
    {

        //debug, shouldnt happen
        if (!HasComponents<AnimationConfig>(entity))
        {
            Logger.Log("Attack System tried to open animation config when it didnt exist");
            return;
        }

        // Get total duration of attack animation
        ref var animConfig = ref GetComponent<AnimationConfig>(entity);
        if (!animConfig.States.TryGetValue(MAGIC.ACTIONS.ATTACK, out var frames))
            return;

        float totalDuration = 0f;
        foreach (var frame in frames)
        {
            totalDuration += frame.Duration;
        }

        Publish(new PlayerStateEvent
        {
            Entity = entity,
            RequestedState = PlayerState.Attack,
            Force = true, // Force is true to ensure a new attack starts if not already attacking
            Duration = totalDuration
        });

        Publish(new HitboxSpawnEvent
        {
            Entity = entity,
        });

        Publish<SoundEvent>(new SoundEvent
        {
            SoundKey = MAGIC.SOUND.PUNCH
        });
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
