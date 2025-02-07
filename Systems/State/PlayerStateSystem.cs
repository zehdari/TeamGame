using ECS.Components.Physics;
using ECS.Components.State;

namespace ECS.Systems.State;

public class PlayerStateSystem : SystemBase
{
    private const float RunThreshold = 200.0f; // Example threshold for running
    private const float WalkThreshold = 0.1f; // Example threshold for walking

    public override void Update(World world, GameTime gameTime)
    {
        foreach (var entity in world.GetEntities())
        {
            if (!HasComponents<PlayerStateComponent>(entity) ||
                !HasComponents<Velocity>(entity))
                continue; // Skip entities without required components

            ref var state = ref GetComponent<PlayerStateComponent>(entity);
            ref var velocity = ref GetComponent<Velocity>(entity);

            // Determine player state based on movement
            if (velocity.Value.Y > 0)
            {
                state.currentState = PlayerState.Fall;
            }
            else if (velocity.Value.Y < 0)
            {
                state.currentState = PlayerState.Jump;
            }
            else if (Math.Abs(velocity.Value.X) > WalkThreshold) // Check for walking
            {
                // Check for running based on velocity
                if (Math.Abs(velocity.Value.X) > RunThreshold)
                {
                    state.currentState = PlayerState.Run;
                }
                else
                {
                    state.currentState = PlayerState.Walk; // Walking state
                }
            }
            else
            {
                state.currentState = PlayerState.Idle; // No movement
            }

            //// Attacking/Blocking states (example, modify based on input system)
            //if (input.AxisValues.ContainsKey("attack") && input.AxisValues["attack"] > 0)
            //{
            //    state.CurrentState = PlayerState.Attack;
            //}

            //else if (input.AxisValues.ContainsKey("block") && input.AxisValues["block"] > 0)
            //{
            //    state.CurrentState = PlayerState.Block;
            //}


            // **Debugging Output: Print Current Player State**
            //Console.WriteLine($"Entity {entity.Id} - Player State: {state.currentState}");

        }
    }
}
