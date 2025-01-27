namespace ECS.Systems;

public class MovementSystem : SystemBase
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

    public override void Update(World world, GameTime gameTime)
    {
        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        
        foreach (var entity in World.GetEntities())
        {
            if (!HasComponents<Position>(entity) || 
                !HasComponents<Velocity>(entity) || 
                !HasComponents<Force>(entity) ||
                !HasComponents<Friction>(entity) ||
                !HasComponents<MaxVelocity>(entity))
                continue;

            ref var position = ref GetComponent<Position>(entity);
            ref var velocity = ref GetComponent<Velocity>(entity);
            ref var force = ref GetComponent<Force>(entity);
            ref var friction = ref GetComponent<Friction>(entity);
            ref var maxVelocity = ref GetComponent<MaxVelocity>(entity);
            
            // Apply force to velocity
            velocity.Value += force.Value * deltaTime;
            
            // Apply friction to slow down
            velocity.Value -= velocity.Value * friction.Value * deltaTime;
            
            // Clamp velocity to max velocity
            if (velocity.Value.Length() > maxVelocity.Value)
            {
                velocity.Value = Vector2.Normalize(velocity.Value) * maxVelocity.Value;
            }
            
            // Update position based on velocity
            position.Value += velocity.Value * deltaTime;
            
            // Reset force after application
            force.Value = Vector2.Zero;
        }
    }
}