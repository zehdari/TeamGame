namespace ECS.Core;

internal static class CharacterRegistry
{
    private static readonly Dictionary<string, CharacterAssetKeys> Characters = new();

    internal static IEnumerable<KeyValuePair<string, CharacterAssetKeys>> GetCharacters() => Characters;

    internal static void RegisterCharacter(string characterName, string spriteKey, string animationKey, string configKey)
    {
        Characters[characterName] = new CharacterAssetKeys(spriteKey, animationKey, configKey);
    }

    internal static void Clear()
    {
        Characters.Clear();
    }
}

public record CharacterAssetKeys(string SpriteKey, string AnimationKey, string ConfigKey);
