using ECS.Components.Physics;
using ECS.Components.SpawnPoint;

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
        var entity = spawnEvent.Entity;

        // Check if it's a player and if it has a SpawnPoint component
        if (!spawnEvent.typeSpawned.Equals("player") || !HasComponents<SpawnPoint>(entity))
            return;

        ref var position = ref GetComponent<Position>(entity);
        ref var spawnPoint = ref GetComponent<SpawnPoint>(entity);
        position.Value = spawnPoint.Value;  // Respawn at original location
    }

    public override void Update(World world, GameTime gameTime)
    {
        // No logic needed for PlayerDespawnSystem
    }
}
