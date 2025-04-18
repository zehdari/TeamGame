using ECS.Components.Animation;
using ECS.Components.Physics;
using ECS.Components.Projectiles;
using ECS.Components.PVZ;
using ECS.Components.Random;
using ECS.Components.Tags;

namespace ECS.Systems.Spawning;

public class PvZDeathMonitoringSystem : SystemBase
{
    // Everything has the same health for now
    private const int DEATH_PERCENT_LIMIT = 100;

    public override void Initialize(World world)
    {
        base.Initialize(world);
    }
    public override void Update(World world, GameTime gameTime)
    {
        
       foreach(var entity in World.GetEntities())
        {
            if (!HasComponents<PvZTag>(entity))
            {
                //Logger.Log($"Entity id: {entity.Id} did not have PvZTag");
                continue;
            }

            Entity? grid = null;
            try
            {
                grid = World.GetEntities().First(grid => HasComponents<GridTag>(grid));
               // Logger.Log($"Got grid: {entity.Id}");
            }
            catch (Exception ex)
            {
                Logger.Log($"Couldn't find grid: {entity.Id}");
                return;
            }

            if (!HasComponents<Percent>(entity)) return;
            ref var percent = ref GetComponent<Percent>(entity);


            if(percent.Value >= DEATH_PERCENT_LIMIT)
            {
                Logger.Log($"Trying to despawn entity: {entity.Id}");
                Publish<PvZDeathEvent>(new PvZDeathEvent
                {
                    Entity = entity,
                    Grid = (Entity)grid,
                });
            } else
            {
                if(HasComponents<PlayerTag>(entity))
                Logger.Log($"Percent wasn't above limit: {percent.Value}, ID: {entity.Id}");
            }
        }
    }
}