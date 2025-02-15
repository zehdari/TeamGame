using ECS.Components.Physics;
using ECS.Components.State;
using ECS.Components.Animation;
using ECS.Components.Timer;

namespace ECS.Systems.State;

public class PlayerStateSystem : SystemBase
{
    private const float VELOCITY_THRESHOLD = 50f;
    private Dictionary<Entity, PlayerState> previousStates = new();

    public override void Initialize(World world)
    {
        base.Initialize(world);
        Subscribe<PlayerStateEvent>(HandleStateChangeRequest);
        Subscribe<TimerEvent>(HandleStateTimer);
    }

    private void HandleStateTimer(IEvent evt)
    {
        var timerEvent = (TimerEvent)evt;
        
        if (!HasComponents<PlayerStateComponent>(timerEvent.Entity))
            return;

        // Return to idle state when timer expires
        SetState(timerEvent.Entity, PlayerState.Idle, true);
    }

    private void HandleStateChangeRequest(IEvent evt)
    {
        var stateEvent = (PlayerStateEvent)evt;
        
        if (!HasComponents<PlayerStateComponent>(stateEvent.Entity))
            return;

        if (ShouldOverrideState(GetComponent<PlayerStateComponent>(stateEvent.Entity).CurrentState, 
                               stateEvent.RequestedState, 
                               stateEvent.Force))
        {
            SetState(stateEvent.Entity, stateEvent.RequestedState, stateEvent.Force);

            // If duration specified, add a timer component
            if (stateEvent.Duration.HasValue)
            {
                World.GetPool<Timer>().Set(stateEvent.Entity, new Timer
                {
                    Duration = stateEvent.Duration.Value,
                    Elapsed = 0f
                });
            }
        }
    }

    private bool ShouldOverrideState(PlayerState currentState, PlayerState newState, bool force)
    {
        if (force)
            return true;

        return (int)newState >= (int)currentState;
    }

    private void SetState(Entity entity, PlayerState newState, bool force)
    {
        ref var playerState = ref GetComponent<PlayerStateComponent>(entity);

        if (newState == playerState.CurrentState && !force)
            return;

        playerState.CurrentState = newState;

        Publish(new AnimationStateEvent
        {
            Entity = entity,
            NewState = newState.ToString().ToLower()
        });
    }

    private bool IsInPriorityState(PlayerState state)
    {
        return (int)state >= (int)PlayerState.Block;
    }
    
    public override void Update(World world, GameTime gameTime)
    {
        foreach (var entity in world.GetEntities())
        {
            if (!HasComponents<PlayerStateComponent>(entity) ||
                !HasComponents<Velocity>(entity) ||
                !HasComponents<IsGrounded>(entity))
                continue;

            ref var player = ref GetComponent<PlayerStateComponent>(entity);
            ref var velocity = ref GetComponent<Velocity>(entity);
            ref var grounded = ref GetComponent<IsGrounded>(entity);

            if (!previousStates.ContainsKey(entity))
            {
                previousStates[entity] = player.CurrentState;
            }

            if (!IsInPriorityState(player.CurrentState))
            {
                if (!grounded.Value && velocity.Value.Y > 0)
                {
                    SetState(entity, PlayerState.Fall, false);
                }
                else if (Math.Abs(velocity.Value.X) < VELOCITY_THRESHOLD && 
                         Math.Abs(velocity.Value.Y) < VELOCITY_THRESHOLD)
                {
                    SetState(entity, PlayerState.Idle, false);
                }
            }
            previousStates[entity] = player.CurrentState;
        }
    }
}