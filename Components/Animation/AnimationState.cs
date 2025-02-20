namespace ECS.Components.Animation;

public struct AnimationState
{
    public string CurrentState;
    public float TimeInFrame;
    public int FrameIndex;
    public bool IsPlaying;
}