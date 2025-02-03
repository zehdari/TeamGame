namespace ECS.Components;

public struct InputConfig
{
    public Dictionary<string, InputAction> Actions;
}

public struct InputAction
{
    public Keys[] Keys;
}