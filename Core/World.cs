namespace ECS.Core;

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

    public World()
    {
        systemManager = new SystemManager(this);
        entityFactory = new EntityFactory(this);
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
        systemManager.UpdatePhase(SystemExecutionPhase.Terminal, gameTime);
        systemManager.UpdatePhase(SystemExecutionPhase.Input, gameTime);
        systemManager.UpdatePhase(SystemExecutionPhase.PreUpdate, gameTime);
        systemManager.UpdatePhase(SystemExecutionPhase.Update, gameTime);
        systemManager.UpdatePhase(SystemExecutionPhase.PostUpdate, gameTime);
        ProcessEntityDestructions();
    }

    public void Draw(GameTime gameTime, GraphicsManager graphicsManager)
    {
        graphicsManager.graphicsDevice.Clear(Color.CornflowerBlue);
        
        graphicsManager.spriteBatch.Begin(
            sortMode: SpriteSortMode.FrontToBack,
            samplerState: SamplerState.PointClamp,
            transformMatrix: graphicsManager.GetTransformMatrix()
        );
        
        systemManager.UpdatePhase(SystemExecutionPhase.Render, gameTime);
        
        graphicsManager.spriteBatch.End();
        
        // Draw terminal on top of everything else
        graphicsManager.spriteBatch.Begin(
            sortMode: SpriteSortMode.Immediate,
            samplerState: SamplerState.LinearClamp
        );
        
        // Find the terminal system and call its Draw method
        foreach (var system in systemManager.GetAllSystems())
        {
            if (system is Systems.UI.TerminalSystem terminalSystem)
            {
                terminalSystem.Draw(gameTime);
                break;
            }
        }
        
        graphicsManager.spriteBatch.End();
    }

    public HashSet<Entity> GetEntities() => entities;
}