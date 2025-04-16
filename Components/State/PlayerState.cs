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
    down_special = 101,
    up_special = 102,
    right_special = 103,
    left_special = 104,
    up_jab = 105,
    down_jab = 106,
    right_jab = 107,
    left_jab = 108,
    Stunned = 110  // Highest priority
}

// Define a component to store the player's current state
public struct PlayerStateComponent
{
    public PlayerState CurrentState;
}