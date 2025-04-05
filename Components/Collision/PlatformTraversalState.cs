namespace ECS.Components.Collision;

public struct PlatformTraversalState
{
    public float LastYPosition;       // Track the character's previous Y position
    public bool WasGoingUp;           // Whether the character was moving upward last frame
    public bool JustPassedUp;         // Whether the character just passed upward through a platform
    public bool IsRequestingDropThrough; // Whether the player is pressing down to drop through platforms
    public HashSet<int> PassedThrough; // Set of platform IDs the character has passed through
}