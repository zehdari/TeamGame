namespace ECS.Core;
using ECS.Systems.Debug;

public class World
{
    private int nextEntityId = 0;
    private readonly Stack<int> recycledEntityIds = new();
    private readonly Dictionary<Type, IComponentPool> componentPools = new();
    private readonly HashSet<Entity> entities = new();
    private readonly HashSet<Entity> entitiesToDestroy = new();
    private readonly SystemManager systemManager;
    public EntityFactory entityFactory { get; }
    public EventBus EventBus { get; } = new();
    
    // Time scaling properties
    private float timeScale = 1.0f;
    private GameTime scaledGameTime;
    
    public bool ProfilingEnabled
    {
        get => systemManager.ProfilingEnabled;
        set => systemManager.ProfilingEnabled = value;
    }

    public World()
    {
        systemManager = new SystemManager(this);
        entityFactory = new EntityFactory(this);
        scaledGameTime = new GameTime();
    }

    // Method to set time scale
    internal void SetTimeScale(float scale)
    {
        timeScale = Math.Clamp(scale, 0.1f, 3.0f);
    }
    
    // Method to get current time scale
    public float GetTimeScale()
    {
        return timeScale;
    }
    
    // Method to get scaled game time
    public GameTime GetScaledGameTime(GameTime originalTime)
    {
        scaledGameTime = new GameTime(
            originalTime.TotalGameTime,
            TimeSpan.FromTicks((long)(originalTime.ElapsedGameTime.Ticks * timeScale)),
            originalTime.IsRunningSlowly
        );
        
        return scaledGameTime;
    }

    public Entity CreateEntity()
    {
        int id;
        if (recycledEntityIds.Count > 0)
        {
            // Reuse an ID from the recycled pool
            id = recycledEntityIds.Pop();
        }
        else
        {
            // No recycled ID available, so use a new one
            id = nextEntityId++;
        }
        var entity = new Entity(id);
        entities.Add(entity);
        return entity;
    }

    public void DestroyEntity(Entity entity)
    {
        if (entities.Contains(entity))
        {
            entitiesToDestroy.Add(entity);
        }
    }

    private void ProcessEntityDestructions()
    {
        if (entitiesToDestroy.Count == 0)
            return;

        foreach (var entity in entitiesToDestroy)
        {
            if (!entities.Remove(entity))
            {
                continue; // Entity doesn't exist
            }

            // Remove from all component pools
            foreach (var pool in componentPools.Values)
            {
                pool.Remove(entity);
            }
            
            // Recycle the entity ID for later use
            recycledEntityIds.Push(entity.Id);
        }
        entitiesToDestroy.Clear();
    }

    public ComponentPool<T> GetPool<T>() where T : struct
    {
        var type = typeof(T);
        if (!componentPools.TryGetValue(type, out var pool))
        {
            pool = new ComponentPool<T>();
            componentPools[type] = pool;
        }
        return (ComponentPool<T>)pool;
    }

    public void AddSystem(ISystem system, SystemExecutionPhase phase, int priority = 0)
    {
        systemManager.AddSystem(system, phase, priority);
    }

    public void Update(GameTime gameTime)
    {
        // Pre-calculate scaled time once
        GameTime scaledTime = GetScaledGameTime(gameTime);
        
        // Store both times in a tuple for the SystemManager to use
        var timePair = (original: gameTime, scaled: scaledTime);
        
        systemManager.UpdatePhase(SystemExecutionPhase.Terminal, timePair);
        systemManager.UpdatePhase(SystemExecutionPhase.Input, timePair);
        systemManager.UpdatePhase(SystemExecutionPhase.PreUpdate, timePair);
        systemManager.UpdatePhase(SystemExecutionPhase.Update, timePair);
        systemManager.UpdatePhase(SystemExecutionPhase.PostUpdate, timePair);
        ProcessEntityDestructions();
    }

    public void Draw(GameTime gameTime, GraphicsManager graphicsManager)
    {
        // Pre-calculate scaled time once for drawing
        GameTime scaledTime = GetScaledGameTime(gameTime);
        var timePair = (original: gameTime, scaled: scaledTime);
        
        graphicsManager.graphicsDevice.Clear(Color.CornflowerBlue);
        
        graphicsManager.spriteBatch.Begin(
            sortMode: SpriteSortMode.FrontToBack,
            samplerState: SamplerState.PointClamp,
            transformMatrix: graphicsManager.GetTransformMatrix()
        );
        
        systemManager.UpdatePhase(SystemExecutionPhase.Render, timePair);
        
        graphicsManager.spriteBatch.End();
        
        // Draw terminal on top of everything else
        graphicsManager.spriteBatch.Begin(
            sortMode: SpriteSortMode.Immediate,
            samplerState: SamplerState.LinearClamp
        );
        
        // Find the terminal system and call its Draw method
        foreach (var system in systemManager.GetAllSystems())
        {
            if (system is TerminalSystem terminalSystem)
            {
                // Use original time for terminal
                terminalSystem.Draw(gameTime);
                break;
            }
        }
        
        graphicsManager.spriteBatch.End();
    }

    public HashSet<Entity> GetEntities() => entities;

    public Entity GetEntityById(int id)
    {
        return entities.FirstOrDefault(e => e.Id == id);
    }

    public Dictionary<Type, object> GetEntityComponents(Entity entity)
    {
        var result = new Dictionary<Type, object>();
        // Iterate over every registered component pool.
        foreach (var kv in componentPools)
        {
            // Check if this pool has the component for the entity.
            if (kv.Value.Has(entity))
            {
                // Use reflection to get the "Get" method.
                MethodInfo getMethod = kv.Value.GetType().GetMethod(MAGIC.METHODTYPES.GET);
                // Invoke the method on the pool; this will box the returned component.
                object component = getMethod.Invoke(kv.Value, new object[] { entity });
                result[kv.Key] = component;
            }
        }
        return result;
    }
}