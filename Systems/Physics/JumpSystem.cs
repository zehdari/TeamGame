using ECS.Components.Physics;
using ECS.Components.State;

namespace ECS.Systems.Physics;

public class JumpSystem : SystemBase
{
    public override void Initialize(World world)
    {
        base.Initialize(world);
        World.EventBus.Subscribe<ActionEvent>(HandleJump);
    }

    private void HandleJump(IEvent evt)
    {
        var jumpEvent = (ActionEvent)evt;

        if (!jumpEvent.ActionName.Equals("jump"))
        {
            return;
        }

        if (!HasComponents<Force>(jumpEvent.Entity) ||
            !HasComponents<JumpForce>(jumpEvent.Entity) ||
            !HasComponents<IsGrounded>(jumpEvent.Entity) ||
            !HasComponents<PlayerStateComponent>(jumpEvent.Entity))
            return;

        ref var force = ref GetComponent<Force>(jumpEvent.Entity);
        ref var grounded = ref GetComponent<IsGrounded>(jumpEvent.Entity);
        ref var jump = ref GetComponent<JumpForce>(jumpEvent.Entity);
        ref var player = ref GetComponent<PlayerStateComponent>(jumpEvent.Entity);

        // Only jump if we are grounded and the jump button is pressed
        if (grounded.Value && jumpEvent.IsStarted)
        {
            force.Value += new Vector2(0, -jump.Value);
            player.currentState = PlayerState.Jump;

        }
        System.Diagnostics.Debug.WriteLine($"Entity{jumpEvent.Entity.Id} - Player State: {player.currentState}");
    }

    public override void Update(World world, GameTime gameTime) { }
}