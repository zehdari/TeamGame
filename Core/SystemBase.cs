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

    public virtual bool Pausible => true;
}