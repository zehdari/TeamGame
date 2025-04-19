using System.Diagnostics.CodeAnalysis;
using ECS.Components.Input;

namespace ECS.Systems.Input;

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
        // Keys are player-independent, so we just count them as player 1
        for (PlayerIndex player = PlayerIndex.One; player <= PlayerIndex.Four; player++)
        {
            if (player == PlayerIndex.One)
            {
                HandleKeys(world, gameTime, player);
            }

            var gamePadState = GamePad.GetState(player);
            HandleGamePad(world, gameTime, gamePadState, player);
            HandleTriggers(world, gameTime, gamePadState, player);
            HandleJoysticks(world, gameTime, gamePadState, player);
        }
    }

    private void PublishRawInputEvent(Entity entity, Keys? key, Buttons? button, bool isGamePad, bool isJoystick, bool isTrigger, JoystickType? joystickType, Vector2? joystickValue, TriggerType? triggerType, JoystickDirection? joystickDirection, float? triggerValue, PlayerIndex player)
    {
        if(entity.Equals(null)|| player > PlayerIndex.Four)
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
            Player = player
        });
    }

    private bool IsListening(PlayerIndex player, string port)
    {
            if (port == MAGIC.GAMEPAD.ACCEPTS_ALL) return true;
            if (player == PlayerIndex.One && port == MAGIC.GAMEPAD.PLAYER_ONE) return true;
            if (player == PlayerIndex.Two && port == MAGIC.GAMEPAD.PLAYER_TWO) return true;
            if (player == PlayerIndex.Three && port == MAGIC.GAMEPAD.PLAYER_THREE) return true;
            if (player == PlayerIndex.Four && port == MAGIC.GAMEPAD.PLAYER_FOUR) return true;
        

        return false;
    }

    private void HandleKeys(World world, GameTime gameTime, PlayerIndex player)
    {
        var keyState = Keyboard.GetState();


        foreach (var entity in world.GetEntities())
        {
            if (!HasComponents<InputConfig>(entity)) continue;
            if (!HasComponents<OpenPorts>(entity)) continue;
            ref var ports = ref GetComponent<OpenPorts>(entity);
            if (!IsListening(player, ports.port)) continue;

            if (!pressedKeys.ContainsKey(entity))
            {
                pressedKeys.Add(entity, new HashSet<Keys>());
            }

            ref var config = ref GetComponent<InputConfig>(entity);
            var allKeys = config.Actions.Values.SelectMany(a => a.Keys).Distinct();

            foreach (Keys key in allKeys)
            {
                HashSet<Keys> pressedKeyList = pressedKeys[entity];
                if (keyState.IsKeyDown(key) && !pressedKeyList.Contains(key))
                {
                    pressedKeys[entity].Add(key);
                    PublishRawInputEvent(entity, key, null, false, false, false, null, null, null, null, null, player);
                }
                else if (keyState.IsKeyUp(key) && pressedKeyList.Contains(key))
                {
                    pressedKeys[entity].Remove(key);
                    PublishRawInputEvent(entity, key, null, false, false, false, null, null, null, null, null, player);
                }
            }
        }
    }

    private void HandleGamePad(World world, GameTime gameTime, GamePadState gamePadState, PlayerIndex player)
    {
        foreach (var entity in world.GetEntities())
        {
            if (!HasComponents<InputConfig>(entity)) continue;
            if (!HasComponents<OpenPorts>(entity)) continue;

            ref var ports = ref GetComponent<OpenPorts>(entity);
            if (!IsListening(player, ports.port)) continue;

            if (!pressedButtons.ContainsKey(entity))
            {
                pressedButtons[entity] = new HashSet<Buttons>();
            }

            ref var config = ref GetComponent<InputConfig>(entity);
            var allButtons = config.Actions.Values
                .Where(a => a.Buttons != null)
                .SelectMany(a => a.Buttons ?? Array.Empty<Buttons>())
                .Distinct();

            foreach (Buttons button in allButtons)
            {
                HashSet<Buttons> pressedButtonList = pressedButtons[entity];

                if (gamePadState.IsButtonDown(button) && !pressedButtonList.Contains(button))
                {
                    pressedButtonList.Add(button);
                    PublishRawInputEvent(entity, null, button, true, false, false, null, null, null, null, null, player);
                }
                else if (gamePadState.IsButtonUp(button) && pressedButtonList.Contains(button))
                {
                    pressedButtonList.Remove(button);
                    PublishRawInputEvent(entity, null, button, true, false, false, null, null, null, null, null, player);
                }
            }
        }
    }

    private JoystickDirection GetJoyStickDirection(Vector2 joystick, float threshold)
    {
        float x = joystick.X;
        float y = joystick.Y;

        if (x > threshold && y > threshold)
            return JoystickDirection.UpRight;
        if (x > threshold && y < -threshold)
            return JoystickDirection.DownRight;
        if (x < -threshold && y < -threshold)
            return JoystickDirection.DownLeft;
        if (x < -threshold && y > threshold)
            return JoystickDirection.UpLeft;
        if (x > threshold)
            return JoystickDirection.Right;
        if (x < -threshold)
            return JoystickDirection.Left;
        if (y > threshold)
            return JoystickDirection.Up;
        if (y < -threshold)
            return JoystickDirection.Down;

        return JoystickDirection.None;
    }

    private void HandleJoystick(Entity entity, JoystickDirection direction, float threshold, Vector2 joystickValue, PlayerIndex player, JoystickType joystickType, Dictionary<Entity, JoystickDirection> directionMap)
    {
        var currentDirection = GetJoyStickDirection(joystickValue, threshold);
        var previousDirection = directionMap[entity];

        bool directionChanged = (currentDirection == direction && previousDirection == JoystickDirection.None)
                                || (currentDirection != direction && previousDirection != JoystickDirection.None);

        if (directionChanged)
        {
            PublishRawInputEvent(entity, null, null, false, true, false, joystickType, joystickValue, null, currentDirection, null, player);
        }

        directionMap[entity] = currentDirection;
    }

    private void HandleJoysticks(World world, GameTime gameTime, GamePadState gamePadState, PlayerIndex player)
    {
        foreach (var entity in world.GetEntities())
        {
            if (!HasComponents<InputConfig>(entity)) continue;
            if (!HasComponents<OpenPorts>(entity)) continue;

            ref var ports = ref GetComponent<OpenPorts>(entity);
            if (!IsListening(player, ports.port)) continue;

            if (!rightDirection.ContainsKey(entity))
            {
                rightDirection[entity] = JoystickDirection.None;
            }

            if (!leftDirection.ContainsKey(entity))
            {
                leftDirection[entity] = JoystickDirection.None;
            }

            ref var config = ref GetComponent<InputConfig>(entity);

            foreach (var action in config.Actions.Values)
            {
                if (action.Joysticks == null) continue;

                foreach (var joystick in action.Joysticks)
                {
                    if (joystick.Type == JoystickType.LeftStick)
                    {
                        Vector2 left = gamePadState.ThumbSticks.Left;
                        HandleJoystick(entity, joystick.Direction, joystick.Threshold, left, player, JoystickType.LeftStick, leftDirection);
                    }

                    if (joystick.Type == JoystickType.RightStick)
                    {
                        Vector2 right = gamePadState.ThumbSticks.Right;
                        HandleJoystick(entity, joystick.Direction, joystick.Threshold, right, player, JoystickType.RightStick, rightDirection);
                    }
                }
            }
        }
    }

    private void HandleTriggers(World world, GameTime gameTime, GamePadState gamePadState, PlayerIndex player)
    {
        float threshold = 0.5f;

        foreach (var entity in world.GetEntities())
        {
            if (!HasComponents<InputConfig>(entity)) continue;
            if (!HasComponents<OpenPorts>(entity)) continue;

            ref var ports = ref GetComponent<OpenPorts>(entity);
            if (!IsListening(player, ports.port)) continue;

            if (!pressedTriggerList.ContainsKey(entity))
            {
                pressedTriggerList[entity] = new HashSet<TriggerType>();
            }

            ref var config = ref GetComponent<InputConfig>(entity);

            var leftTriggerValue = gamePadState.Triggers.Left;
            var rightTriggerValue = gamePadState.Triggers.Right;

            foreach (var action in config.Actions.Values)
            {
                HashSet<TriggerType> pressedTriggers = pressedTriggerList[entity];

                if (action.Triggers != null)
                {
                    foreach (var trigger in action.Triggers)
                    {
                        float value = trigger == TriggerType.Left ? leftTriggerValue :
                                      trigger == TriggerType.Right ? rightTriggerValue : 0f;

                        if (trigger != TriggerType.Left && trigger != TriggerType.Right)
                            continue;

                        bool isPressed = value > threshold;
                        bool wasPressed = pressedTriggers.Contains(trigger);

                        if (isPressed && !wasPressed)
                        {
                            pressedTriggers.Add(trigger);
                            PublishRawInputEvent(entity, null, null, false, false, true, null, null, trigger, null, value, player);
                        }
                        else if (!isPressed && wasPressed)
                        {
                            pressedTriggers.Remove(trigger);
                            PublishRawInputEvent(entity, null, null, false, false, true, null, null, trigger, null, value, player);
                        }
                    }
                }
            }
        }
    }
}
