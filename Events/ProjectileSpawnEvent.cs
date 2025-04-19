namespace ECS.Events;

public struct ProjectileSpawnEvent : IEvent
{
    public string typeSpawned; // What is spawning
    public Entity Entity; // Who spawned it
    public World World; // TODO this needs to die, not sure how to get around it yet
}