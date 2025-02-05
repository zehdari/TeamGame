namespace ECS.Components.State;

// Define possible player states in an enum
public enum PlayerState
{
    Idle,
    Walk,
    Jump,
    Fall,
    Attack, //How do we check for this?
    Run, //maybe. See if velocity is greater than some scalar value?
    Block, //maybe. How do we check for this?
}

// Define a component to store the player's current state
public struct PlayerStateComponent
{
    public PlayerState currentState;
}
