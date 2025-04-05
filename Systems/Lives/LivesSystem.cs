using ECS.Components.Lives;
namespace ECS.Systems.Lives;

public class LivesSystem : SystemBase
{
    public override void Initialize(World world)
    {
        base.Initialize(world);
        Subscribe<LifeLossEvent>(HandleDespawn); // Listen for DespawnEvents
    }

    // Handles entity despawning by checking remaining lives
    private void HandleDespawn(IEvent evt)
    {
        var despawnEvent = (LifeLossEvent)evt;
        if (HasComponents<LivesCount>(despawnEvent.Entity))
        {
            ref var lives = ref GetComponent<LivesCount>(despawnEvent.Entity);
            if (lives.Lives > 0)
            {
                lives.Lives--;
                Publish(new SpawnEvent { typeSpawned = MAGIC.SPAWNED.PLAYER, Entity = despawnEvent.Entity }); // Respawn entity
            }
            else
            {
                World.DestroyEntity(despawnEvent.Entity); // Permanently remove entity if no lives remain
            }
        }
    }

    public override void Update(World world, GameTime gameTime)
    {
        // No logic needed for LivesSystem
    }
}
