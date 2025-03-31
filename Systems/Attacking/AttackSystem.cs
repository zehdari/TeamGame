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

    private void HandleAttackAction(IEvent evt)
    {
        var attackEvent = (ActionEvent)evt;

        if (!attackEvent.ActionName.Equals("attack"))
            return;

        if (!HasComponents<PlayerStateComponent>(attackEvent.Entity) ||
            !HasComponents<AnimationConfig>(attackEvent.Entity))
            return;

        // Check if the player is already attacking
        ref var stateComp = ref GetComponent<PlayerStateComponent>(attackEvent.Entity);
        if (stateComp.CurrentState == PlayerState.Attack)
        {
            // Already in an attack, so ignore additional attack inputs
            return;
        }

        if (attackEvent.IsStarted)
        {
            // Get total duration of attack animation
            ref var animConfig = ref GetComponent<AnimationConfig>(attackEvent.Entity);
            if (!animConfig.States.TryGetValue("attack", out var frames))
                return;

            float totalDuration = 0f;
            foreach (var frame in frames)
            {
                totalDuration += frame.Duration;
            }

            Publish(new PlayerStateEvent
            {
                Entity = attackEvent.Entity,
                RequestedState = PlayerState.Attack,
                Force = true, // Force is true to ensure a new attack starts if not already attacking
                Duration = totalDuration
            });

            Publish(new HitboxSpawnEvent
            {
                Entity = attackEvent.Entity,
            });
        }
    }

    public override void Update(World world, GameTime gameTime) { }
}
