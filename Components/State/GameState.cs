namespace ECS.Components.State;

public enum GameState
{
    MainMenu,
    LevelSelect,
    CharacterSelect,
    Running,
    Paused,
    Terminal,
    Reset,
    Exit,
    Win
}

public struct GameStateComponent
{
    public GameState CurrentState;
}