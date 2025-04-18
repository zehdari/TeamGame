using ECS.Components.Animation;
using ECS.Components.Physics;
using ECS.Components.Projectiles;
using ECS.Components.PVZ;
using ECS.Components.Random;

namespace ECS.Systems.Spawning;

public class PvZDeathHandlingSystem : SystemBase
{

    public override void Initialize(World world)
    {
        base.Initialize(world);
        Subscribe<PvZDeathEvent>(HandleDeathEvent);
    }

    /// <summary>
    /// Returns bool for if it is in the grid, and coordinates if it is.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="gridInfo"></param>
    /// <returns></returns>
    private (bool, int, int) IsInGrid(Entity entity, GridInfo gridInfo)
    {
        for (int i = 0; i < gridInfo.NumRows; i++)
        {
            for(int j = 0; j < gridInfo.NumColumns; j++)
            {
                if (gridInfo.RowInfo[i][j] != null && ((Entity)gridInfo.RowInfo[i][j]).Equals(entity))
                {
                    return (true, i, j);
                }
            }
        }

        // If we got here, we didn't find it. Return error vals.
        return (false, -1, -1);
    }

    private void RemoveFromGridIfPresent(Entity entity, Entity grid)
    {
        ref var gridInfo = ref GetComponent<GridInfo>(grid);

        var resultTuple = IsInGrid(entity, gridInfo);

        // If it was in the grid, remove it.
        if(resultTuple.Item1)
        {
            gridInfo.RowInfo[resultTuple.Item2][resultTuple.Item3] = null;
        }
        
    }

    private void HandleDeathEvent(IEvent evt)
    {
        PvZDeathEvent deathEvent = (PvZDeathEvent)evt;

        RemoveFromGridIfPresent(deathEvent.Entity, deathEvent.Grid);

        Publish<DespawnEvent>(new DespawnEvent
        {
            Entity = deathEvent.Entity,
        });
    }

    public override void Update(World world, GameTime gameTime)
    {
        
    }
}