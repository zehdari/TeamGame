using ECS.Components.Physics;
using ECS.Components.State;

namespace ECS.Systems.State;

public class PlayerStateSystem : SystemBase
{
    public override void Update(World world, GameTime gameTime)
    {
        foreach (var entity in world.GetEntities())
        {
            if (!HasComponents<PlayerStateComponent>(entity) ||
                !HasComponents<Velocity>(entity) ||
                !HasComponents<IsGrounded>(entity))
                continue; // Skip entities without required components

            ref var player = ref GetComponent<PlayerStateComponent>(entity);
            ref var velocity = ref GetComponent<Velocity>(entity);
            ref var grounded = ref GetComponent<IsGrounded>(entity);

            // Determine player state based on movement
            if (!grounded.Value && velocity.Value.Y > 0)
            {
                player.currentState = PlayerState.Fall;
            }
            else if (grounded.Value && velocity.Value.X == 0 && velocity.Value.Y ==0)
            {
                player.currentState = PlayerState.Idle; // No movement
            }
        }
    }
}
