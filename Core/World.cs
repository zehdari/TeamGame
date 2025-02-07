namespace ECS.Core;

public class World
{
    private int nextEntityId = 0;
    private readonly Dictionary<Type, IComponentPool> componentPools = new();
    private readonly HashSet<Entity> entities = new();
    private readonly HashSet<Entity> entitiesToDestroy = new();
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

    private void ProcessEntityDestructions()
    {
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
        }
        entitiesToDestroy.Clear();
    }

    public void DestroyEntity(Entity entity)
    {
       if (entities.Contains(entity))
        {
            entitiesToDestroy.Add(entity);
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
        ProcessEntityDestructions();
        systemManager.UpdatePhase(SystemExecutionPhase.Input, gameTime);
        systemManager.UpdatePhase(SystemExecutionPhase.PreUpdate, gameTime);
        systemManager.UpdatePhase(SystemExecutionPhase.Update, gameTime);
        systemManager.UpdatePhase(SystemExecutionPhase.PostUpdate, gameTime);
    }

    public void Draw(GameTime gameTime)
    {
        systemManager.UpdatePhase(SystemExecutionPhase.Render, gameTime);
    }

    public HashSet<Entity> GetEntities() => entities;
}