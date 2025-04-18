using ECS.Components.Animation;
using ECS.Components.Physics;
using ECS.Components.Projectiles;
using ECS.Components.PVZ;
using ECS.Components.Random;

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
        // Get the grid
        Entity? grid = null;
        try
        {
            grid = World.GetEntities().First(grid => HasComponents<GridTag>(grid));
        } catch (Exception ex)
        {
            return;
        }
        
       foreach(var entity in World.GetEntities())
        {
            if (!HasComponents<PvZTag>(entity))
                continue;

            if (!HasComponents<Percent>(entity)) return;
            ref var percent = ref GetComponent<Percent>(entity);
            if(percent.Value > DEATH_PERCENT_LIMIT)
            {
                Publish<PvZDeathEvent>(new PvZDeathEvent
                {
                    Entity = entity,
                    Grid = (Entity)grid,
                });
            }
        }
    }
}