namespace ECS.Components.State;

public enum GameState
{
    MainMenu,
    Running,
    Paused,
    Terminal,
    Reset,
    Exit
}

public struct GameStateComponent
{
    public GameState CurrentState;
}