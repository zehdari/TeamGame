using ECS.Components.Physics;
using ECS.Components.State;
using ECS.Core;

namespace ECS.Systems.Physics;

public class JumpSystem : SystemBase
{
    public override void Initialize(World world)
    {
        base.Initialize(world);
        Subscribe<ActionEvent>(HandleJump);
    }

    private void HandleJump(IEvent evt)
    {
        var jumpEvent = (ActionEvent)evt;

        if (!jumpEvent.ActionName.Equals(MAGIC.ACTIONS.JUMP))
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
        ref var state = ref GetComponent<PlayerStateComponent>(jumpEvent.Entity);

        if (state.CurrentState == PlayerState.Stunned)
            return;

        // Only jump if we are grounded and the jump button is pressed
        if (grounded.Value && jumpEvent.IsStarted)
        {
            force.Value += new Vector2(0, -jump.Value);

            Publish(new PlayerStateEvent
            {
                Entity = jumpEvent.Entity,
                RequestedState = PlayerState.Jump
            });

            Publish<SoundEvent>(new SoundEvent
            {
                SoundKey = MAGIC.SOUND.JUMP,
            });
        }
    }

    public override void Update(World world, GameTime gameTime) { }
}