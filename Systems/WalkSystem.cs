public class WalkSystem : SystemBase
{
    private Dictionary<Entity, bool> isWalkingLeft = new();
    private Dictionary<Entity, bool> isWalkingRight = new();

    public override void Initialize(World world)
    {
        base.Initialize(world);
        World.EventBus.Subscribe<ActionEvent>(HandleWalkAction);
    }

    private void HandleWalkAction(IEvent evt)
    {
        var walkEvent = (ActionEvent)evt;
        
        if (walkEvent.ActionName.Equals("walk_left"))
        {
            if (!isWalkingLeft.ContainsKey(walkEvent.Entity))
                isWalkingLeft[walkEvent.Entity] = false;

            isWalkingLeft[walkEvent.Entity] = walkEvent.IsHeld;
        }

        if (walkEvent.ActionName.Equals("walk_right"))
        {
            if (!isWalkingRight.ContainsKey(walkEvent.Entity))
                isWalkingRight[walkEvent.Entity] = false;

            isWalkingRight[walkEvent.Entity] = walkEvent.IsHeld;
        }
    }

    public override void Update(World world, GameTime gameTime)
    {
        foreach (var entity in World.GetEntities())
        {
            if (!HasComponents<Force>(entity) || 
                !HasComponents<WalkForce>(entity) ||
                !HasComponents<IsGrounded>(entity))
                continue;

            ref var force = ref GetComponent<Force>(entity);
            ref var walk = ref GetComponent<WalkForce>(entity);
            ref var grounded = ref GetComponent<IsGrounded>(entity);

            // Only walk when grounded (AirControlSystem will handle air movement)
            if (!grounded.Value) continue;

            // Determine walking direction
            float direction = 0f;
            if (isWalkingLeft.TryGetValue(entity, out bool walkingLeft) && walkingLeft)
                direction -= 1f;
            if (isWalkingRight.TryGetValue(entity, out bool walkingRight) && walkingRight)
                direction += 1f;

            // Apply force based on walking direction
            if (direction != 0)
            {
                Vector2 walkForce = new Vector2(direction * walk.Value, 0);
                force.Value += walkForce;
            }
        }
    }
}