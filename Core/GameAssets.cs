namespace ECS.Core;

public class GameAssets
{
    private readonly Dictionary<string, object> assets = new();

    public void AddAsset<T>(string key, T asset)
    {
        assets[key] = asset;
    }

    public T GetAsset<T>(string key)
    {
        if (assets.TryGetValue(key, out var asset))
        {
            return (T)asset;
        }
        throw new KeyNotFoundException($"Asset with key '{key}' of type {typeof(T).Name} not found.");
    }

    // Helper Methods for Common Types
    public Texture2D GetTexture(string key) => GetAsset<Texture2D>(key);
    public SpriteFont GetFont(string key) => GetAsset<SpriteFont>(key);
    public AnimationConfig GetAnimation(string key) => GetAsset<AnimationConfig>(key);
    public InputConfig GetInputConfig(string key) => GetAsset<InputConfig>(key);
}
