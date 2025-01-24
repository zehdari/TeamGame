namespace ECSAttempt.Core;

public class SystemInfo
{
    public ISystem System { get; }
    public SystemExecutionPhase Phase { get; }
    public int Priority { get; }  // Higher priority systems run first within their phase

    public SystemInfo(ISystem system, SystemExecutionPhase phase, int priority = 0)
    {
        System = system;
        Phase = phase;
        Priority = priority;
    }
}