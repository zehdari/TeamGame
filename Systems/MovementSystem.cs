namespace ECSAttempt.Systems;

public class MovementSystem : SystemBase
{
    private const float MOVEMENT_SPEED = 100f;

    public override void Initialize(World world)
    {
        base.Initialize(world);
        World.EventBus.Subscribe<InputEvent>(HandleInput);
    }

    private void HandleInput(IEvent evt)
    {
        var inputEvent = (InputEvent)evt;
        if (!HasComponents<Velocity>(inputEvent.Entity)) return;

        ref var velocity = ref GetComponent<Velocity>(inputEvent.Entity);
        velocity.Value = inputEvent.MovementDirection * MOVEMENT_SPEED;

        // Publish animation state change based on movement
        World.EventBus.Publish(new AnimationStateEvent
        {
            NewState = inputEvent.MovementDirection != Vector2.Zero ? "walking" : "idle",
            Entity = inputEvent.Entity
        });
    }

    public override void Update(World world, GameTime gameTime)
    {
        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        
        foreach (var entity in World.GetEntities())
        {
            if (!HasComponents<Position>(entity) || !HasComponents<Velocity>(entity))
                continue;

            ref var position = ref GetComponent<Position>(entity);
            ref var velocity = ref GetComponent<Velocity>(entity);
            
            position.Value += velocity.Value * deltaTime;
        }
    }
}