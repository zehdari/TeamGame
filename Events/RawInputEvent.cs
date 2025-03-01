using ECS.Components.Input;
namespace ECS.Events;


public struct RawInputEvent : IEvent
{
    public Entity Entity;
    public Keys? RawKey;
    public Buttons? RawButton;
    public bool IsGamepadInput;
    public bool IsJoystickInput;
    public JoystickType? JoystickType;
    public Vector2? JoystickValue;
    public bool IsTriggerInput;
    public TriggerType? TriggerType;
    public float? TriggerValue;
    public bool IsPressed;
}