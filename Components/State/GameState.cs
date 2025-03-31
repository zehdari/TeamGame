namespace ECS.Components.State;

public enum GameState
{
    Running,
    Paused,
    Reset,
    Exit
}

public struct GameStateComponent
{
    public GameState CurrentState;
}