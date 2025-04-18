using ECS.Components.State;
using ECS.Components.Physics;
using ECS.PvZ.Components;
public class GridSystem : SystemBase
{
    private GameAssets assets;

    private Dictionary<string, Action<Entity>> indexActions;
    private Dictionary<string, Action<Entity>> plantActions;


    public GridSystem()
    {
        indexActions = new Dictionary<string, Action<Entity>>
        {
            [MAGIC.ACTIONS.ROW_DOWN] = (entity) => DecrementRow(entity),
            [MAGIC.ACTIONS.ROW_UP] = (entity) => IncrementRow(entity),
            [MAGIC.ACTIONS.COLUMN_DOWN] = (entity) => DecrementColumn(entity),
            [MAGIC.ACTIONS.COLUMN_UP] = (entity) => IncrementColumn(entity),
            [MAGIC.ACTIONS.PLANT_LIST_RIGHT] = (entity) => IncrementPlantList(entity),
            [MAGIC.ACTIONS.PLANT_LIST_LEFT] = (entity) => DecrementPlantList(entity),
        };

        plantActions = new Dictionary<string, Action<Entity>>
        {
            [MAGIC.ACTIONS.PLANT] = (entity) => PlantThePlant(entity),
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

        // If the action was an index action, call the associated function
        if(indexActions.ContainsKey(action.ActionName))
            indexActions[action.ActionName](action.Entity);
    }

    private void DecrementRow(Entity entity)
    {
        ref var currentTile = ref GetComponent<CurrentTile>(entity);

        // Make sure to stay in bounds
        if (currentTile.RowIndex > 0)
            currentTile.RowIndex -= 1;
    }

    private void IncrementRow(Entity entity)
    {
       

        ref var currentTile = ref GetComponent<CurrentTile>(entity);
        ref var gridInfo = ref GetComponent<GridInfo>(entity);

        // Make sure to stay in bounds
        if (currentTile.RowIndex <= gridInfo.RowInfo.Length)
            currentTile.RowIndex += 1;
    }

    private void DecrementColumn(Entity entity)
    {
        ref var currentTile = ref GetComponent<CurrentTile>(entity);

        // Make sure to stay in bounds
        if (currentTile.ColumnIndex > 0)
            currentTile.ColumnIndex -= 1;
    }

    private void IncrementColumn(Entity entity)
    {
        ref var currentTile = ref GetComponent<CurrentTile>(entity);
        ref var gridInfo = ref GetComponent<GridInfo>(entity);

        // Make sure to stay in bounds
        if (currentTile.ColumnIndex <= gridInfo.RowInfo[0].Length)
            currentTile.ColumnIndex += 1;
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

    private void SetPosition(Entity plant, Entity grid, GridInfo gridInfo, CurrentTile currentTile)
    {
        ref var gridPosition = ref GetComponent<Position>(grid);

        float xCoord = gridPosition.Value.X + (gridInfo.TileSize * currentTile.ColumnIndex);
        float yCoord = gridPosition.Value.Y + (gridInfo.TileSize * currentTile.RowIndex);

        ref var plantPosition = ref GetComponent<Position>(plant);
        plantPosition.Value = new Vector2(xCoord, yCoord);
    }

    private void PlantThePlant(Entity entity)
    {
        ref var plantList = ref GetComponent<PlantList>(entity);
        ref var gridInfo = ref GetComponent<GridInfo>(entity);
        ref var currentTile = ref GetComponent<CurrentTile>(entity);

        Entity plant = World.entityFactory.CreateEntityFromKey(plantList.PossiblePlants[plantList.CurrentPlantIndex], assets);
        SetPosition(plant, entity, gridInfo, currentTile);
        gridInfo.RowInfo[currentTile.RowIndex][currentTile.ColumnIndex] = plant;
    }

    public override void Update(World world, GameTime gameTime)
    {



    }
}