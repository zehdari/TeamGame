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

        Entity? grid = null;
        foreach(var Entity in World.GetEntities())
        {
            if (HasComponents<GridTag>(Entity)){
                grid = Entity;

                break;
            }
        }

        Publish<PvZSpawnEvent>(new PvZSpawnEvent
        {
            Entity = zombieEvent.Entity,
            typeSpawned = MAGIC.SPAWNED.ZOMBIE,
            Grid = (Entity)grid,
            spawnPosition = GetSpawnPosition((Entity)grid),
            GridAssigned = false
        });

        ref var ZombieSpawningInfo = ref GetComponent<ZombieSpawningInfo>((Entity)grid);
        ZombieSpawningInfo.TimeBetweenSpawn -= ZombieSpawningInfo.TimeDecrease;
        if (ZombieSpawningInfo.TimeBetweenSpawn < ZombieSpawningInfo.MinTimeBetweenSpawn)
        {
            ZombieSpawningInfo.TimeBetweenSpawn = ZombieSpawningInfo.MinTimeBetweenSpawn;
        }

        ref var timers = ref GetComponent<Timers>(zombieEvent.Entity);

        System.Diagnostics.Debug.WriteLine("Trying to add a timer...");
        // Begin the timer, if not already existing

        System.Diagnostics.Debug.WriteLine("Adding a timer!");
        System.Diagnostics.Debug.WriteLine($"New timer is going for: {ZombieSpawningInfo.TimeBetweenSpawn}");
        
        //else
        //{
        //    System.Diagnostics.Debug.WriteLine("Editing a timer!");
        //    System.Diagnostics.Debug.WriteLine($"New timer is going for: {ZombieSpawningInfo.TimeBetweenSpawn}");
        //    timers.TimerMap[TimerType.ZombieSpawningTimer] = new Timer
        //    {
        //        Duration = ZombieSpawningInfo.TimeBetweenSpawn,
        //        Elapsed = 0f,
        //        Type = TimerType.ZombieSpawningTimer,
        //        OneShot = false,
        //    };

        //}
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

    public override void Update(World world, GameTime gameTime)
    {
      
    }
}