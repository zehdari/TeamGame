using ECS.Components.Animation;
using ECS.Components.Input;
using ECS.Resources;

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

    public bool HasAsset<T>(string key)
    {
        return assets.TryGetValue(key, out var asset) && asset is T;
    }

    // Helper Methods for Common Types
    public Texture2D GetTexture(string path) => GetAsset<Texture2D>(path);
    public SpriteFont GetFont(string path) => GetAsset<SpriteFont>(path);
    public AnimationConfig GetAnimation(string path) => GetAsset<AnimationConfig>(path);
    public InputConfig GetInputConfig(string path) => GetAsset<InputConfig>(path);
    public EntityConfig GetEntityConfig(string path) => HasAsset<EntityConfig>(path) ? GetAsset<EntityConfig>(path) : null;
}
