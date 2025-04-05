using ECS.Components.State;
using ECS.Components.Animation;

namespace ECS.Systems.Attacking;

public class AttackSystem : SystemBase
{
    public override void Initialize(World world)
    {
        base.Initialize(world);
        Subscribe<ActionEvent>(HandleAttackAction);
    }

    private void DealWithAttack(Entity entity)
    {
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
        var attackEvent = (ActionEvent)evt;

        if (!attackEvent.ActionName.Equals(MAGIC.ACTIONS.ATTACK))
            return;

        if (!HasComponents<PlayerStateComponent>(attackEvent.Entity) ||
            !HasComponents<AnimationConfig>(attackEvent.Entity))
            return;

        // Ignore additional inputs if already attacking
        ref var stateComp = ref GetComponent<PlayerStateComponent>(attackEvent.Entity);
        if (stateComp.CurrentState == PlayerState.Attack)
            return;
        
        if (attackEvent.IsStarted)
           DealWithAttack(attackEvent.Entity);
        
    }

    public override void Update(World world, GameTime gameTime) { }
}
