namespace ECS.Components.UI;

public struct TerminalComponent
{
    public bool IsActive;                   // Whether the terminal is currently visible/active
    public string CurrentInput;             // Current user input string
    public List<string> History;            // Command history
    public int HistoryIndex;                // Current position in command history
    public List<string> OutputLines;        // Lines of output text
    public int ScrollPosition;              // For scrolling through output
    public int MaxOutputLines;              // Maximum number of output lines to store
    public float BackgroundOpacity;         // Opacity of terminal background
}