namespace ECS.Core;

public interface IComponentPool
{
    void Remove(Entity entity);
    bool Has(Entity entity);
}
