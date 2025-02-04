namespace ECS.Systems;

public class InputMappingSystem : SystemBase
{
    // Track active keys and their corresponding actions for each entity
    private Dictionary<Entity, Dictionary<string, bool>> activeActions = new();

    public override void Initialize(World world)
    {
        base.Initialize(world);
        World.EventBus.Subscribe<RawInputEvent>(HandleRawInput);
    }

    private void HandleRawInput(IEvent evt)
    {
        var rawInput = (RawInputEvent)evt;
        var entity = rawInput.Entity;

        // Check if entity has input configuration
        if (!HasComponents<InputConfig>(entity)) return;

        ref var config = ref GetComponent<InputConfig>(entity);

        // Initialize tracking for this entity if needed
        if (!activeActions.ContainsKey(entity))
        {
            activeActions[entity] = new Dictionary<string, bool>();
        }

        // Find all actions that use this key
        foreach (var (actionName, action) in config.Actions)
        {
            if (action.Keys.Contains(rawInput.RawKey))
            {
                // Initialize action state if not already tracked
                if (!activeActions[entity].ContainsKey(actionName))
                {
                    activeActions[entity][actionName] = false;
                }

                bool wasActive = activeActions[entity][actionName];
                bool isActive = false;

                // Update action state based on the keys
                foreach (var key in action.Keys)
                {
                    if (Keyboard.GetState().IsKeyDown(key))
                    {
                        isActive = true;
                        break; // Once we find ANY pressed key (for that action), we can stop checking
                    }
                }

                activeActions[entity][actionName] = isActive;

                // Generate the ActionEvent
                World.EventBus.Publish(new ActionEvent
                {
                    ActionName = actionName,
                    Entity = entity,
                    IsStarted = isActive && !wasActive,
                    IsEnded = !isActive && wasActive,
                    IsHeld = isActive // This will be the same for single action-key mappings, but not for actions with multiple key triggers
                });
            }
        }
    }

    public override void Update(World world, GameTime gameTime) { }
}