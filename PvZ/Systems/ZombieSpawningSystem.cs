using ECS.Components.Animation;
using ECS.Components.Physics;
using ECS.Components.Projectiles;
using ECS.Components.PVZ;
using ECS.Components.Random;
using ECS.Components.Timer;
using static System.Formats.Asn1.AsnWriter;

namespace ECS.Systems.Spawning;

public class ZombieSpawningSystem : SystemBase
{
    public override void Initialize(World world)
    {
        base.Initialize(world);
        Subscribe<TimerEvent>(HandleSpawnAction);
    }

    private void HandleSpawnAction(IEvent evt)
    {
        var zombieEvent = (TimerEvent)evt;
        if (!zombieEvent.TimerType.Equals(TimerType.ZombieSpawningTimer))
        {
            return;
        }

        System.Diagnostics.Debug.WriteLine("Trying to spawn a zombie");

        Publish<PvZSpawnEvent>(new PvZSpawnEvent
        {
            Entity = zombieEvent.Entity,
            typeSpawned = MAGIC.SPAWNED.ZOMBIE,
            Grid = zombieEvent.Entity,
            spawnPosition = GetSpawnPosition(zombieEvent.Entity),
            GridAssigned = false
        });
       
        ResetTimer(zombieEvent.Entity);

        

    }
    private Vector2 GetSpawnPosition(Entity grid)
    {
        const float X = 800;

        ref var gridPosition = ref GetComponent<Position>(grid);
        ref var gridInfo = ref GetComponent<GridInfo>(grid);
        ref var scale = ref GetComponent<Scale>(grid);
        ref var random = ref GetComponent<RandomlyGeneratedInteger>(grid);

        float Y = gridPosition.Value.Y + (gridInfo.TileSize * scale.Value.Y * random.Value) - gridInfo.YOffset;

        return new Vector2(X, Y);

    }

    private void ResetTimer(Entity entity)
    {
        ref var zombieSpawningInfo = ref GetComponent<ZombieSpawningInfo>(entity);

        zombieSpawningInfo.TimeBetweenSpawn -= zombieSpawningInfo.TimeDecrease;

        if (zombieSpawningInfo.TimeBetweenSpawn < zombieSpawningInfo.MinTimeBetweenSpawn)
        {
            zombieSpawningInfo.TimeBetweenSpawn = zombieSpawningInfo.MinTimeBetweenSpawn;
        }

        Publish<UpdateTimerEvent>(new UpdateTimerEvent
        {
            Entity = entity,
            Type = TimerType.ZombieSpawningTimer,
            Timer = new Timer
            {
                Duration = zombieSpawningInfo.TimeBetweenSpawn,
                Elapsed = 0f,
                Type = TimerType.ZombieSpawningTimer,
                OneShot = true,

            }
        });
    }

    public override void Update(World world, GameTime gameTime)
    {
      
    }
}