namespace ECS.Components.Effects;

// Interface for all effect components
public interface IEffectBase
{
    float Duration { get; set; }       // Total duration
    float RemainingTime { get; set; }  // Time left
    float Magnitude { get; set; }      // Effect strength
    bool IsApplied { get; set; }       // Whether effect has been applied
}

// Enum to represent different types of effects
public enum EffectType
{
    None,
    SpeedBoost,
    JumpBoost,
    GravityReduction,
    MassChange,
    ScaleChange,
    InvincibilityShield,
    FireDamage,
    IceSlow
}

// Component to store original component values
public struct OriginalValues
{
    public Dictionary<Type, object> Values;
}