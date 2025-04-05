using ECS.Components.Input;

namespace ECS.Resources;

public class MapConfigLoader : JsonLoaderBase<MapConfig>
{
    private class InputConfigJson
    {
        public Dictionary<string, InputActionJson> Actions { get; set; }
    }

    private class InputActionJson
    {
        public List<string> Keys { get; set; }
    }

    protected override MapConfig ParseJson(string jsonContent)
    {
        var document = JsonDocument.Parse(jsonContent);
        var root = document.RootElement;

        var actions = GetRequiredValue<Dictionary<string, InputActionJson>>(root, MAGIC.JSON_PARSING.LEVEL_ELEMENTS);

        var inputConfig = new MapConfig
        {
            Actions = new Dictionary<string, MapAction>()
        };

        foreach (var (actionName, actionData) in actions)
        {
            if (actionData?.Keys == null || actionData.Keys.Count == 0)
            {
                throw new InvalidOperationException($"Invalid action data for action: {actionName}");
            }

            inputConfig.Actions[actionName] = new MapAction
            {
                levelEntities = actionData.Keys
            };
        }

        return inputConfig;
    }
}