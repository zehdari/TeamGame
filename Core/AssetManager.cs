using ECS.Resources;

namespace ECS.Core;

public static class AssetManager
{
    private static readonly SpriteSheetLoader spriteSheetLoader = new();
    private static readonly InputConfigLoader inputConfigLoader = new();
    private static readonly EntityConfigLoader entityConfigLoader = new();
    private static readonly MapConfigLoader mapConfigLoader = new();

    public static void LoadTexture(GameAssets assets, ContentManager content, string key, string path)
    {
        assets.AddAsset(key, content.Load<Texture2D>(path));
    }

    public static void LoadFont(GameAssets assets, ContentManager content, string key, string path)
    {
        assets.AddAsset(key, content.Load<SpriteFont>(path));
    }

    public static void LoadSpriteSheet(GameAssets assets, string key, string path)
    {
        var config = spriteSheetLoader.LoadFromFile(path);
        assets.AddAsset(key, config);
    }

    public static void LoadInputConfig(GameAssets assets, string key, string path)
    {
        var config = inputConfigLoader.LoadFromFile(path);
        assets.AddAsset(key, config);
    }

    public static void LoadEntityConfig(GameAssets assets, string key, string path)
    {
        var config = entityConfigLoader.LoadFromFile(path);
        assets.AddAsset(key, config);
    }
    public static void LoadLevelConfig(GameAssets assets, string key, string path)
    {
        var config = mapConfigLoader.LoadFromFile(path);
        assets.AddAsset(key, config);
    }
    public static void LoadSound(GameAssets assets, ContentManager content, string key, string path)
    {
        assets.AddAsset(key, content.Load<SoundEffect>(path));
    }
}
