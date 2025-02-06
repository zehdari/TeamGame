namespace ECS.Core;

public static class AssetManager
{
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
        assets.AddAsset(key, SpriteSheetLoader.LoadSpriteSheet(File.ReadAllText(path)));
    }

    public static void LoadInputConfig(GameAssets assets, string key, string path)
    {
        assets.AddAsset(key, InputConfigLoader.LoadInputConfig(File.ReadAllText(path)));
    }
}
