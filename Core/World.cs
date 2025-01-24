public class World
{
    private int nextEntityId = 0;
    private readonly Dictionary<Type, object> componentPools = new();
    private readonly HashSet<Entity> entities = new();
    private readonly SystemManager systemManager;
    
    public EventBus EventBus { get; } = new();

    public World()
    {
        systemManager = new SystemManager(this);
    }

    public Entity CreateEntity()
    {
        var entity = new Entity(nextEntityId++);
        entities.Add(entity);
        return entity;
    }

    public void DestroyEntity(Entity entity)
    {
        if (!entities.Remove(entity))
        {
            return; // Entity doesn't exist
        }

        // Remove from all component pools
        foreach (var pool in componentPools.Values)
        {
            var removeMethod = pool.GetType().GetMethod("Remove");
            removeMethod?.Invoke(pool, new object[] { entity });
        }
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
        systemManager.Update(gameTime);
    }

    public HashSet<Entity> GetEntities() => entities;
}