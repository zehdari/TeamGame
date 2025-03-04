using ECS.Components.Animation;
using ECS.Components.Input;
using ECS.Resources;

namespace ECS.Core;

public class LevelLoader
{
    private readonly World world;
    private readonly EntityFactory entityFactory;
    private GameAssets assets;
    private MapConfig config;
    private delegate void MakeEntity(string element, EntityConfig config, AnimationConfig animation, Texture2D sprite, EntityAssetKey assetKey);
    private Dictionary<string, MakeEntity> makeEntities = new Dictionary<string, MakeEntity>();
    private Vector2[] spawnpoints;
    private int currentSpawnpoint;
    public LevelLoader(World world)
    {
        this.world = world;
        this.entityFactory = world.entityFactory;
        makeEntities["players"] = MakePlayers;
        makeEntities["platforms"]= MakeLevelObjects;
        makeEntities["items"] = MakeLevelObjects;
        makeEntities["ui"] = MakeUI;
        makeEntities["ai"] = MakeAI;

        spawnpoints = new[] { new Vector2(100, 100), new Vector2(500, 500), new Vector2(250, 250), new Vector2(400, 400) };
            
    }

    public void InitializeLevel(GameAssets assets, int screenWidth, int screenHeight, string level)
    {
        currentSpawnpoint = 0;
        this.assets = assets;
        config = assets.GetMapConfig(level);
        MakeEntities();
    }
    private void MakeEntities()
    {
        foreach (var (key, value) in config.Actions)
        {
            foreach(var identifier in value.levelEntities)
            {
                var pair = EntityRegistry.GetEntities().First(pair => pair.Key.Equals(identifier));
                var assetKeys = pair.Value;
                var config = assets.GetEntityConfig(assetKeys.ConfigKey);
                var animation = assets.GetAnimation(assetKeys.AnimationKey);
                var sprite = assets.GetTexture(assetKeys.SpriteKey);
                makeEntities[key](identifier, config, animation, sprite, assetKeys);

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
            entityFactory.CreateEntityFromConfig(config, sprite, animation);
    }

    private void MakeUI(string element, EntityConfig config, AnimationConfig animation, Texture2D sprite, EntityAssetKey assetKey)
    {
        var input = assets.GetAsset<InputConfig>(assetKey.InputKey);
        entityFactory.CreateEntityFromConfig(config, sprite, animation, input);
    }
    private void MakeAI(string element, EntityConfig config, AnimationConfig animation, Texture2D sprite, EntityAssetKey assetKey)
    {
        var spawnPosition = spawnpoints[currentSpawnpoint++];
        entityFactory.CreateAIFromConfig(config, sprite, animation, spawnPosition);
    }
}

    