using ECS.Components.Input;

namespace ECS.Core;

public class LevelLoader
{
    private readonly World world;
    private readonly EntityFactory entityFactory;

    public LevelLoader(World world)
    {
        this.world = world;
        this.entityFactory = world.entityFactory;
    }

    public void InitializeLevel(GameAssets assets, int screenWidth, int screenHeight, string level)
    {
        var mapconfig= assets.GetMapConfig(level);
        MakeEntities(mapconfig, assets);
    }
    private void MakeEntities(MapConfig mapconfig, GameAssets assets)
    {
        foreach (var(key, value) in mapconfig.Actions)
        {
            foreach(var element in value.levelEntities)
            {
                var pair = EntityRegistry.GetEntities().First(pair => pair.Key.Equals(element));
                var assetKeys = pair.Value;

                // Grab all of my pieces
                var config = assets.GetEntityConfig(assetKeys.ConfigKey);
                var animation = assets.GetAnimation(assetKeys.AnimationKey);
                var sprite = assets.GetTexture(assetKeys.SpriteKey);
                entityFactory.CreateEntityFromConfig(config, sprite, animation);
            }
            
        }
    }

}

    