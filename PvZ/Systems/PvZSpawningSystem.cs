using ECS.Components.Animation;
using ECS.Components.Physics;
using ECS.Components.Projectiles;
using ECS.Components.PVZ;
using ECS.Components.Random;

namespace ECS.Systems.Spawning;

public class PvZSpawningSystem : SystemBase
{
    private EntityFactory entityFactory;
    private GameAssets assets;
    private Stack<(Vector2, string, Entity, bool)> spawners = new();

    public PvZSpawningSystem(GameAssets assets)
    {
        this.assets = assets;
    }

    public override void Initialize(World world)
    {
        base.Initialize(world);
        entityFactory = world.entityFactory;
        Subscribe<PvZSpawnEvent>(HandleSpawnAction);
    }

    private void HandleSpawnAction(IEvent evt)
    {
        var spawnEvent = (PvZSpawnEvent)evt;

        spawners.Push((spawnEvent.spawnPosition, spawnEvent.typeSpawned, spawnEvent.Grid, spawnEvent.GridAssigned));
    }

    public override void Update(World world, GameTime gameTime)
    {
        while (spawners.Count > 0)
        {
            var tuple = spawners.Pop();
            var spawnpoint = tuple.Item1;
            var type = tuple.Item2;
            var grid = tuple.Item3;
            var gridAssigned = tuple.Item4;

            var entity = entityFactory.CreateEntityFromKey(type, assets);

            // Set the position of the entity
            ref var entityPosition = ref GetComponent<Position>(entity);
            entityPosition.Value = spawnpoint;

            // Store entity in the grid
            ref var gridInfo = ref GetComponent<GridInfo>(grid);
            ref var currentTile = ref GetComponent<CurrentTile>(grid);

            // If there's an entity already at that position, write over it
            if (gridAssigned)
            {
                if(gridInfo.RowInfo[currentTile.RowIndex][currentTile.ColumnIndex] != null)
                    World.DestroyEntity((Entity)(gridInfo.RowInfo[currentTile.RowIndex][currentTile.ColumnIndex]));

                gridInfo.RowInfo[currentTile.RowIndex][currentTile.ColumnIndex] = entity;
            }

            System.Diagnostics.Debug.WriteLine($"Entity type is {type}");
            System.Diagnostics.Debug.WriteLine($"Spawnpoint is supposed to be {spawnpoint}");

        }
    }
}