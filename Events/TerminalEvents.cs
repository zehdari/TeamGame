
namespace ECS.Events;

// Event for toggling the terminal on/off
public struct TerminalToggleEvent : IEvent
{
    public Entity Entity;
}

// Event for when a command is executed in the terminal
public struct TerminalCommandEvent : IEvent
{
    public Entity Entity;
    public string Command;
    public string[] Args;
}