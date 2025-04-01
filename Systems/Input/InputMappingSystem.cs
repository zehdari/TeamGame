using ECS.Components.Input;

namespace ECS.Systems.Input;

public class InputMappingSystem : SystemBase
{
    // Track active keys and their corresponding actions for each entity
    private Dictionary<Entity, Dictionary<string, bool>> activeActions = new();
    public override bool Pausible => false;

    public override void Initialize(World world)
    {
        base.Initialize(world);
        Subscribe<RawInputEvent>(HandleRawInput);
    }

    private void HandleRawInput(IEvent evt)
    {
        var rawInput = (RawInputEvent)evt;
        var entity = rawInput.Entity;

        ref var config = ref GetComponent<InputConfig>(entity);

        // Initialize tracking for this entity if needed
        if (!activeActions.ContainsKey(entity))
        {
            activeActions[entity] = new Dictionary<string, bool>();
        }

        // Check if entity has input configuration
        if (!HasComponents<InputConfig>(entity)) return;

        if(rawInput.IsJoystickInput) {
            HandleJoystickInput(entity, rawInput, config);
        } else if(rawInput.IsGamepadInput){
            HandleGamepadInput(entity, rawInput, config);
        } else if(rawInput.IsTriggerInput){
            HandleTriggerInput(entity, rawInput, config);
        } else{
            HandleKeyboardInput(entity, rawInput, config);
        }
    }

    private void HandleKeyboardInput(Entity entity, RawInputEvent rawInput, InputConfig config){


        // Find all actions that use this key
        foreach (var (actionName, action) in config.Actions)
        {
            if (Array.IndexOf(action.Keys, rawInput.RawKey) != -1)
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
                Publish(new ActionEvent
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

    private void HandleGamepadInput(Entity entity, RawInputEvent rawInput, InputConfig config)
    {


        // Find all actions that use this key
        foreach (var (actionName, action) in config.Actions)
        {

            if (Array.IndexOf(action.Buttons, rawInput.RawButton) != -1)
            {

                // Initialize action state if not already tracked
                if (!activeActions[entity].ContainsKey(actionName))
                {
                    activeActions[entity][actionName] = false;
                }

                bool wasActive = activeActions[entity][actionName];
                bool isActive = false;


                // Update action state based on the keys
                foreach (var button in action.Buttons)
                {
                        var gamepad = GamePad.GetState(rawInput.Player);
                        if (gamepad.IsButtonDown(button))
                        {
                            isActive = true;
                            break;
                        }
                }

                activeActions[entity][actionName] = isActive;


                // Generate the ActionEvent
                Publish(new ActionEvent
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

    private void HandleJoystickInput(Entity entity, RawInputEvent rawInput, InputConfig config){
        foreach (var (actionName, action) in config.Actions)
        {

            bool contains = false;
            foreach (var joystick in action.Joysticks)
            {
                if (joystick.Type == rawInput.JoystickType) contains = true;
            }

            if (contains)
            {
                if (!activeActions[entity].ContainsKey(actionName))
                {
                    activeActions[entity][actionName] = false;
                }


                bool wasActive = activeActions[entity][actionName];
                bool isActive = false;

                // Update action state based on the keys
                foreach (var joystick in action.Joysticks)
                {

                    var direction = joystick.Direction;

                    if (rawInput.JoystickDirection == direction)
                    {
                        
                        isActive = true;
                        break;
                    }
                }


                activeActions[entity][actionName] = isActive;


                // Generate the ActionEvent
                Publish(new ActionEvent
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

    private void HandleTriggerInput(Entity entity, RawInputEvent rawInput, InputConfig config){
        foreach (var (actionName, action) in config.Actions)
        {

            if (Array.IndexOf(action.Triggers, rawInput.TriggerType) != -1)
            {
                // Initialize action state if not already tracked
                if (!activeActions[entity].ContainsKey(actionName))
                {
                    activeActions[entity][actionName] = false;
                }

                bool wasActive = activeActions[entity][actionName];
                bool isActive = false;

                // Update action state based on the triggers
                foreach (var trigger in action.Triggers){

                    var gamepad = GamePad.GetState(rawInput.Player);
                    if ((trigger == TriggerType.Left && gamepad.Triggers.Left > 0.5) || (trigger == TriggerType.Right && gamepad.Triggers.Right > 0.5))
                    {
                        isActive = true;
                        break;
                    }
                }

                activeActions[entity][actionName] = isActive;


                // Generate the ActionEvent
                Publish(new ActionEvent
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
