namespace ECS.Core.Utilities;

public class AssetEntry
{
    public string Key { get; set; }
    public string Path { get; set; }
}

public class AssetConfig
{
    public List<AssetEntry> Textures { get; set; } = new();
    public List<AssetEntry> Fonts { get; set; } = new();
    public List<AssetEntry> Sounds { get; set; } = new();
    public List<AssetEntry> SpriteSheets { get; set; } = new();
    public List<AssetEntry> InputConfigs { get; set; } = new();
    public List<AssetEntry> EntityConfigs { get; set; } = new();
    public List<AssetEntry> LevelConfigs { get; set; } = new();
    public List<EntityRegistration> Entities { get; set; } = new();
}

public class EntityRegistration
{
    public string Key { get; set; }
    public string Sprite { get; set; }
    public string Animation { get; set; }
    public string Config { get; set; }
    public string Input { get; set; }
}