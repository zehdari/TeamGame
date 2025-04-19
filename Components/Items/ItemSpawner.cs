using ECS.Components.Timer;

namespace ECS.Components.Items;

public struct ItemSpawnerComponent
{
    // Dictionary of item keys to their spawn weights
    public Dictionary<string, int> SpawnableItems;
    
    // Probability (0.0f to 1.0f) of spawning an item when timer completes
    public float SpawnChance;
    
    // Maximum number of items this spawner can have active at once
    public int MaxItemsAllowed;
    
    // Time between spawn attempts in seconds
    public float SpawnInterval;
    
    // Time an item lives before despawning in seconds
    public float ItemLifetime;
    
    // How far items can spawn from the center of the spawner
    public Vector2 SpawnAreaSize;
    
    // Optional tag to track which items came from this spawner
    public string SpawnerTag;
}