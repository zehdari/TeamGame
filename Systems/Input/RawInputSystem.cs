using ECS.Components.Input;

namespace ECS.Systems.Input;

public class RawInputSystem : SystemBase
{

    //stores keys that are pressed
    private Dictionary<Entity, HashSet<Keys>> pressedKeys = new();
    private Dictionary<Entity, HashSet<Buttons>> pressedButtons = new();
    // Dictionary to store the previous trigger values for each entity
    private Dictionary<Entity, HashSet<TriggerType>> pressedTriggerList = new();



    public override bool Pausible => false;
    public override void Update(World world, GameTime gameTime)
    {
        HandleKeys(world, gameTime);
        if(GamePad.GetState(PlayerIndex.One).IsConnected){
            var gamePadState = GamePad.GetState(PlayerIndex.One);
            HandleGamePad(world, gameTime, gamePadState);
            HandleJoyStick(world, gameTime, gamePadState);
            HandleTriggers(world, gameTime, gamePadState);
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
        Console.WriteLine("Controller 1 connected!");

        foreach (var entity in world.GetEntities())
    {
        // Check if the entity can process input
        if (!HasComponents<InputConfig>(entity)) continue;

        // Ensure entity has an entry in pressedButtons
        if (!pressedButtons.ContainsKey(entity))
            pressedButtons[entity] = new HashSet<Buttons>();

        // Get input configuration
        ref var config = ref GetComponent<InputConfig>(entity);
        var allButtons = config.Actions.Values.SelectMany(a => a.Buttons).Distinct();

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

    private void HandleJoyStick(World world, GameTime gameTime, GamePadState gamePadState){
        //TODO
    }

    private void HandleTriggers(World world, GameTime gameTime, GamePadState gamePadState){
        
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
                foreach (var trigger in action.Triggers)
                {
                    if(trigger.Type ==TriggerType.Left && leftTriggerValue > trigger.Threshold && !pressedTriggers.Contains(TriggerType.Left)){
                        // We need to register a new left trigger press
                    }

                    if(trigger.Type == TriggerType.Left && leftTriggerValue < trigger.Threshold && pressedTriggers.Contains(TriggerType.Left)){
                        // We need to register a new left trigger release
                    }        

                    if(trigger.Type == TriggerType.Right && rightTriggerValue > trigger.Threshold && !pressedTriggers.Contains(TriggerType.Right)){
                        // We need to register a new right trigger press
                    }

                    if(trigger.Type == TriggerType.Right && rightTriggerValue < trigger.Threshold && pressedTriggers.Contains(TriggerType.Right)){
                        // We need to register a new right trigger release
                    }
                }
            }   
        }
    }
}
