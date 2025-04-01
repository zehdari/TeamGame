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
        PlayerIndex player = PlayerIndex.One;
        HandleKeys(world, gameTime, player);

        var gamePadState = GamePad.GetState(player);
        HandleGamePad(world, gameTime, gamePadState, player);
        HandleTriggers(world, gameTime, gamePadState, player);
        HandleJoyStick(world, gameTime, gamePadState, player);

        player = PlayerIndex.Two;
        gamePadState = GamePad.GetState(player);
        HandleGamePad(world, gameTime, gamePadState, player);
        HandleTriggers(world, gameTime, gamePadState, player);
        HandleJoyStick(world, gameTime, gamePadState, player);

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

            if(ports.port != "AcceptsAll"){
                if(player == PlayerIndex.One && ports.port != "PlayerOne") continue;
                if(player == PlayerIndex.Two && ports.port != "PlayerTwo") continue;
                if(player == PlayerIndex.Three && ports.port != "PlayerThree") continue;
                if(player == PlayerIndex.Four && ports.port != "PlayerFour") continue;
            }
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

    private JoystickDirection GetLeftJoyStickDirection(Vector2 joystick, float threshold)
    {
        // Determine direction based on joystick values.
        JoystickDirection direction;

        if (joystick.X > threshold && joystick.Y > threshold)
            direction = JoystickDirection.UpRight;
        else if (joystick.X > threshold && joystick.Y < -threshold)
            direction = JoystickDirection.DownRight;
        else if (joystick.X < -threshold && joystick.Y < -threshold)
            direction = JoystickDirection.DownLeft;
        else if (joystick.X < -threshold && joystick.Y > threshold)
            direction = JoystickDirection.UpLeft;
        else if (joystick.X > threshold)
            direction = JoystickDirection.Right;
        else if (joystick.X < -threshold)
            direction = JoystickDirection.Left;
        else if (joystick.Y > threshold)
            direction = JoystickDirection.Up;
        else if (joystick.Y < -threshold)
            direction = JoystickDirection.Down;
        else
            direction = JoystickDirection.None;
        //System.Diagnostics.Trace.WriteLine($"Joystick LEft: {joystick.X}, Y: {joystick.Y}, Direction: {direction}");

        return direction;
    }

    private JoystickDirection GetRightJoyStickDirection(Vector2 joystick, float threshold)
    {
        // Determine direction based on joystick values.
        JoystickDirection direction;

        if (joystick.X > threshold && joystick.Y > threshold)
            direction = JoystickDirection.DownLeft;
        else if (joystick.X > threshold && joystick.Y < -threshold)
            direction = JoystickDirection.UpLeft;
        else if (joystick.X < -threshold && joystick.Y < -threshold)
            direction = JoystickDirection.UpRight;
        else if (joystick.X < -threshold && joystick.Y > threshold)
            direction = JoystickDirection.DownRight;
        else if (joystick.X > threshold)
            direction = JoystickDirection.Left;
        else if (joystick.X < -threshold)
            direction = JoystickDirection.Right;
        else if (joystick.Y > threshold)
            direction = JoystickDirection.Down;
        else if (joystick.Y < -threshold)
            direction = JoystickDirection.Up;
        else
            direction = JoystickDirection.None;

        //System.Diagnostics.Trace.WriteLine($"Joystick Right: {joystick.X}, Y: {joystick.Y}, Direction: {direction}");
        return direction;
    }

    private void HandleLeftJoyStick(Entity entity, JoystickDirection direction, float threshold, Vector2 leftJoystickValue, PlayerIndex player)
    {

        var joystickDirection = GetLeftJoyStickDirection(leftJoystickValue, threshold);

        // direction has changed so make an event
        if ((joystickDirection == direction && leftDirection[entity] == JoystickDirection.None) || (joystickDirection != direction && leftDirection[entity] != JoystickDirection.None))
        {
            // new direction, is pressed should be true
            PublishRawInputEvent(entity, null, null, false, true, false, JoystickType.LeftStick, leftJoystickValue,null, joystickDirection, null, player);


        }

        leftDirection[entity] = joystickDirection;
    
    }

    private void HandleRightJoyStick(Entity entity, JoystickDirection direction, float threshold, Vector2 rightJoystickValue, PlayerIndex player)
    {

        var joystickDirection = GetRightJoyStickDirection(rightJoystickValue, threshold);

        // direction has changed so make an event
        if (joystickDirection == direction && rightDirection[entity] == JoystickDirection.None || (joystickDirection != direction && rightDirection[entity] != JoystickDirection.None))
        {
            // new direction, is pressed should be true
            PublishRawInputEvent(entity, null, null, false, true, false, JoystickType.RightStick, rightJoystickValue, null, joystickDirection, null, player);
        }

        rightDirection[entity] = joystickDirection;
      
    }

    private void HandleJoyStick(World world, GameTime gameTime, GamePadState gamePadState, PlayerIndex player)
    {
        foreach (var entity in world.GetEntities())
        {
            if (!HasComponents<InputConfig>(entity)) continue;
            if (!HasComponents<OpenPorts>(entity)) continue;

            //throw out calls from ports we dont care about
            ref var ports = ref GetComponent<OpenPorts>(entity);

            if(ports.port != "AcceptsAll"){
                if(player == PlayerIndex.One && ports.port != "PlayerOne") continue;
                if(player == PlayerIndex.Two && ports.port != "PlayerTwo") continue;
                if(player == PlayerIndex.Three && ports.port != "PlayerThree") continue;
                if(player == PlayerIndex.Four && ports.port != "PlayerFour") continue;
            }

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
                        HandleLeftJoyStick(entity, joystick.Direction, joystick.Threshold, left, player);
                    }

                    if (joystick.Type == JoystickType.RightStick)
                    {
                        Vector2 right = gamePadState.ThumbSticks.Right;
                        HandleRightJoyStick(entity, joystick.Direction, joystick.Threshold, right, player);
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

            if(ports.port != "AcceptsAll"){
                if(player == PlayerIndex.One && ports.port != "PlayerOne") continue;
                if(player == PlayerIndex.Two && ports.port != "PlayerTwo") continue;
                if(player == PlayerIndex.Three && ports.port != "PlayerThree") continue;
                if(player == PlayerIndex.Four && ports.port != "PlayerFour") continue;
            }

         

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

                        if (trigger == TriggerType.Left && leftTriggerValue > threshold && !pressedTriggers.Contains(TriggerType.Left))
                        {
                            pressedTriggers.Add(TriggerType.Left);
                            PublishRawInputEvent(entity, null,null,false,false,true,null,null,TriggerType.Left,null,leftTriggerValue, player);

                        }

                        if (trigger == TriggerType.Left && leftTriggerValue < threshold && pressedTriggers.Contains(TriggerType.Left))
                        {
                            // We need to register a new left trigger release                    
                            pressedTriggers.Remove(TriggerType.Left);
                            PublishRawInputEvent(entity, null, null, false, false, true, null, null, TriggerType.Left, null, leftTriggerValue, player);
                        }

                        if (trigger == TriggerType.Right && rightTriggerValue > threshold && !pressedTriggers.Contains(TriggerType.Right))
                        {
                            pressedTriggers.Add(TriggerType.Right);
                            PublishRawInputEvent(entity, null, null, false, false, true, null, null, TriggerType.Right, null, rightTriggerValue, player);

                        }

                        if (trigger == TriggerType.Right && rightTriggerValue < threshold && pressedTriggers.Contains(TriggerType.Right))
                        {
                            pressedTriggers.Remove(TriggerType.Right);
                            PublishRawInputEvent(entity, null, null, false, false, true, null, null, TriggerType.Right, null, rightTriggerValue, player);

                        }
                    }
                }
 
            }   
        }
    }
}
