namespace ECSAttempt.Systems;

public class FacingSystem : SystemBase
{
    public override void Initialize(World world)
    {
        base.Initialize(world);
        World.EventBus.Subscribe<InputEvent>(HandleInput);
    }

    private void HandleInput(IEvent evt)
    {
        var inputEvent = (InputEvent)evt;
        if (!HasComponents<FacingDirection>(inputEvent.Entity)) return;

        float horizontalMovement = inputEvent.MovementDirection.X;
        if (horizontalMovement == 0) return;

        ref var facing = ref GetComponent<FacingDirection>(inputEvent.Entity);
        facing.IsFacingLeft = horizontalMovement < 0;
    }

    public override void Update(World world, GameTime gameTime) { }
}