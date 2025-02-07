namespace ECS.Components.State;

// Define possible player states in an enum
public enum PlayerState
{
    Idle,
    Walk,
    Jump,
    Fall,
    Attack,
    Run,
    Block
}

// Define a component to store the player's current state
public struct PlayerStateComponent
{
    public PlayerState currentState;
}
