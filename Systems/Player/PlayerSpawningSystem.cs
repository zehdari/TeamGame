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
        if (!spawnEvent.typeSpawned.Equals("player"))
            return;

        var entity = spawnEvent.Entity;
        ref var position = ref GetComponent<Position>(entity);
        position.Value = new Vector2(500, 500); // Respawn player at some position for now
        Console.WriteLine($"Entity {entity} respawned at {position.Value}.");
    }

    public override void Update(World world, GameTime gameTime)
    {
        // No logic needed for PlayerDespawnSystem
    }
}