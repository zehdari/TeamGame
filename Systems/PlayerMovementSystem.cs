namespace ECS.Systems;

public class PlayerMovementSystem : SystemBase
{
    public override void Initialize(World world)
    {
        base.Initialize(world);
        World.EventBus.Subscribe<InputEvent>(HandleInput);
    }

    private void HandleInput(IEvent evt)
    {
        var inputEvent = (InputEvent)evt;
        if (!HasComponents<Force>(inputEvent.Entity) || 
            !HasComponents<MovementForce>(inputEvent.Entity)) 
            return;

        ref var force = ref GetComponent<Force>(inputEvent.Entity);
        ref var movementForce = ref GetComponent<MovementForce>(inputEvent.Entity);
        force.Value = inputEvent.MovementDirection * movementForce.Magnitude;

        // Publish animation state change based on movement
        World.EventBus.Publish(new AnimationStateEvent
        {
            NewState = inputEvent.MovementDirection != Vector2.Zero ? "walking" : "idle",
            Entity = inputEvent.Entity
        });
    }

    public override void Update(World world, GameTime gameTime) { }
}