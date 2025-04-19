using ECS.Components.Animation;
using ECS.Components.Collision;
using ECS.Components.Items;
using ECS.Components.Physics;
using ECS.Components.Timer;
using ECS.Components.Tags;
using ECS.Components.State;
using ECS.Core;

namespace ECS.Systems.Spawning;

public class ItemSpawningSystem : SystemBase
{
    private EntityFactory entityFactory;
    private readonly GameAssets assets;
    private readonly Random random = new();
    private Dictionary<Entity, HashSet<Entity>> spawnerItemsMap = new();

    public ItemSpawningSystem(GameAssets assets)
    {
        this.assets = assets;
    }

    public override void Initialize(World world)
    {
        base.Initialize(world);
        entityFactory = world.entityFactory;
    }

    // Method to spawn an item at a specific position from a specific spawner
    public Entity SpawnItem(Entity spawner, Vector2 position, string itemKey)
    {
        // Create the entity
        var entity = entityFactory.CreateEntityFromKey(itemKey, assets);
        
        if (entity.Id < 0)  // Invalid entity
            return entity;
        
        // Set its position
        if (HasComponents<Position>(entity))
        {
            ref var pos = ref GetComponent<Position>(entity);
            pos.Value = position;
        }
        
        // Add the ItemTag component to identify this as an item
        if (!HasComponents<ItemTag>(entity))
        {
            World.GetPool<ItemTag>().Set(entity, new ItemTag());
        }
        
        // Add or update the timer component for the item
        if (HasComponents<ItemSpawnerComponent>(spawner))
        {
            ref var spawnerComp = ref GetComponent<ItemSpawnerComponent>(spawner);
            float lifetime = spawnerComp.ItemLifetime;
            
            if (!HasComponents<Timers>(entity))
            {
                World.GetPool<Timers>().Set(entity, new Timers
                {
                    TimerMap = new Dictionary<TimerType, Timer>
                    {
                        {
                            TimerType.ItemTimer, new Timer
                            {
                                Duration = lifetime,
                                Elapsed = 0f,
                                Type = TimerType.ItemTimer,
                                OneShot = true
                            }
                        }
                    }
                });
            }
            else
            {
                ref var timers = ref GetComponent<Timers>(entity);
                if (timers.TimerMap == null)
                {
                    timers.TimerMap = new Dictionary<TimerType, Timer>();
                }
                
                timers.TimerMap[TimerType.ItemTimer] = new Timer
                {
                    Duration = lifetime,
                    Elapsed = 0f,
                    Type = TimerType.ItemTimer,
                    OneShot = true
                };
            }
        }
        
        // Keep track of which spawner created this item
        if (!spawnerItemsMap.ContainsKey(spawner))
        {
            spawnerItemsMap[spawner] = new HashSet<Entity>();
        }
        spawnerItemsMap[spawner].Add(entity);
        
        return entity;
    }

    private string SelectRandomItem(Dictionary<string, int> itemWeights)
    {
        if (itemWeights == null || itemWeights.Count == 0)
            return null;
            
        // Calculate the sum of weights
        int totalWeight = itemWeights.Values.Sum();
        
        // Generate a random number between 0 and totalWeight
        int randomNumber = random.Next(totalWeight);
        
        // Find which item corresponds to the random number
        int cumulativeWeight = 0;
        foreach (var pair in itemWeights)
        {
            cumulativeWeight += pair.Value;
            if (randomNumber < cumulativeWeight)
            {
                return pair.Key;
            }
        }
        
        // Fallback to the first item (should never occur with proper weights)
        return itemWeights.Keys.First();
    }

    private int CountItemsFromSpawner(Entity spawner)
    {
        if (spawnerItemsMap.TryGetValue(spawner, out var items))
        {
            // Use ToList() to avoid modifying the collection during enumeration
            var itemsToRemove = items.Where(item => !World.GetEntities().Contains(item)).ToList();
            foreach (var item in itemsToRemove)
            {
                items.Remove(item);
            }
            return items.Count;
        }
        return 0;
    }

    private Vector2 GetRandomPositionInSpawnArea(Entity spawner)
    {
        if (!HasComponents<Position>(spawner) || !HasComponents<ItemSpawnerComponent>(spawner))
            return Vector2.Zero;
            
        ref var pos = ref GetComponent<Position>(spawner);
        ref var spawnerComp = ref GetComponent<ItemSpawnerComponent>(spawner);
        
        // Get a random position within the spawn area
        float halfWidth = spawnerComp.SpawnAreaSize.X / 2;
        float halfHeight = spawnerComp.SpawnAreaSize.Y / 2;
        
        float offsetX = ((float)random.NextDouble() * 2 - 1) * halfWidth;
        float offsetY = ((float)random.NextDouble() * 2 - 1) * halfHeight;
        
        return new Vector2(
            pos.Value.X + offsetX,
            pos.Value.Y + offsetY
        );
    }

    public override void Update(World world, GameTime gameTime)
    {
        // Check if the game is running
        bool gameIsRunning = IsGameRunning();
        if (!gameIsRunning)
            return;
        
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        
        // Get a copy of entities to avoid modification issues during iteration
        var entities = world.GetEntities().ToList();
        
        // Process each spawner
        foreach (var entity in entities)
        {
            if (!HasComponents<ItemSpawnerComponent>(entity) || !HasComponents<Timers>(entity))
                continue;
                
            ref var spawnerComp = ref GetComponent<ItemSpawnerComponent>(entity);
            ref var timers = ref GetComponent<Timers>(entity);
            
            if (timers.TimerMap == null)
            {
                timers.TimerMap = new Dictionary<TimerType, Timer>();
                continue;
            }
            
            // Count existing items from this spawner
            int currentItemCount = CountItemsFromSpawner(entity);
            
            // Skip this spawner if at max capacity
            if (currentItemCount >= spawnerComp.MaxItemsAllowed)
                continue;
            
            // Process the timer for this spawner
            if (timers.TimerMap.TryGetValue(TimerType.SpecialTimer, out var timer))
            {
                // Update the timer
                timer.Elapsed += deltaTime;
                
                if (timer.Elapsed >= timer.Duration)
                {
                    // Check if we should spawn an item based on chance
                    if (random.NextDouble() < spawnerComp.SpawnChance)
                    {
                        // Select an item to spawn
                        string itemKey = SelectRandomItem(spawnerComp.SpawnableItems);
                        
                        if (!string.IsNullOrEmpty(itemKey))
                        {
                            // Get a random position in the spawn area
                            Vector2 spawnPosition = GetRandomPositionInSpawnArea(entity);
                            
                            // Spawn the item
                            SpawnItem(entity, spawnPosition, itemKey);
                        }
                    }
                    
                    // Reset the timer
                    timer.Elapsed = 0f;
                    timers.TimerMap[TimerType.SpecialTimer] = timer;
                }
                else
                {
                    // Update the timer
                    timers.TimerMap[TimerType.SpecialTimer] = timer;
                }
            }
            else
            {
                // Create the timer if it doesn't exist
                timers.TimerMap[TimerType.SpecialTimer] = new Timer
                {
                    Duration = spawnerComp.SpawnInterval,
                    Elapsed = 0f,
                    Type = TimerType.SpecialTimer,
                    OneShot = false
                };
            }
        }
        
        // Clean up spawner item map
        CleanupItemMap();
    }
    
    private bool IsGameRunning()
    {
        // Find the entity with GameStateComponent
        foreach (var entity in World.GetEntities())
        {
            if (HasComponents<GameStateComponent>(entity))
            {
                ref var gameState = ref GetComponent<GameStateComponent>(entity);
                return gameState.CurrentState == GameState.Running;
            }
        }
        return false;
    }
    
    private void CleanupItemMap()
    {
        // Create a list of spawners to remove
        var spawnersToRemove = new List<Entity>();
        
        foreach (var spawner in spawnerItemsMap.Keys.ToList())
        {
            // Check if spawner is still valid
            if (!World.GetEntities().Contains(spawner))
            {
                spawnersToRemove.Add(spawner);
                continue;
            }
            
            // Remove destroyed items
            var items = spawnerItemsMap[spawner];
            var itemsToRemove = items.Where(item => !World.GetEntities().Contains(item)).ToList();
            foreach (var item in itemsToRemove)
            {
                items.Remove(item);
            }
        }
        
        // Remove invalid spawners
        foreach (var spawner in spawnersToRemove)
        {
            spawnerItemsMap.Remove(spawner);
        }
    }
}