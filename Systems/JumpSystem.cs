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
        var jumpEvent = (ActionEvent)evt;

        if (!jumpEvent.ActionName.Equals("jump"))
        {
            return;
        }

        if (!HasComponents<Force>(jumpEvent.Entity) ||
            !HasComponents<JumpSpeed>(jumpEvent.Entity) ||
            !HasComponents<IsGrounded>(jumpEvent.Entity))
            return;

        ref var force = ref GetComponent<Force>(jumpEvent.Entity);
        ref var grounded = ref GetComponent<IsGrounded>(jumpEvent.Entity);
        ref var jump = ref GetComponent<JumpSpeed>(jumpEvent.Entity);

        // Only jump if we are grounded and the jump button is pressed
        if (grounded.Value && jumpEvent.IsStarted)
        {
            force.Value += jump.Value;
        }
    }

    public override void Update(World world, GameTime gameTime) { }
}