namespace ECS.Components.Input;

public struct InputConfig
{
    public Dictionary<string, InputAction> Actions;
}

public struct InputAction
{
    public Keys[] Keys;
    public Buttons[] Buttons;
    public JoystickInput[] Joysticks;
    //public JoystickType[] Joysticks;
    public TriggerType[] Triggers;
    //public TriggerInput[] Triggers;
}

public enum JoystickType
{
    LeftStick,
    RightStick
}

public enum TriggerType
{
    Left,
    Right
}

public struct JoystickInput
{
    public JoystickType Type;
    public JoystickDirection Direction;
    public float Threshold;
}

//public struct TriggerInput
//{
//    public TriggerType Type;
//    public float Threshold;
//}

public enum JoystickDirection
{
    None,
    Up,
    Down,
    Left,
    Right,
    UpLeft,
    UpRight,
    DownLeft,
    DownRight
}