using System.ComponentModel.Design;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using ECS.Components.AI;
using ECS.Components.Input;

namespace ECS.Systems.Input
{
    public class InputMappingSystem : SystemBase
    {
        private Dictionary<Entity, Dictionary<string, bool>> activeActions = new();

        public override bool Pausible => false;

        public override void Initialize(World world)
        {
            base.Initialize(world);
            Subscribe<RawInputEvent>(HandleRawInput);
        }

        private static AttackDirection? GetDirection(bool up, bool down, bool left, bool right)
        {
            if (up) return AttackDirection.Up;
            if (down) return AttackDirection.Down;
            if (left) return AttackDirection.Left;
            if (right) return AttackDirection.Right;
            return null;
        }

        private void HandleRawInput(IEvent evt)
        {
            var raw = (RawInputEvent)evt;
            var entity = raw.Entity;
            if (!HasComponents<InputConfig>(entity)) return;

            ref var config = ref GetComponent<InputConfig>(entity);
            if (!activeActions.ContainsKey(entity))
                activeActions[entity] = new Dictionary<string, bool>();

            if (raw.IsJoystickInput) HandleJoystickInput(entity, raw, config);
            else if (raw.IsGamepadInput) HandleGamepadInput(entity, raw, config);
            else if (raw.IsTriggerInput) HandleTriggerInput(entity, raw, config);
            else HandleKeyboardInput(entity, raw, config);

            HandleSpecializedAttackInput(entity, raw, config);
        }

        private void HandleKeyboardInput(Entity entity, RawInputEvent raw, InputConfig config)
        {
            foreach (var (name, action) in config.Actions)
            {
                if (!action.Keys.Contains(raw.RawKey.GetValueOrDefault())) continue;
                UpdateAndPublish(entity, name, raw.IsPressed);
            }
        }

        private void HandleGamepadInput(Entity entity, RawInputEvent raw, InputConfig config)
        {
            foreach (var (name, action) in config.Actions)
            {
                if (action.Buttons == null || !action.Buttons.Contains(raw.RawButton.GetValueOrDefault())) continue;
                UpdateAndPublish(entity, name, raw.IsPressed);
            }
        }

        private void HandleTriggerInput(Entity entity, RawInputEvent raw, InputConfig config)
        {
            foreach (var (name, action) in config.Actions)
            {
                if (action.Triggers == null || !action.Triggers.Contains(raw.TriggerType.GetValueOrDefault())) continue;
                UpdateAndPublish(entity, name, raw.IsPressed);
            }
        }

        private void HandleJoystickInput(Entity entity, RawInputEvent raw, InputConfig config)
        {
            foreach (var (name, action) in config.Actions)
            {
                if (action.Joysticks == null) continue;
                bool matches = action.Joysticks.Any(j => j.Type == raw.JoystickType && j.Direction == raw.JoystickDirection);
                if (!matches) continue;
                UpdateAndPublish(entity, name, raw.IsPressed);
            }
        }

        private void UpdateAndPublish(Entity entity, string actionName, bool isActive)
        {
            if (!activeActions[entity].ContainsKey(actionName))
                activeActions[entity][actionName] = false;

            bool was = activeActions[entity][actionName];
            activeActions[entity][actionName] = isActive;

            Publish(new ActionEvent
            {
                ActionName = actionName,
                Entity = entity,
                IsStarted = isActive && !was,
                IsEnded   = !isActive && was,
                IsHeld    = isActive
            });
        }

        private void HandleSpecializedAttackInput(Entity entity, RawInputEvent raw, InputConfig config)
        {
            bool jab    = activeActions[entity].GetValueOrDefault(MAGIC.ATTACK.JAB);
            bool spec   = activeActions[entity].GetValueOrDefault(MAGIC.ATTACK.SPECIAL);
            bool up     = activeActions[entity].GetValueOrDefault(MAGIC.DIRECTION.UP);
            bool down   = activeActions[entity].GetValueOrDefault(MAGIC.DIRECTION.DOWN);
            bool left   = activeActions[entity].GetValueOrDefault(MAGIC.DIRECTION.LEFT);
            bool right  = activeActions[entity].GetValueOrDefault(MAGIC.DIRECTION.RIGHT);

            AttackDirection? direction = GetDirection(up, down, left, right);
            AttackType?      type      = jab ? AttackType.Jab : spec ? AttackType.Special : (AttackType?)null;

            if (direction != null && type != null)
            {
                Publish(new AttackActionEvent { Entity = entity, Direction = direction.Value, Type = type.Value });
            }
        }
        public override void Update(World world, GameTime gameTime) { }
    }


}
