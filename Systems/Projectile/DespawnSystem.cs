using ECS.Components.AI;
using ECS.Components.Tags;
using ECS.Components.Timer;

namespace ECS.Systems.Projectile;

public class DespawnSystem : SystemBase
{
    private Stack<Entity> despawners = new();

    public override void Initialize(World world)
    {
        base.Initialize(world);
        Subscribe<DespawnEvent>(HandleDespawn);
    }

    private void HandleDespawn(IEvent evt)
    {
        var despawnEvent = (DespawnEvent)evt;
        despawners.Push(despawnEvent.Entity);
    }

    public override void Update(World world, GameTime gameTime)
    {
        while (despawners.Count > 0)
        {
            world.DestroyEntity(despawners.Pop());
        }
    }
}

