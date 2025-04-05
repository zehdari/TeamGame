using Microsoft.Xna.Framework.Audio;
using ECS.Resources;

namespace ECS.Core;

public static class AssetLoader
{
    private const string DEFAULT_CONFIG_PATH = "Config/AssetConfig/assets.json";

    public static GameAssets LoadAssets(ContentManager content, string configPath = DEFAULT_CONFIG_PATH)
    {
        var assets = new GameAssets();
        var config = AssetConfigLoader.LoadConfig(configPath);

        // Generic loader for assets with key/path pattern
        LoadAssetCollection(config.Textures, (key, path) => AssetManager.LoadTexture(assets, content, key, path));
        LoadAssetCollection(config.Fonts, (key, path) => AssetManager.LoadFont(assets, content, key, path));
        LoadAssetCollection(config.Sounds, (key, path) => AssetManager.LoadSound(assets, content, key, path));
        LoadAssetCollection(config.SpriteSheets, (key, path) => AssetManager.LoadSpriteSheet(assets, key, path));
        LoadAssetCollection(config.InputConfigs, (key, path) => AssetManager.LoadInputConfig(assets, key, path));
        LoadAssetCollection(config.EntityConfigs, (key, path) => AssetManager.LoadEntityConfig(assets, key, path));
        LoadAssetCollection(config.LevelConfigs, (key, path) => AssetManager.LoadLevelConfig(assets, key, path));
        
        RegisterEntities(config.Entities);

        return assets;
    }

    private static void LoadAssetCollection(IEnumerable<AssetEntry> assetConfigs, Action<string, string> loadFunc) 
    {
        foreach (var asset in assetConfigs)
        {
            loadFunc(asset.Key, asset.Path);
        }
    }

    private static void RegisterEntities(IEnumerable<EntityRegistration> entities)
    {
        // Clear existing registry to prevent duplicate registrations
        EntityRegistry.Clear();

        foreach (var entity in entities)
        {
            EntityRegistry.RegisterEntity(
                entity.Key,
                string.IsNullOrEmpty(entity.Sprite) ? null : entity.Sprite,
                string.IsNullOrEmpty(entity.Animation) ? null : entity.Animation,
                string.IsNullOrEmpty(entity.Config) ? null : entity.Config,
                string.IsNullOrEmpty(entity.Input) ? null : entity.Input
            );
        }
    }
}