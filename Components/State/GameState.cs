namespace ECS.Components.State;

public enum GameState
{
    MainMenu,
    Running,
    Paused,
    Reset,
    Exit
}

public struct GameStateComponent
{
    public GameState CurrentState;
}