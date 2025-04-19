using ECS.Components.State;
using ECS.Components.Physics;
using ECS.Components.PVZ;
using System;
using ECS.Components.Animation;
using ECS.Components.Timer;
using ECS.Core;
public class GridSystem : SystemBase
{

    private readonly Dictionary<string, Action<Entity>> indexActions;
    private readonly Dictionary<string, Action<Entity>> plantActions;

    public GridSystem()
    {

        indexActions = new Dictionary<string, Action<Entity>>
        {
            [MAGIC.ACTIONS.ROW_DOWN] = (entity) => IncrementRow(entity),
            [MAGIC.ACTIONS.ROW_UP] = (entity) => DecrementRow(entity),
            [MAGIC.ACTIONS.COLUMN_LEFT] = (entity) => DecrementColumn(entity),
            [MAGIC.ACTIONS.COLUMN_RIGHT] = (entity) => IncrementColumn(entity),
            [MAGIC.ACTIONS.PLANT_LIST_RIGHT] = (entity) => IncrementPlantList(entity),
            [MAGIC.ACTIONS.PLANT_LIST_LEFT] = (entity) => DecrementPlantList(entity),
        };

        plantActions = new Dictionary<string, Action<Entity>>
        {
            [MAGIC.ACTIONS.PLANT] = (entity) => PlantThePlant(entity),
            [MAGIC.ACTIONS.DIG] = (entity) => DigThePlant(entity),
        };
    }
        
    public override void Initialize(World world)
    {
        base.Initialize(world);
        Subscribe<ActionEvent>(HandleGridActions);
    }

    public void HandleGridActions(IEvent evt)
    {
        ActionEvent action = (ActionEvent)evt;

        if (!HasComponents<GridTag>(action.Entity)) return;
        if(!action.IsStarted) return;

        System.Diagnostics.Debug.WriteLine($"Action was: {action.ActionName}");

        // If the action was an index action, call the associated function
        if (indexActions.ContainsKey(action.ActionName))
            indexActions[action.ActionName](action.Entity);

        // If it was a plant action, call the associated function
        if(plantActions.ContainsKey(action.ActionName))
            plantActions[action.ActionName](action.Entity);
    }

    private void DecrementRow(Entity entity)
    {
        ref var currentTile = ref GetComponent<CurrentTile>(entity);
        ref var gridInfo = ref GetComponent<GridInfo>(entity);

        System.Diagnostics.Debug.WriteLine($"Decrementing row");
        System.Diagnostics.Debug.WriteLine($"Row index was: {currentTile.RowIndex}");

        // Make sure to stay in bounds
        if (currentTile.RowIndex > 0)
        {
            currentTile.RowIndex -= 1;
        } else
        {
            currentTile.RowIndex = gridInfo.NumRows - 1;
        }
            

        System.Diagnostics.Debug.WriteLine($"Row index is: {currentTile.RowIndex}");
    }

    private void IncrementRow(Entity entity)
    {
       

        ref var currentTile = ref GetComponent<CurrentTile>(entity);
        ref var gridInfo = ref GetComponent<GridInfo>(entity);

        System.Diagnostics.Debug.WriteLine($"Incrementing row");
        System.Diagnostics.Debug.WriteLine($"Row index was: {currentTile.RowIndex}");

        // Make sure to stay in bounds
        if (currentTile.RowIndex < gridInfo.NumRows - 1)
        {
            currentTile.RowIndex += 1;
        } else
        {
            currentTile.RowIndex = 0;
        }

        System.Diagnostics.Debug.WriteLine($"Row index is: {currentTile.RowIndex}");
    }

    private void DecrementColumn(Entity entity)
    {
        ref var currentTile = ref GetComponent<CurrentTile>(entity);
        ref var gridInfo = ref GetComponent<GridInfo>(entity);

        System.Diagnostics.Debug.WriteLine($"Incrementing col");
        System.Diagnostics.Debug.WriteLine($"Col index was: {currentTile.ColumnIndex}");

        // Make sure to stay in bounds
        if (currentTile.ColumnIndex > 0)
        {
            currentTile.ColumnIndex -= 1;
        } else
        {
            currentTile.ColumnIndex = gridInfo.NumColumns - 1;
        }

        System.Diagnostics.Debug.WriteLine($"Col index is: {currentTile.ColumnIndex}");
    }

    private void IncrementColumn(Entity entity)
    {
        ref var currentTile = ref GetComponent<CurrentTile>(entity);
        ref var gridInfo = ref GetComponent<GridInfo>(entity);

        System.Diagnostics.Debug.WriteLine($"Incrementing col");
        System.Diagnostics.Debug.WriteLine($"Col index was: {currentTile.ColumnIndex}");

        // Make sure to stay in bounds
        if (currentTile.ColumnIndex < gridInfo.NumColumns - 1)
        {
            currentTile.ColumnIndex += 1;
        }
        else
        {
            currentTile.ColumnIndex = 0;
        }

        System.Diagnostics.Debug.WriteLine($"Col index is: {currentTile.ColumnIndex}");
    }

    private void DecrementPlantList(Entity entity)
    {
        ref var plantList = ref GetComponent<PlantList>(entity);

        if(plantList.CurrentPlantIndex == 0)
        {
            plantList.CurrentPlantIndex = plantList.PossiblePlants.Count - 1;
        }
        else
        {
            plantList.CurrentPlantIndex--;
        }
        
    }

    private void IncrementPlantList(Entity entity)
    {
        ref var plantList = ref GetComponent<PlantList>(entity);

        if (plantList.CurrentPlantIndex == plantList.PossiblePlants.Count - 1)
        {
            plantList.CurrentPlantIndex = 0;
        }
        else
        {
            plantList.CurrentPlantIndex++;
        }
    }

    private Vector2 GetPosition(Entity grid, GridInfo gridInfo, CurrentTile currentTile)
    {
        ref var gridPosition = ref GetComponent<Position>(grid);
        ref var scale = ref GetComponent<Scale>(grid);

        System.Diagnostics.Debug.WriteLine($"Col index: {currentTile.ColumnIndex}");
        System.Diagnostics.Debug.WriteLine($"Row index: {currentTile.RowIndex}");

        float xCoord = gridPosition.Value.X + (gridInfo.TileSize * scale.Value.X * currentTile.ColumnIndex) - gridInfo.XOffset;
        float yCoord = gridPosition.Value.Y + (gridInfo.TileSize * scale.Value.Y * currentTile.RowIndex) - gridInfo.YOffset;

        System.Diagnostics.Debug.WriteLine($"xCoord: {xCoord}");
        System.Diagnostics.Debug.WriteLine($"yCoord: {yCoord}");

        return new Vector2(xCoord, yCoord);
    }

    private void PlantThePlant(Entity entity)
    {
        ref var plantList = ref GetComponent<PlantList>(entity);
        ref var gridInfo = ref GetComponent<GridInfo>(entity);
        ref var currentTile = ref GetComponent<CurrentTile>(entity);

        // Entity plant = World.entityFactory.CreateEntityFromKey(plantList.PossiblePlants[plantList.CurrentPlantIndex], assets);

        Publish<PvZSpawnEvent>(new PvZSpawnEvent
        {
            Grid = entity,
            Entity = entity,
            typeSpawned = plantList.PossiblePlants[plantList.CurrentPlantIndex],
            spawnPosition = GetPosition(entity, gridInfo, currentTile),
            GridAssigned = true,
        });

        // gridInfo.RowInfo[currentTile.RowIndex][currentTile.ColumnIndex] = plant;
    }

    private void DigThePlant(Entity entity)
    {
        ref var gridInfo = ref GetComponent<GridInfo>(entity);
        ref var currentTile = ref GetComponent<CurrentTile>(entity);

        // Get the plant at the current position, and set grid to null
        var plant = gridInfo.RowInfo[currentTile.RowIndex][currentTile.ColumnIndex];

        if (plant != null)
        {
            gridInfo.RowInfo[currentTile.RowIndex][currentTile.ColumnIndex] = null;

            Publish<DespawnEvent>(new DespawnEvent
            {
                Entity = (Entity)plant
            });

        }
    }

    private void StartProjectileTimer(Entity entity)
    {
        ref var timers = ref GetComponent<Timers>(entity);

        timers.TimerMap[TimerType.AITimer] = new Timer
        {
            Duration = 1.75f,
            Elapsed = 0f,
            Type = TimerType.AITimer,
            OneShot = true,
        };
    }

    private void MakePlantsAttack(Entity grid, GridInfo gridInfo, ref Entity?[] row)
    {
        for (int i = 0; i < row.Length; i++)
        {
            if(row[i] != null)
            {
                ref var timers = ref GetComponent<Timers>((Entity)row[i]);

                if (timers.TimerMap.ContainsKey(TimerType.AITimer))
                    continue;

                StartProjectileTimer((Entity)row[i]);
                Publish<ProjectileSpawnEvent>(new ProjectileSpawnEvent
                {
                    typeSpawned = MAGIC.SPAWNED.PVZ_PEA,
                    Entity = (Entity)row[i],
                    World = World
                });
            }
        }
    }

    public override void Update(World world, GameTime gameTime) 
    {
        foreach (var entity in World.GetEntities())
        {
            if (!HasComponents<GridTag>(entity))
                continue;

            ref var gridInfo = ref GetComponent<GridInfo>(entity);

            for (int i = 0; i < gridInfo.ZombiesInRow.Length; i++)
            {
                if (gridInfo.ZombiesInRow[i].Count > 0)
                {
                    MakePlantsAttack(entity, gridInfo, ref gridInfo.RowInfo[i]);
                }
            }
        }
        
    }
}