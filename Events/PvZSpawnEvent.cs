namespace ECS.Events;

public struct PvZSpawnEvent : IEvent
{
    public string typeSpawned; // What is spawning
    public Entity Entity; // Who spawned it
    public Entity Grid; // This should be the grid
    public Vector2 spawnPosition;
}