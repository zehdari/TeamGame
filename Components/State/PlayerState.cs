namespace ECS.Components.State;

// Define possible player states with priority values
public enum PlayerState
{
    Idle = 0,       // Lowest priority
    Walk = 50,
    Run = 60,
    Fall = 70,
    Jump = 80,
    Block = 90,
    Attack = 100    // Highest priority
}

// Define a component to store the player's current state
public struct PlayerStateComponent
{
    public PlayerState currentState;
}