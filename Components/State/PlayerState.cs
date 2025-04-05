namespace ECS.Components.State;

// Define possible player states with priority values
public enum PlayerState
{
    Idle = 0,       // Lowest priority
    Fall = 20,
    Walk = 50,
    Run = 60,
    Jump = 80,
    Block = 90,
    Shoot = 95,
    Attack = 100,
    Stunned = 110  // Highest priority
}

// Define a component to store the player's current state
public struct PlayerStateComponent
{
    public PlayerState CurrentState;
}