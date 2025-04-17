using ECS.Components.State;
using ECS.Components.Physics;
public class GridSystem : SystemBase
{
    private Dictionary<Entity, Dictionary<string, bool >> actions = new();

    public override void Initialize(World world)
    {
        base.Initialize(world);
        Subscribe<ActionEvent>(HandleWalkAction);
    }

    private void HandleWalkAction(IEvent evt)
    {
        var moveEvent = (ActionEvent)evt;
        if(!actions.ContainsKey(moveEvent.Entity))
        {
            actions.Add(moveEvent.Entity, new());
        }
        actions[moveEvent.Entity][moveEvent.ActionName] = moveEvent.IsHeld;
    }
           
    private void RequestPlayerState(Entity entity, PlayerState state)
    {
        Publish(new PlayerStateEvent
        {
            Entity = entity,
            RequestedState = state
        });
    }

    public override void Update(World world, GameTime gameTime)
    {
        foreach (var entity in World.GetEntities())
        {
            if (!HasComponents<Force>(entity) || 
                !HasComponents<WalkForce>(entity) ||
                !HasComponents<IsGrounded>(entity) ||
                !HasComponents<RunSpeed>(entity) ||
                !HasComponents<PlayerStateComponent>(entity))
                continue;
            
            ref var force = ref GetComponent<Force>(entity);
            ref var walk = ref GetComponent<WalkForce>(entity);
            ref var grounded = ref GetComponent<IsGrounded>(entity);
            ref var run = ref GetComponent<RunSpeed>(entity);
            ref var state = ref GetComponent<PlayerStateComponent>(entity);

            if (state.CurrentState == PlayerState.Stunned)
                return;

            // Only walk/run when grounded (AirControlSystem will handle air movement)
            if (!grounded.Value) continue;
            bool isRunning = false;
            // Determine walking direction/running
            float direction = 0f;
            if (actions.TryGetValue(entity, out Dictionary<string, bool> shouldMove))
            {
                if (shouldMove.TryGetValue(MAGIC.ACTIONS.WALKRIGHT, out bool walkingRight) && walkingRight)
                    direction += 1f;
                if (shouldMove.TryGetValue(MAGIC.ACTIONS.WALKLEFT, out bool walkingLeft) && walkingLeft)
                    direction -= 1f;
                if (shouldMove.TryGetValue(MAGIC.ACTIONS.RUN, out bool running) && running)
                {
                    isRunning = true;
                }
            }
            // Apply force based on walking direction
            if (direction != 0)
            {
                // Calculate tangent to the ground
                Vector2 groundNormal = grounded.GroundNormal;
                Vector2 tangent = new Vector2(-groundNormal.Y, groundNormal.X); // perpendicular to normal
                tangent = Vector2.Normalize(tangent);

                // Apply walk/run force along tangent
                Vector2 walkForce = tangent * (direction * walk.Value);

                if (isRunning)
                {
                    walkForce *= run.Scalar;
                    RequestPlayerState(entity, PlayerState.Run);
                }
                else
                {
                    RequestPlayerState(entity, PlayerState.Walk);
                }

                force.Value += walkForce;
            }
        }
    }
}