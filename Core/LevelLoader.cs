using ECS.Components.Animation;
using ECS.Components.Input;
using ECS.Resources;
using ECS.Systems.Items;

namespace ECS.Core;

public class LevelLoader
{
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

        makeEntities["players"] = MakePlayers;
        makeEntities["platforms"]= MakeLevelObjects;
        makeEntities["items"] = MakeLevelObjects;
        makeEntities["ui"] = MakeUI;
        makeEntities["ai"] = MakeAI;
        makeEntities["background"]= MakeLevelObjects;

        spawnpoints = new[] { new Vector2(100, 100), new Vector2(300, 100), new Vector2(500, 100), new Vector2(600, 100) };
            
    }

    //public void InitializeLevel(string level)
    //{
    //    MakeEntities(level);
    //}

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
            entityFactory.CreateEntityFromConfig(config, sprite, animation);
    }

    private void MakeUI(string element, EntityConfig config, AnimationConfig animation, Texture2D sprite, EntityAssetKey assetKey)
    {
        entityFactory.CreateEntityFromConfig(config, sprite, animation);
    }
    private void MakeAI(string element, EntityConfig config, AnimationConfig animation, Texture2D sprite, EntityAssetKey assetKey)
    {
        var spawnPosition = spawnpoints[currentSpawnpoint++];
        entityFactory.CreateAIFromConfig(config, sprite, animation, spawnPosition);
    }
}

    