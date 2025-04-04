using System.Diagnostics.CodeAnalysis;
using ECS.Components.Input;

namespace ECS.Systems.Input;

public class RawInputSystem : SystemBase
{

    //stores keys that are pressed
    private Dictionary<Entity, HashSet<Keys>> pressedKeys = new();
    private Dictionary<Entity, HashSet<Buttons>> pressedButtons = new();
    // Dictionary to store the previous trigger values for each entity
    private Dictionary<Entity, HashSet<TriggerType>> pressedTriggerList = new();
    private Dictionary<Entity, JoystickDirection> leftDirection = new();
    private Dictionary<Entity, JoystickDirection> rightDirection = new();

    public override bool Pausible => false;
    public override void Update(World world, GameTime gameTime)
    {
        // Keys are player independant and we just count them as player 1
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

    private void PublishRawInputEvent(Entity entity, Keys? key, Buttons? button, bool isGamePad, bool isJoystick, bool IsTrigger, JoystickType? joystickType, Vector2? joystickValue, TriggerType? triggerType, JoystickDirection? joystickDirection, float? triggerValue, PlayerIndex player)
    {

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
            IsTriggerInput = IsTrigger,
            TriggerType = triggerType,
            TriggerValue = triggerValue,
            Player = player
            });
    }

    private bool IsListening(PlayerIndex player, string port)
    {

        if (port != "AcceptsAll")
        {
            if (player == PlayerIndex.One && port != "PlayerOne") return false;
            if (player == PlayerIndex.Two && port != "PlayerTwo") return false;
            if (player == PlayerIndex.Three && port != "PlayerThree") return false;
            if (player == PlayerIndex.Four && port != "PlayerFour") return false;
        }

        return true;

    }

    private void HandleKeys(World world, GameTime gameTime, PlayerIndex player){
        //get state of keys
        var KeyState = Keyboard.GetState();

        foreach (var entity in world.GetEntities())
        {
            // check if we even are capable of input
            if (!HasComponents<InputConfig>(entity)) continue;

            // add entity to pressed keys array if it isnt already
            if (!pressedKeys.ContainsKey(entity))
            {
                pressedKeys.Add(entity, new HashSet<Keys>());

            }

            //get inputconfig
            ref var config = ref GetComponent<InputConfig>(entity);
            //linq to get all the actions
            // make an aray of all the keys this action uses
            var allKeys = config.Actions.Values.SelectMany(a => a.Keys).Distinct();


            // for all keys that this entity uses
            foreach (Keys key in allKeys)
            {
                // this could probably be simplified into one statement but its late and im tired
                HashSet<Keys> PressedKeyList = pressedKeys[entity];
                if (KeyState.IsKeyDown(key) && !PressedKeyList.Contains(key))
                {
                    // add new key to pressed list
                    pressedKeys[entity].Add(key);
                    //publish event for this input
                    PublishRawInputEvent(entity, key, null, false, false, false, null, null, null, null, null, player);

                }
                else if (KeyState.IsKeyUp(key) && PressedKeyList.Contains(key))
                {
                    // remove key from pressedKeys
                    pressedKeys[entity].Remove(key);
                    PublishRawInputEvent(entity, key, null, false, false, false, null, null, null, null, null, player);


                }
            }

        }

    }

        
    private void HandleGamePad(World world, GameTime gameTime, GamePadState gamePadState, PlayerIndex player){

        foreach (var entity in world.GetEntities())
        {
        // Check if the entity can process input
        if (!HasComponents<InputConfig>(entity)) continue;
        if (!HasComponents<OpenPorts>(entity)) continue;

        //throw out calls from ports we dont care about
        ref var ports = ref GetComponent<OpenPorts>(entity);
        if (!IsListening(player, ports.port)) continue;


         // Ensure entity has an entry in pressedButtons
        if (!pressedButtons.ContainsKey(entity))
        pressedButtons[entity] = new HashSet<Buttons>();

        // Get input configuration
        ref var config = ref GetComponent<InputConfig>(entity);
        var allButtons = config.Actions.Values
        .Where(a => a.Buttons != null) // Filter out null Buttons
        .SelectMany(a => a.Buttons ?? Array.Empty<Buttons>()) // Default to empty array if Buttons is null
        .Distinct();



            foreach (Buttons button in allButtons)
        {
            HashSet<Buttons> pressedButtonList = pressedButtons[entity];

                if (gamePadState.IsButtonDown(button) && !pressedButtonList.Contains(button))
            {
                    // Add new button to pressed list
                    pressedButtonList.Add(button);
                    PublishRawInputEvent(entity, null, button, true, false, false, null, null, null, null, null, player);

                }
                else if (gamePadState.IsButtonUp(button) && pressedButtonList.Contains(button))
            {
                // Remove button from pressed list
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


    private void HandleJoystick(Entity entity, JoystickDirection direction, float threshold, Vector2 joystickValue,
                            PlayerIndex player, JoystickType joystickType, Dictionary<Entity, JoystickDirection> directionMap)
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

            //throw out calls from ports we dont care about
            ref var ports = ref GetComponent<OpenPorts>(entity);

            if (!IsListening(player, ports.port)) continue;

            // Ensure the entity has an entry for previous trigger values
            if (!rightDirection.ContainsKey(entity))
            {
                rightDirection[entity] = JoystickDirection.None;
            }

            // Ensure the entity has an entry for previous trigger values
            if (!leftDirection.ContainsKey(entity))
            {
                leftDirection[entity] = JoystickDirection.None;
            }

            // Get input configuration for the entity
            ref var config = ref GetComponent<InputConfig>(entity);


            // for every action the entity has
            foreach (var action in config.Actions.Values)
            {
                if (action.Joysticks == null) continue;

                // for each joystick used in the action
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

    private void HandleTriggers(World world, GameTime gameTime, GamePadState gamePadState, PlayerIndex player){
        float threshold = 0.5f;

        foreach (var entity in world.GetEntities())
        {
            if (!HasComponents<InputConfig>(entity)) continue;
            if (!HasComponents<OpenPorts>(entity)) continue;

            //throw out calls from ports we dont care about
            ref var ports = ref GetComponent<OpenPorts>(entity);

            if (!IsListening(player, ports.port)) continue;


            // Ensure the entity has an entry for previous trigger values
            if (!pressedTriggerList.ContainsKey(entity))
            {
                pressedTriggerList[entity] = new HashSet<TriggerType>();
            }

            // Get input configuration for the entity
            ref var config = ref GetComponent<InputConfig>(entity);

            // Get the current trigger values
            var leftTriggerValue = gamePadState.Triggers.Left;
            var rightTriggerValue = gamePadState.Triggers.Right;

            // for every action the entity has
            foreach (var action in config.Actions.Values)
            {
                HashSet<TriggerType> pressedTriggers = pressedTriggerList[entity];
                // for every trigger in that action
                if(action.Triggers != null)
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
