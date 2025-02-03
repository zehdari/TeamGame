using ECS.Core;
using ECS.Events;

namespace ECS.Systems;

public class JumpSystem : SystemBase
{

    public override void Initialize(World world)
    {
        base.Initialize(world);
        World.EventBus.Subscribe<ActionEvent>(HandleJump);
    }


    private void HandleJump(IEvent evt)
    {
        //checks to see if action event is jump
        var jumpEvent = (ActionEvent)evt;
        if (!jumpEvent.ActionName.Equals("JUMP"))
        {
            return;
        }
        //checks to see if it has these components so we can make sure we can get them
        if (!HasComponents<Force>(jumpEvent.Entity) ||
            !HasComponents<JumpSpeed>(jumpEvent.Entity) ||
            !HasComponents<IsGrounded>(jumpEvent.Entity))
            return;
        ref var force = ref GetComponent<Force>(jumpEvent.Entity);
        ref var grounded = ref GetComponent<IsGrounded>(jumpEvent.Entity);
        ref var jump = ref GetComponent<JumpSpeed>(jumpEvent.Entity);
        //check make sure grounded (may change later to be able to jump mid air)
        if (!grounded.Value && jumpEvent.IsHeld)
        {
            force.Value += jump.Value;
        }
    }
    public override void Update(World world, GameTime gameTime)
    {
    }
}