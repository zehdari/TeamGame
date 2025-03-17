namespace ECS.Resources;

public class AssetConfigLoader : JsonLoaderBase<AssetConfig>
{

    protected override AssetConfig ParseJson(string jsonContent)
    {
        var document = JsonDocument.Parse(jsonContent);
        var root = document.RootElement;
        
        var config = new AssetConfig
        {
            Textures = GetOptionalValue<List<AssetEntry>>(root, "textures", new List<AssetEntry>()),
            Effects = GetOptionalValue<List<AssetEntry>>(root, "effects", new List<AssetEntry>()),
            Fonts = GetOptionalValue<List<AssetEntry>>(root, "fonts", new List<AssetEntry>()),
            Sounds = GetOptionalValue<List<AssetEntry>>(root, "sounds", new List<AssetEntry>()),
            SpriteSheets = GetOptionalValue<List<AssetEntry>>(root, "spriteSheets", new List<AssetEntry>()),
            InputConfigs = GetOptionalValue<List<AssetEntry>>(root, "inputConfigs", new List<AssetEntry>()),
            EntityConfigs = GetOptionalValue<List<AssetEntry>>(root, "entityConfigs", new List<AssetEntry>()),
            LevelConfigs = GetOptionalValue<List<AssetEntry>>(root, "levelConfigs", new List<AssetEntry>()),
            Entities = GetOptionalValue<List<EntityRegistration>>(root, "entities", new List<EntityRegistration>())
        };
        
        return config;
    }

    public static AssetConfig LoadConfig(string filePath)
    {
        var loader = new AssetConfigLoader();
        return loader.LoadFromFile(filePath);
    }
}