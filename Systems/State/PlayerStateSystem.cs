using ECS.Components.Physics;
using ECS.Components.State;
using ECS.Components.Animation;
using ECS.Components.Timer;

namespace ECS.Systems.State;

public class PlayerStateSystem : SystemBase
{
    private const float VELOCITY_THRESHOLD = 100f;
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
        if (timerEvent.TimerType != TimerType.StateTimer)
            return;

        if (!HasComponents<PlayerStateComponent>(timerEvent.Entity))
            return;

        ref var playerState = ref GetComponent<PlayerStateComponent>(timerEvent.Entity);

        // Process timer only for states that require a timed transition (Attack and Block)
        if (playerState.CurrentState == PlayerState.Attack ||
            playerState.CurrentState == PlayerState.Block ||
            playerState.CurrentState == PlayerState.Stunned)
        {
            // Determine the next appropriate state based on current conditions
            PlayerState nextState = DetermineNextState(timerEvent.Entity);
            SetState(timerEvent.Entity, nextState, true);
        }
    }

    // Determines the next state when a timed state ends.
    private PlayerState DetermineNextState(Entity entity)
    {
        if (HasComponents<Velocity>(entity) && HasComponents<IsGrounded>(entity))
        {
            ref var velocity = ref GetComponent<Velocity>(entity);
            ref var grounded = ref GetComponent<IsGrounded>(entity);

            if (!grounded.Value)
            {
                // If not grounded, transition to Fall.
                return PlayerState.Fall;
            }
            else
            {
                // On the ground: if nearly stationary, use Idle
                if (Math.Abs(velocity.Value.X) < VELOCITY_THRESHOLD)
                {
                    return PlayerState.Idle;
                }
                else
                {
                    // For now, default to Idle
                    return PlayerState.Idle;
                }
            }
        }
        // Fallback
        return PlayerState.Idle;
    }

    private void HandleStateChangeRequest(IEvent evt)
    {
        var stateEvent = (PlayerStateEvent)evt;

        if (!HasComponents<PlayerStateComponent>(stateEvent.Entity))
            return;

        var playerStateComp = GetComponent<PlayerStateComponent>(stateEvent.Entity);
        bool shouldOverride = ShouldOverrideState(playerStateComp.CurrentState, stateEvent.RequestedState, stateEvent.Force);

        if (shouldOverride)
        {
            SetState(stateEvent.Entity, stateEvent.RequestedState, stateEvent.Force);
        }

        // If a duration is provided, set a timer and mark it as one-shot
        if (stateEvent.Duration.HasValue)
        {
            if (!HasComponents<Timers>(stateEvent.Entity))
            {
                World.GetPool<Timers>().Set(stateEvent.Entity, new Timers
                {
                    TimerMap = new Dictionary<TimerType, Timer>()
                });
            }
            ref var timers = ref GetComponent<Timers>(stateEvent.Entity);
            timers.TimerMap[TimerType.StateTimer] = new Timer
            {
                Duration = stateEvent.Duration.Value,
                Elapsed = 0f,
                Type = TimerType.StateTimer,
                OneShot = true
            };
        }
        // If no duration is provided, don't mess with the timer
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
                if (grounded.Value)
                {
                    if (Math.Abs(velocity.Value.X) < VELOCITY_THRESHOLD &&
                        Math.Abs(velocity.Value.Y) < VELOCITY_THRESHOLD)
                    {
                        SetState(entity, PlayerState.Idle, false);
                    }
                }
                else
                {
                    if (velocity.Value.Y > 0)
                    {
                        SetState(entity, PlayerState.Fall, false);
                    }
                }
            }
            previousStates[entity] = player.CurrentState;
        }
    }
}
