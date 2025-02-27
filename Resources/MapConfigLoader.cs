using ECS.Components.Animation;

namespace ECS.Resources;

public class MapConfigLoader : JsonLoaderBase<List<string>>
{
    
    protected override List<string> ParseJson(string jsonContent)
    {
        var document = JsonDocument.Parse(jsonContent);
        var root = document.RootElement;

        return GetRequiredValue<List<string>>(root, "platforms");
    }
}