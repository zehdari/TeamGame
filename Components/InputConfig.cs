namespace ECSAttempt.Components;

public struct InputConfig
{
    public Dictionary<string, InputAction> Actions;
}

public struct InputAction
{
    public Keys[] Keys;
    public string Axis;
    public float Value;
}