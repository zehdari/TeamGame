namespace ECS.Core;

internal static class EntityRegistry
{
    private static readonly Dictionary<string, EntityAssetKey> Entities = new();

    internal static IEnumerable<KeyValuePair<string, EntityAssetKey>> GetEntities() => Entities;

    internal static EntityAssetKey GetEntity(string entityName)
    {
        if (Entities.TryGetValue(entityName, out var entityAssetKey))
        {
            return entityAssetKey;
        }
        return null;
    }

    internal static void RegisterEntity(string entityName, string spriteKey, string animationKey, string configKey, string inputKey)
    {
        Entities[entityName] = new EntityAssetKey(spriteKey, animationKey, configKey, inputKey);
    }

    internal static void Clear()
    {
        Entities.Clear();
    }
}

public record EntityAssetKey(string SpriteKey, string AnimationKey, string ConfigKey, string InputKey);