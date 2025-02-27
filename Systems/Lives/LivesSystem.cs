using ECS.Components.Lives;
namespace ECS.Systems.Lives;

public class LivesSystem : SystemBase
{
    public override void Initialize(World world)
    {
        base.Initialize(world);
        Subscribe<DespawnEvent>(HandleDespawn);
    }

    private void HandleDespawn(IEvent evt)
    {
        var despawnEvent = (DespawnEvent)evt;
        if (HasComponents<LivesCount>(despawnEvent.Entity))
        {
            ref var lives = ref GetComponent<LivesCount>(despawnEvent.Entity);
            if (lives.Lives > 0)
            {
                lives.Lives--;
                Publish(new SpawnEvent { typeSpawned = "player", Entity = despawnEvent.Entity });
            }
            else
            {
                World.DestroyEntity(despawnEvent.Entity);
            }
        }
    }

    public override void Update(World world, GameTime gameTime)
    {
        // No logic needed for LivesSystem
    }
}
