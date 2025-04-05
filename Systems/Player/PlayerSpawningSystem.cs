using ECS.Components.Physics;

namespace ECS.Systems.Player;

public class PlayerSpawningSystem : SystemBase
{
    public override void Initialize(World world)
    {
        base.Initialize(world);
        Subscribe<SpawnEvent>(HandleSpawn); // Listen for spawn events
    }

    // Handles the spawning of player entities
    private void HandleSpawn(IEvent evt)
    {
        var spawnEvent = (SpawnEvent)evt;
        if (!spawnEvent.typeSpawned.Equals(MAGIC.SPAWNED.PLAYER))
            return;

        var entity = spawnEvent.Entity;
        ref var position = ref GetComponent<Position>(entity);
        ref var velocity = ref GetComponent<Velocity>(entity);
        ref var percent = ref GetComponent<Percent>(entity);
        position.Value = new Vector2(400, 100); // Respawn player at some position for now
        velocity.Value = Vector2.Zero;
        percent.Value = 0f;
    }

    public override void Update(World world, GameTime gameTime)
    {
        // No logic needed for PlayerDespawnSystem
    }
}