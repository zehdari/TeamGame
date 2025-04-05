using ECS.Components.Animation;
using ECS.Components.Input;
using ECS.Resources;
using ECS.Systems.Items;

namespace ECS.Core;

public class LevelLoader
{
    private const int X_1_SPAWNPOINT = 100;
    private const int X_2_SPAWNPOINT = 300;
    private const int X_3_SPAWNPOINT = 500;
    private const int X_4_SPAWNPOINT = 600;
    private const int Y_1_SPAWNPOINT = 100;
    private const int Y_2_SPAWNPOINT = 100;
    private const int Y_3_SPAWNPOINT = 100;
    private const int Y_4_SPAWNPOINT = 100;

    private readonly World world;
    private readonly EntityFactory entityFactory;
    private GameAssets assets;
    private delegate void MakeEntity(string element, EntityConfig config, AnimationConfig animation, Texture2D sprite, EntityAssetKey assetKey);
    private Dictionary<string, MakeEntity> makeEntities = new Dictionary<string, MakeEntity>();
    private Vector2[] spawnpoints;
    private int currentSpawnpoint;
    public bool shouldChangeLevel { get; set; }

    public LevelLoader(World world, GameAssets assets)
        
    {
        shouldChangeLevel = true;
        this.world = world;
        this.entityFactory = world.entityFactory;
        this.assets = assets;

        makeEntities[MAGIC.LEVEL.PLAYERS] = MakePlayers;
        makeEntities[MAGIC.LEVEL.PLATFORMS]= MakeLevelObjects;
        makeEntities[MAGIC.LEVEL.ITEMS] = MakeLevelObjects;
        makeEntities[MAGIC.LEVEL.UI] = MakeUI;
        makeEntities[MAGIC.LEVEL.AI] = MakeAI;
        makeEntities[MAGIC.LEVEL.BACKGROUND]= MakeLevelObjects;

        spawnpoints = new[] { new Vector2(X_1_SPAWNPOINT, Y_1_SPAWNPOINT),
            new Vector2(X_2_SPAWNPOINT, Y_2_SPAWNPOINT),
            new Vector2(X_3_SPAWNPOINT, Y_3_SPAWNPOINT),
            new Vector2(X_4_SPAWNPOINT, Y_4_SPAWNPOINT) };
            
    }

    public void MakeEntities(string level)
    {
        currentSpawnpoint = 0;

        var config = assets.GetMapConfig(level);

        foreach (var (key, value) in config.Actions)
        {
            foreach(var identifier in value.levelEntities)
            {
                var pair = EntityRegistry.GetEntities().First(pair => pair.Key.Equals(identifier));
                var assetKeys = pair.Value;
                var entityConfig = assets.GetEntityConfig(assetKeys.ConfigKey);
                var animation = assets.GetAnimation(assetKeys.AnimationKey);
                var sprite = assets.GetTexture(assetKeys.SpriteKey);
                makeEntities[key](identifier, entityConfig, animation, sprite, assetKeys);

            }

        }
    }
    private void MakePlayers(string element, EntityConfig config, AnimationConfig animation, Texture2D sprite, EntityAssetKey assetKey)
    {

        // Grab all of my pieces
        var input = assets.GetAsset<InputConfig>(assetKey.InputKey);
        var spawnPosition = spawnpoints[currentSpawnpoint++];
        entityFactory.CreatePlayerFromConfig(config, sprite, animation, input, spawnPosition);
    }

    private void MakeLevelObjects(string element, EntityConfig config, AnimationConfig animation, Texture2D sprite, EntityAssetKey assetKey)
    {
        entityFactory.CreateEntityFromKey(element, assets);
    }

    private void MakeUI(string element, EntityConfig config, AnimationConfig animation, Texture2D sprite, EntityAssetKey assetKey)
    {
        entityFactory.CreateEntityFromKey(element, assets);
    }
    private void MakeAI(string element, EntityConfig config, AnimationConfig animation, Texture2D sprite, EntityAssetKey assetKey)
    {
        var spawnPosition = spawnpoints[currentSpawnpoint++];
        entityFactory.CreateAIFromConfig(config, sprite, animation, spawnPosition);
    }
}

    