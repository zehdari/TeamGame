namespace ECSAttempt.Systems;

public class InputEventSystem : SystemBase
{
    private Game game;

    public InputEventSystem(Game game)
    {
        this.game = game;
    }

    public override void Initialize(World world)
    {
        base.Initialize(world);
        World.EventBus.Subscribe<GameExitEvent>(HandleGameExit);
    }

    private void HandleGameExit(IEvent evt)
    {
        game.Exit();
    }

    public override void Update(World world, GameTime gameTime)
    {
        var keyState = Keyboard.GetState();
        var gamepadState = GamePad.GetState(PlayerIndex.One);
        
        // Check for system-wide exit condition
        if (gamepadState.Buttons.Back == ButtonState.Pressed)
        {
            World.EventBus.Publish(new GameExitEvent());
            return;
        }

        foreach (var entity in World.GetEntities())
        {
            if (!HasComponents<InputConfig>(entity) || !HasComponents<InputState>(entity))
                continue;

            ref var config = ref GetComponent<InputConfig>(entity);
            ref var state = ref GetComponent<InputState>(entity);

            // Create a new axis values dictionary for this entity
            var entityAxisValues = new Dictionary<string, float>();
            foreach (var axis in state.AxisValues.Keys)
            {
                entityAxisValues[axis] = 0f;
            }

            // Process each configured action for this specific entity
            foreach (var (actionName, action) in config.Actions)
            {
                if (action.Keys.Any(key => keyState.IsKeyDown(key)))
                {
                    if (!entityAxisValues.ContainsKey(action.Axis))
                    {
                        entityAxisValues[action.Axis] = 0f;
                    }
                    entityAxisValues[action.Axis] += action.Value;

                    // Check if this is an exit action
                    if (actionName == "exit" && action.Axis == "system")
                    {
                        World.EventBus.Publish(new GameExitEvent());
                        return;
                    }
                }
            }

            // Update input state for this specific entity
            state.AxisValues = entityAxisValues;

            // Create input event with movement direction for this entity
            if (HasComponents<Velocity>(entity))
            {
                var direction = new Vector2(
                    entityAxisValues.GetValueOrDefault("horizontal", 0f),
                    entityAxisValues.GetValueOrDefault("vertical", 0f)
                );

                // Normalize if moving diagonally
                if (direction != Vector2.Zero)
                {
                    direction = Vector2.Normalize(direction);
                }

                World.EventBus.Publish(new InputEvent 
                { 
                    MovementDirection = direction,
                    Entity = entity
                });
            }
        }
    }
}