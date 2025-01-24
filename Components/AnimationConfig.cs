namespace ECSAttempt.Components;

public struct AnimationConfig
{
    public Dictionary<string, AnimationFrameConfig[]> States;
}

public struct AnimationFrameConfig
{
    public Rectangle SourceRect;
    public float Duration;
}