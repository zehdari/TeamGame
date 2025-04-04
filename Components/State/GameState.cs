namespace ECS.Components.State;

public enum GameState
{
    MainMenu,
    LevelSelect,
    Running,
    Paused,
    Reset,
    Exit
}

public struct GameStateComponent
{
    public GameState CurrentState;
}