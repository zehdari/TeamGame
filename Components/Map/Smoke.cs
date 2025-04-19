namespace ECS.Components.Objects;

public enum SmokeState
{
    Hidden,
    NightChimney,
    Chimney
}

public struct Smoke
{
    public SmokeState CurrentState;
    public float ChimneyDuration;
    public float NightChimneyDuration;
    public float HiddenDuration;
    public Vector2 ForceToApply;
}