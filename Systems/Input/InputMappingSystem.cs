using ECS.Components.Input;

namespace ECS.Systems.Input
{
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

            // Check if entity has input configuration
            if (!HasComponents<InputConfig>(entity)) return;

            ref var config = ref GetComponent<InputConfig>(entity);

            // Initialize tracking for this entity if needed
            if (!activeActions.ContainsKey(entity))
            {
                activeActions[entity] = new Dictionary<string, bool>();
            }

            if (rawInput.IsJoystickInput)
                HandleJoystickInput(entity, rawInput, config);
            else if (rawInput.IsGamepadInput)
                HandleGamepadInput(entity, rawInput, config);
            else if (rawInput.IsTriggerInput)
                HandleTriggerInput(entity, rawInput, config);
            else
                HandleKeyboardInput(entity, rawInput, config);
        }

        private void HandleKeyboardInput(Entity entity, RawInputEvent rawInput, InputConfig config)
        {
            foreach (var (actionName, action) in config.Actions)
            {
                if (Array.IndexOf(action.Keys, rawInput.RawKey) == -1) continue;

                if (!activeActions[entity].ContainsKey(actionName))
                    activeActions[entity][actionName] = false;

                bool wasActive = activeActions[entity][actionName];
                bool isActive = action.Keys.Any(key => Keyboard.GetState().IsKeyDown(key));

                activeActions[entity][actionName] = isActive;

                Publish(new ActionEvent
                {
                    ActionName = actionName,
                    Entity = entity,
                    IsStarted = isActive && !wasActive,
                    IsEnded = !isActive && wasActive,
                    IsHeld = isActive
                });
            }
        }

        private void HandleGamepadInput(Entity entity, RawInputEvent rawInput, InputConfig config)
        {
            foreach (var (actionName, action) in config.Actions)
            {
                if (Array.IndexOf(action.Buttons, rawInput.RawButton) == -1) continue;

                if (!activeActions[entity].ContainsKey(actionName))
                    activeActions[entity][actionName] = false;

                bool wasActive = activeActions[entity][actionName];
                var gamepad = GamePad.GetState(rawInput.Player);
                bool isActive = action.Buttons.Any(button => gamepad.IsButtonDown(button));

                activeActions[entity][actionName] = isActive;

                Publish(new ActionEvent
                {
                    ActionName = actionName,
                    Entity = entity,
                    IsStarted = isActive && !wasActive,
                    IsEnded = !isActive && wasActive,
                    IsHeld = isActive
                });
            }
        }

        private void HandleJoystickInput(Entity entity, RawInputEvent rawInput, InputConfig config)
        {
            foreach (var (actionName, action) in config.Actions)
            {
                bool contains = action.Joysticks.Any(j => j.Type == rawInput.JoystickType);
                if (!contains) continue;

                if (!activeActions[entity].ContainsKey(actionName))
                    activeActions[entity][actionName] = false;

                bool wasActive = activeActions[entity][actionName];
                bool isActive = action.Joysticks.Any(j => j.Direction == rawInput.JoystickDirection);

                activeActions[entity][actionName] = isActive;

                Publish(new ActionEvent
                {
                    ActionName = actionName,
                    Entity = entity,
                    IsStarted = isActive && !wasActive,
                    IsEnded = !isActive && wasActive,
                    IsHeld = isActive
                });
            }
        }

        private void HandleTriggerInput(Entity entity, RawInputEvent rawInput, InputConfig config)
        {
            const float TRIGGER_THRESHOLD = 0.5f;

            foreach (var (actionName, action) in config.Actions)
            {
                if (Array.IndexOf(action.Triggers, rawInput.TriggerType) == -1) continue;

                if (!activeActions[entity].ContainsKey(actionName))
                    activeActions[entity][actionName] = false;

                bool wasActive = activeActions[entity][actionName];
                var gamepad = GamePad.GetState(rawInput.Player);

                bool isActive = action.Triggers.Any(trigger =>
                    (trigger == TriggerType.Left && gamepad.Triggers.Left > TRIGGER_THRESHOLD) ||
                    (trigger == TriggerType.Right && gamepad.Triggers.Right > TRIGGER_THRESHOLD)
                );

                activeActions[entity][actionName] = isActive;

                Publish(new ActionEvent
                {
                    ActionName = actionName,
                    Entity = entity,
                    IsStarted = isActive && !wasActive,
                    IsEnded = !isActive && wasActive,
                    IsHeld = isActive
                });
            }
        }

        public override void Update(World world, GameTime gameTime) { }
    }
}
