namespace ECS.Core;

public enum SystemExecutionPhase
{
    Terminal,
    Input,
    PreUpdate,
    Update,
    PostUpdate,
    Render
}