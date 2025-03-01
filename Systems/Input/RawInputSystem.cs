using ECS.Components.Input;

namespace ECS.Systems.Input;

public class RawInputSystem : SystemBase
{

    //stores keys that are pressed
    private Dictionary<Entity, HashSet<Keys>> pressedKeys = new();
    public override bool Pausible => false;
    public override void Update(World world, GameTime gameTime)
    {

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
}