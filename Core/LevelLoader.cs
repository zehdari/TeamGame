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

    public void InitializeLevel(GameAssets assets, int screenWidth, int screenHeight)
    {
        List<string> platforms = new List<string>();
        platforms.Add("little_left_platform_day");
        platforms.Add("little_right_platform_day");
        platforms.Add("big_platform_day");
        MakeEntity(platforms, assets);
    }
    private void MakeEntity(List<string> platforms, GameAssets assets)
    {
        foreach (var plat in platforms)
        {
            var pair = CharacterRegistry.GetCharacters().First(pair => pair.Key.Equals(plat));
            var assetKeys = pair.Value;

            // Grab all of my pieces
            var config = assets.GetEntityConfig(assetKeys.ConfigKey);
            var animation = assets.GetAnimation(assetKeys.AnimationKey);
            var sprite = assets.GetTexture(assetKeys.SpriteKey);
            entityFactory.CreateEntityFromConfig(config, sprite, animation);
        }
    }

}

    