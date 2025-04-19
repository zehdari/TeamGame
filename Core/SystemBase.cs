using ECS.Core.Utilities;

namespace ECS.Core;

public abstract class SystemBase : ISystem
{
    protected World World { get; private set; }
    
    public virtual void Initialize(World world)
    {
        World = world;
    }

    public abstract void Update(World world, GameTime gameTime);

    protected bool HasComponents<T>(Entity entity) where T : struct
        => World.GetPool<T>().Has(entity);

    protected IEnumerable<Entity> GetEntitiesWith<T>(Entity entity) where T : struct
        => World.GetEntities().Where(e => HasComponents<T>(e));

    protected ref T GetComponent<T>(Entity entity) where T : struct
        => ref World.GetPool<T>().Get(entity);

    protected void Subscribe<T>(Action<IEvent> handler) where T : IEvent
    {
        World.EventBus.Subscribe<T>((evt) => 
        {
            if (Pausible && GameStateHelper.IsPaused(World))
                return;
                
            handler(evt);
        });
    }

    protected void Publish<T>(T evt) where T : IEvent
    {
        World.EventBus.Publish(evt);
    }

    // Controls if the system pauses when the game is paused
    public virtual bool Pausible => true;
    
    // Controls if the system uses scaled game time
    public virtual bool UseScaledGameTime => true;
}