using ECS.Components.Input;

namespace ECS.Resources;

public class InputConfigLoader : JsonLoaderBase<InputConfig>
{
    private class InputConfigJson
    {
        public Dictionary<string, InputActionJson> Actions { get; set; }
    }

    private class InputActionJson
    {
        public List<string> Keys { get; set; }
        public List<string> Buttons { get; set; }
    }

    protected override InputConfig ParseJson(string jsonContent)
    {
        var document = JsonDocument.Parse(jsonContent);
        var root = document.RootElement;

        var actions = GetRequiredValue<Dictionary<string, InputActionJson>>(root, "actions");
        
        var inputConfig = new InputConfig
        {
            Actions = new Dictionary<string, InputAction>()
        };

        foreach (var (actionName, actionData) in actions)
        {
            if (actionData?.Keys == null || actionData.Keys.Count == 0)
            {
                throw new InvalidOperationException($"Invalid action data for action: {actionName}");
            }

            var keys = actionData.Keys.Select(k => Enum.Parse<Keys>(k)).ToArray();
            var buttons = actionData.Buttons?.Select(b => Enum.Parse<Buttons>(b)).ToArray() ?? Array.Empty<Buttons>();

            inputConfig.Actions[actionName] = new InputAction
            {
                Keys = keys,
                Buttons = buttons
            };
        }

        return inputConfig;
    }
}