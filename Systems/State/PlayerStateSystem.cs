using ECS.Components.Physics;
using ECS.Components.State;
using ECS.Components.Animation;

namespace ECS.Systems.State;

public class PlayerStateSystem : SystemBase
{
    private const float VELOCITY_THRESHOLD = 50f;
    private Dictionary<Entity, PlayerState> previousStates = new();

    public override void Initialize(World world)
    {
        base.Initialize(world);
        World.EventBus.Subscribe<PlayerStateEvent>(HandleStateChangeRequest);
    }

    private void HandleStateChangeRequest(IEvent evt)
    {
        var stateEvent = (PlayerStateEvent)evt;
        
        if (!HasComponents<PlayerStateComponent>(stateEvent.Entity))
            return;

        ref var playerState = ref GetComponent<PlayerStateComponent>(stateEvent.Entity);

        if (ShouldOverrideState(playerState.CurrentState, stateEvent.RequestedState, stateEvent.Force))
        {
            SetState(stateEvent.Entity, stateEvent.RequestedState);
        }
    }

    private bool ShouldOverrideState(PlayerState currentState, PlayerState newState, bool force)
    {
        if (force)
            return true;

        return (int)newState >= (int)currentState;
    }

    private void SetState(Entity entity, PlayerState newState)
    {
        ref var playerState = ref GetComponent<PlayerStateComponent>(entity);

        if (newState == playerState.CurrentState)
            return;

        playerState.CurrentState = newState;

        World.EventBus.Publish(new AnimationStateEvent
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

            // Store previous state if not already tracking this entity
            if (!previousStates.ContainsKey(entity))
            {
                previousStates[entity] = player.CurrentState;
            }

            // Only update state if we're not in a priority state
            if (!IsInPriorityState(player.CurrentState))
            {
                if (!grounded.Value && velocity.Value.Y > 0)
                {
                    SetState(entity, PlayerState.Fall);
                }
                else if (Math.Abs(velocity.Value.X) < VELOCITY_THRESHOLD && 
                         Math.Abs(velocity.Value.Y) < VELOCITY_THRESHOLD)
                {
                    SetState(entity, PlayerState.Idle);
                }
            }
            // Update previous state
            previousStates[entity] = player.CurrentState;
        }
    }
}