namespace ECS.Components.State;

public enum GameState
{
    MainMenu,
    LevelSelect,
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