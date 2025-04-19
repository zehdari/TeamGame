namespace ECS.Components.Effects;

public struct MassChangeEffect : IEffectBase
{
    public float Duration { get; set; }
    public float RemainingTime { get; set; }
    public float Magnitude { get; set; }
    public bool IsApplied { get; set; }
}