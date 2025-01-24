namespace ECS.Core;

public enum SystemExecutionPhase
{
    Input,
    PreUpdate,
    Update,
    PostUpdate,
    Render
}