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
        HandleKeys(world, gameTime);

            if (GamePad.GetState(PlayerIndex.One).IsConnected)
            {

                 //System.Diagnostics.Trace.WriteLine("Checking controllers...");
               var gamePadState = GamePad.GetState(PlayerIndex.One);
                //var leftValueDebug = gamePadState.ThumbSticks.Left;
               // System.Diagnostics.Trace.WriteLine($"Left Thumbstick X: {leftValueDebug.X}, Y: {leftValueDebug.Y}");
                HandleGamePad(world, gameTime, gamePadState);
                //HandleJoyStick(world, gameTime, gamePadState);
                HandleTriggers(world, gameTime, gamePadState);
                HandleJoyStick(world, gameTime, gamePadState);
            }
     }


    private void HandleKeys(World world, GameTime gameTime){
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
                    Publish(new RawInputEvent
                    {
                        Entity = entity,
                        RawKey = key,
                        RawButton = null,
                        IsGamepadInput = false,
                        IsJoystickInput = false,
                        JoystickType = null,
                        JoystickValue = null,
                        IsTriggerInput = false,
                        TriggerType = null,
                        TriggerValue = null,
                        IsPressed = true,
                    });
                }
                else if (KeyState.IsKeyUp(key) && PressedKeyList.Contains(key))
                {
                    // remove key from pressedKeys
                    pressedKeys[entity].Remove(key);

                    //publish event for picking up key
                    Publish(new RawInputEvent
                    {
                        Entity = entity,
                        RawKey = key,
                        RawButton = null,
                        IsGamepadInput = false,
                        IsJoystickInput = false,
                        JoystickType = null,
                        JoystickValue = null,
                        IsTriggerInput = false,
                        TriggerType = null,
                        TriggerValue = null,
                        IsPressed = false,
                    });
                }
            }

        }

    }

        
    private void HandleGamePad(World world, GameTime gameTime, GamePadState gamePadState){

        foreach (var entity in world.GetEntities())
    {
        // Check if the entity can process input
        if (!HasComponents<InputConfig>(entity)) continue;

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
                Publish(new RawInputEvent
                {
                        Entity = entity,
                        RawKey = null,
                        RawButton = button,
                        IsGamepadInput = true,
                        IsJoystickInput = false,
                        JoystickType = null,
                        JoystickValue = null,
                        IsTriggerInput = false,
                        TriggerType = null,
                        TriggerValue = null,
                        IsPressed = true,
                });
            }
            else if (gamePadState.IsButtonUp(button) && pressedButtonList.Contains(button))
            {
                // Remove button from pressed list
                pressedButtonList.Remove(button);
                Publish(new RawInputEvent
                {
                        Entity = entity,
                        RawKey = null,
                        RawButton = button,
                        IsGamepadInput = true,
                        IsJoystickInput = false,
                        JoystickType = null,
                        JoystickValue = null,
                        IsTriggerInput = false,
                        TriggerType = null,
                        TriggerValue = null,
                        IsPressed = false,
                });
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

    private void HandleLeftJoyStick(Entity entity, JoystickDirection direction, float threshold, Vector2 leftJoystickValue)
    {

        var joystickDirection = GetLeftJoyStickDirection(leftJoystickValue, threshold);

        // direction has changed so make an event
        if ((joystickDirection == direction && leftDirection[entity] == JoystickDirection.None))
        {
            // new direction, is pressed should be true
            Publish(new RawInputEvent
            {
                Entity = entity,
                IsGamepadInput = false,
                IsJoystickInput = true,
                JoystickType = JoystickType.LeftStick,
                JoystickValue = leftJoystickValue,
                JoystickDirection = joystickDirection,
                IsTriggerInput = false,
                IsPressed = true,
            });
        }
        else if (joystickDirection != direction && leftDirection[entity] != JoystickDirection.None)
            {
                Publish(new RawInputEvent
                {
                    Entity = entity,
                    IsGamepadInput = false,
                    IsJoystickInput = true,
                    JoystickType = JoystickType.LeftStick,
                    JoystickValue = leftJoystickValue,
                    JoystickDirection = joystickDirection,
                    IsTriggerInput = false,
                    IsPressed = false,
                });
            }

            leftDirection[entity] = joystickDirection;
    
    }

    private void HandleRightJoyStick(Entity entity, JoystickDirection direction, float threshold, Vector2 rightJoystickValue)
    {

        var joystickDirection = GetRightJoyStickDirection(rightJoystickValue, threshold);

            // direction has changed so make an event
            if (joystickDirection == direction && rightDirection[entity] == JoystickDirection.None)
            {
                // new direction, is pressed should be true
                Publish(new RawInputEvent
                {
                    Entity = entity,
                    IsGamepadInput = false,
                    IsJoystickInput = true,
                    JoystickType = JoystickType.RightStick,
                    JoystickValue = rightJoystickValue,
                    JoystickDirection = joystickDirection,
                    IsTriggerInput = false,
                    IsPressed = true,
                });
            }
            else if(joystickDirection != direction && rightDirection[entity] != JoystickDirection.None)
            {
                Publish(new RawInputEvent
                {
                    Entity = entity,
                    IsGamepadInput = false,
                    IsJoystickInput = true,
                    JoystickType = JoystickType.RightStick,
                    JoystickValue = rightJoystickValue,
                    JoystickDirection = joystickDirection,
                    IsTriggerInput = false,
                    IsPressed = false,
                });
            }

            rightDirection[entity] = joystickDirection;
      
    }

    private void HandleJoyStick(World world, GameTime gameTime, GamePadState gamePadState)
    {
        foreach (var entity in world.GetEntities())
        {
            if (!HasComponents<InputConfig>(entity)) continue;

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
                        HandleLeftJoyStick(entity, joystick.Direction, joystick.Threshold, left);
                    }

                    if (joystick.Type == JoystickType.RightStick)
                    {
                        Vector2 right = gamePadState.ThumbSticks.Right;
                        HandleRightJoyStick(entity, joystick.Direction, joystick.Threshold, right);
                    }
                }

            }
        }

    }

    private void HandleTriggers(World world, GameTime gameTime, GamePadState gamePadState){
        float threshold = 0.5f;

        foreach (var entity in world.GetEntities())
        {
            if (!HasComponents<InputConfig>(entity)) continue;

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
                            Publish(new RawInputEvent
                            {
                                Entity = entity,
                                RawKey = null,
                                RawButton = null,
                                IsGamepadInput = false,
                                IsJoystickInput = false,
                                JoystickType = null,
                                JoystickValue = null,
                                IsTriggerInput = true,
                                TriggerType = TriggerType.Left,
                                TriggerValue = leftTriggerValue,
                                IsPressed = true,
                            });
                        }

                        if (trigger == TriggerType.Left && leftTriggerValue < threshold && pressedTriggers.Contains(TriggerType.Left))
                        {
                            // We need to register a new left trigger release                    
                            pressedTriggers.Remove(TriggerType.Left);
                            Publish(new RawInputEvent
                            {
                                Entity = entity,
                                RawKey = null,
                                RawButton = null,
                                IsGamepadInput = false,
                                IsJoystickInput = false,
                                JoystickType = null,
                                JoystickValue = null,
                                IsTriggerInput = true,
                                TriggerType = TriggerType.Left,
                                TriggerValue = leftTriggerValue,
                                IsPressed = false,
                            });
                        }

                        if (trigger == TriggerType.Right && rightTriggerValue > threshold && !pressedTriggers.Contains(TriggerType.Right))
                        {
                            pressedTriggers.Add(TriggerType.Right);
                            Publish(new RawInputEvent
                            {
                                Entity = entity,
                                RawKey = null,
                                RawButton = null,
                                IsGamepadInput = false,
                                IsJoystickInput = false,
                                JoystickType = null,
                                JoystickValue = null,
                                IsTriggerInput = true,
                                TriggerType = TriggerType.Right,
                                TriggerValue = rightTriggerValue,
                                IsPressed = true,
                            });
                        }

                        if (trigger == TriggerType.Right && rightTriggerValue < threshold && pressedTriggers.Contains(TriggerType.Right))
                        {
                            pressedTriggers.Remove(TriggerType.Right);
                            Publish(new RawInputEvent
                            {
                                Entity = entity,
                                RawKey = null,
                                RawButton = null,
                                IsGamepadInput = false,
                                IsJoystickInput = false,
                                JoystickType = null,
                                JoystickValue = null,
                                IsTriggerInput = true,
                                TriggerType = TriggerType.Right,
                                TriggerValue = rightTriggerValue,
                                IsPressed = false,
                            });
                        }
                    }
                }
 
            }   
        }
    }
}
