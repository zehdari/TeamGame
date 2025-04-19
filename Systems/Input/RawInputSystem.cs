using System.Diagnostics.CodeAnalysis;
using ECS.Components.Input;
using ECS.Events;

namespace ECS.Systems.Input
{
    public class RawInputSystem : SystemBase
    {
        // Stores keys that are pressed
        private Dictionary<Entity, HashSet<Keys>> pressedKeys = new();
        private Dictionary<Entity, HashSet<Buttons>> pressedButtons = new();
        private Dictionary<Entity, HashSet<TriggerType>> pressedTriggerList = new();
        private Dictionary<Entity, JoystickDirection> leftDirection = new();
        private Dictionary<Entity, JoystickDirection> rightDirection = new();

        public override bool Pausible => false;

        public override void Update(World world, GameTime gameTime)
        {
            for (PlayerIndex player = PlayerIndex.One; player <= PlayerIndex.Four; player++)
            {
                HandleKeys(world, gameTime, player);
                var gamePadState = GamePad.GetState(player);
                HandleGamePad(world, gameTime, gamePadState, player);
                HandleTriggers(world, gameTime, gamePadState, player);
                HandleJoysticks(world, gameTime, gamePadState, player);
            }
        }

        private void PublishRawInputEvent(
            Entity entity,
            Keys? key,
            Buttons? button,
            bool isGamePad,
            bool isJoystick,
            bool isTrigger,
            JoystickType? joystickType,
            Vector2? joystickValue,
            TriggerType? triggerType,
            JoystickDirection? joystickDirection,
            float? triggerValue,
            bool isPressed,
            PlayerIndex player)
        {
            // Use Equals(default(Entity)) since Entity has no == operator
            if (entity.Equals(default(Entity)) || player > PlayerIndex.Four)
            {
                Logger.Log("RawInputSystem Skipped Publishing an event because it detected an error");
                return;
            }

            Publish(new RawInputEvent
            {
                Entity = entity,
                RawKey = key,
                RawButton = button,
                IsGamepadInput = isGamePad,
                IsJoystickInput = isJoystick,
                JoystickType = joystickType,
                JoystickValue = joystickValue,
                JoystickDirection = joystickDirection,
                IsTriggerInput = isTrigger,
                TriggerType = triggerType,
                TriggerValue = triggerValue,
                IsPressed = isPressed,
                Player = player
            });
        }

        private void HandleKeys(World world, GameTime gameTime, PlayerIndex player)
        {
            var keyState = Keyboard.GetState();

            foreach (var entity in world.GetEntities())
            {
                if (!HasComponents<InputConfig>(entity) || !HasComponents<OpenPorts>(entity))
                    continue;

                ref var ports = ref GetComponent<OpenPorts>(entity);
                if (!IsListening(player, ports.port))
                    continue;

                if (!pressedKeys.ContainsKey(entity))
                    pressedKeys[entity] = new HashSet<Keys>();

                ref var config = ref GetComponent<InputConfig>(entity);
                var allKeys = config.Actions.Values.SelectMany(a => a.Keys).Distinct();

                foreach (Keys key in allKeys)
                {
                    var list = pressedKeys[entity];
                    if (keyState.IsKeyDown(key) && !list.Contains(key))
                    {
                        list.Add(key);
                        PublishRawInputEvent(entity, key, null, false, false, false, null, null, null, null, null, true, player);
                    }
                    else if (keyState.IsKeyUp(key) && list.Contains(key))
                    {
                        list.Remove(key);
                        PublishRawInputEvent(entity, key, null, false, false, false, null, null, null, null, null, false, player);
                    }
                }
            }
        }

        private void HandleGamePad(World world, GameTime gameTime, GamePadState gamePadState, PlayerIndex player)
        {
            foreach (var entity in world.GetEntities())
            {
                if (!HasComponents<InputConfig>(entity) || !HasComponents<OpenPorts>(entity))
                    continue;

                ref var ports = ref GetComponent<OpenPorts>(entity);
                if (!IsListening(player, ports.port))
                    continue;

                if (!pressedButtons.ContainsKey(entity))
                    pressedButtons[entity] = new HashSet<Buttons>();

                ref var config = ref GetComponent<InputConfig>(entity);
                var allButtons = config.Actions.Values
                    .Where(a => a.Buttons != null)
                    .SelectMany(a => a.Buttons)
                    .Distinct();

                foreach (Buttons button in allButtons)
                {
                    var list = pressedButtons[entity];
                    if (gamePadState.IsButtonDown(button) && !list.Contains(button))
                    {
                        list.Add(button);
                        PublishRawInputEvent(entity, null, button, true, false, false, null, null, null, null, null, true, player);
                    }
                    else if (gamePadState.IsButtonUp(button) && list.Contains(button))
                    {
                        list.Remove(button);
                        PublishRawInputEvent(entity, null, button, true, false, false, null, null, null, null, null, false, player);
                    }
                }
            }
        }

        private void HandleTriggers(World world, GameTime gameTime, GamePadState gamePadState, PlayerIndex player)
        {
            const float threshold = 0.5f;

            foreach (var entity in world.GetEntities())
            {
                if (!HasComponents<InputConfig>(entity) || !HasComponents<OpenPorts>(entity))
                    continue;

                ref var ports = ref GetComponent<OpenPorts>(entity);
                if (!IsListening(player, ports.port))
                    continue;

                if (!pressedTriggerList.ContainsKey(entity))
                    pressedTriggerList[entity] = new HashSet<TriggerType>();

                ref var config = ref GetComponent<InputConfig>(entity);
                var leftVal = gamePadState.Triggers.Left;
                var rightVal = gamePadState.Triggers.Right;

                foreach (var action in config.Actions.Values)
                {
                    var pressedSet = pressedTriggerList[entity];
                    if (action.Triggers == null) continue;

                    foreach (var trigger in action.Triggers)
                    {
                        if (trigger != TriggerType.Left && trigger != TriggerType.Right) continue;

                        float value = trigger == TriggerType.Left ? leftVal : rightVal;
                        bool isPressed = value > threshold;
                        bool wasPressed = pressedSet.Contains(trigger);

                        if (isPressed && !wasPressed)
                        {
                            pressedSet.Add(trigger);
                            PublishRawInputEvent(entity, null, null, false, false, true, null, null, trigger, null, value, true, player);
                        }
                        else if (!isPressed && wasPressed)
                        {
                            pressedSet.Remove(trigger);
                            PublishRawInputEvent(entity, null, null, false, false, true, null, null, trigger, null, value, false, player);
                        }
                    }
                }
            }
        }

        private void HandleJoysticks(World world, GameTime gameTime, GamePadState gamePadState, PlayerIndex player)
        {
            foreach (var entity in world.GetEntities())
            {
                if (!HasComponents<InputConfig>(entity) || !HasComponents<OpenPorts>(entity))
                    continue;

                ref var ports = ref GetComponent<OpenPorts>(entity);
                if (!IsListening(player, ports.port))
                    continue;

                if (!leftDirection.ContainsKey(entity)) leftDirection[entity] = JoystickDirection.None;
                if (!rightDirection.ContainsKey(entity)) rightDirection[entity] = JoystickDirection.None;

                ref var config = ref GetComponent<InputConfig>(entity);

                foreach (var action in config.Actions.Values)
                {
                    if (action.Joysticks == null) continue;

                    foreach (var joystick in action.Joysticks)
                    {
                        var stickValue = joystick.Type == JoystickType.LeftStick
                            ? gamePadState.ThumbSticks.Left
                            : gamePadState.ThumbSticks.Right;
                        var dirMap = joystick.Type == JoystickType.LeftStick ? leftDirection : rightDirection;

                        var prev = dirMap[entity];
                        var curr = GetJoyStickDirection(stickValue, joystick.Threshold);
                        if (curr != prev)
                        {
                            bool isPressed = curr != JoystickDirection.None;
                            PublishRawInputEvent(entity, null, null, false, true, false, joystick.Type, stickValue, null, curr, null, isPressed, player);
                        }
                        dirMap[entity] = curr;
                    }
                }
            }
        }

        private JoystickDirection GetJoyStickDirection(Vector2 joystick, float threshold)
        {
            float x = joystick.X;
            float y = joystick.Y;
            if (x > threshold && y > threshold) return JoystickDirection.UpRight;
            if (x > threshold && y < -threshold) return JoystickDirection.DownRight;
            if (x < -threshold && y < -threshold) return JoystickDirection.DownLeft;
            if (x < -threshold && y > threshold) return JoystickDirection.UpLeft;
            if (x > threshold) return JoystickDirection.Right;
            if (x < -threshold) return JoystickDirection.Left;
            if (y > threshold) return JoystickDirection.Up;
            if (y < -threshold) return JoystickDirection.Down;
            return JoystickDirection.None;
        }

        private bool IsListening(PlayerIndex player, string port)
        {
            if (port == MAGIC.GAMEPAD.ACCEPTS_ALL)
                return true;
            
            if (player == PlayerIndex.One)
                return port == MAGIC.GAMEPAD.PLAYER_ONE;
            if (player == PlayerIndex.Two)
                return port == MAGIC.GAMEPAD.PLAYER_TWO;
            if (player == PlayerIndex.Three)
                return port == MAGIC.GAMEPAD.PLAYER_THREE;
            if (player == PlayerIndex.Four)
                return port == MAGIC.GAMEPAD.PLAYER_FOUR;

            return false;
        }
    }

}