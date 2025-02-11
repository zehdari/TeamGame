using ECS.Components.State;
using ECS.Components.Physics;

public class MoveSystem : SystemBase
{
    private Dictionary<Entity, Dictionary<string, bool >> actions = new();

    public override void Initialize(World world)
    {
        base.Initialize(world);
        World.EventBus.Subscribe<ActionEvent>(HandleWalkAction);
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
            ref var player = ref GetComponent<PlayerStateComponent>(entity);

            // Only walk/run when grounded (AirControlSystem will handle air movement)
            if (!grounded.Value) continue;
            bool isRunning = false;
            // Determine walking direction/running
            float direction = 0f;
            if (actions.TryGetValue(entity, out Dictionary<string, bool> shouldMove))
            {
                if (shouldMove.TryGetValue("walk_right", out bool walkingRight) && walkingRight)
                    direction += 1f;
                if (shouldMove.TryGetValue("walk_left", out bool walkingLeft) && walkingLeft)
                    direction -= 1f;
                if (shouldMove.TryGetValue("run", out bool running) && running)
                {
                    isRunning = true;
                }
            }
            // Apply force based on walking direction
            if (direction != 0)
            {
                
                Vector2 walkForce = new Vector2(direction * walk.Value, 0);
                if (isRunning)
                {
                    walkForce *= run.Scalar;
                    player.currentState = PlayerState.Run;

                    // Send an event to trigger the attack animation
                    World.EventBus.Publish(new AnimationStateEvent
                    {
                        Entity = entity,
                        NewState = "walking"
                    });
                }
                else
                {
                    player.currentState = PlayerState.Walk;

                    // Send an event to trigger the attack animation
                    World.EventBus.Publish(new AnimationStateEvent
                    {
                        Entity = entity,
                        NewState = "walking"
                    });
                }
                force.Value += walkForce;
            }
        }
    }
}