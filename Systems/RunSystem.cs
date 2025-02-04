public class RunSystem : SystemBase
{
    private Dictionary<Entity, bool> isRunningLeft = new();
    private Dictionary<Entity, bool> isRunningRight = new();

    public override void Initialize(World world)
    {
        base.Initialize(world);
        World.EventBus.Subscribe<ActionEvent>(HandleRunAction);
    }

    private void HandleRunAction(IEvent evt)
    {
        var runEvent = (ActionEvent)evt;

        if (runEvent.ActionName.Equals("run_left"))
        {
            if (!isRunningLeft.ContainsKey(runEvent.Entity))
                isRunningLeft[runEvent.Entity] = false;

            isRunningLeft[runEvent.Entity] = runEvent.IsHeld;
        }

        if (runEvent.ActionName.Equals("run_right"))
        {
            if (!isRunningRight.ContainsKey(runEvent.Entity))
                isRunningRight[runEvent.Entity] = false;

            isRunningRight[runEvent.Entity] = runEvent.IsHeld;
        }
    }

    public override void Update(World world, GameTime gameTime)
    {
        foreach (var entity in World.GetEntities())
        {
            if (!HasComponents<Force>(entity) ||
                !HasComponents<WalkForce>(entity) ||
                !HasComponents<IsGrounded>(entity) ||
                !HasComponents<RunSpeed>(entity))
                continue;

            ref var force = ref GetComponent<Force>(entity);
            ref var walk = ref GetComponent<WalkForce>(entity);
            ref var grounded = ref GetComponent<IsGrounded>(entity);
            ref var run = ref GetComponent<RunSpeed>(entity);

            // Only run when grounded (AirControlSystem will handle air movement)
            if (!grounded.Value) continue;

            // Determine running direction
            float direction = 0f;
            if (isRunningLeft.TryGetValue(entity, out bool runningLeft) && runningLeft)
                direction -= 1f;
            if (isRunningRight.TryGetValue(entity, out bool runningRight) && runningRight)
                direction += 1f;

            // Apply force based on walking direction
            if (direction != 0)
            {
                Vector2 runForce = new Vector2(direction * walk.Value*run.Scalar, 0);
                force.Value += runForce;
            }
        }
    }
}