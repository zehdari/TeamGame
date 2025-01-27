namespace ECS.Resources;

public static class InputConfigLoader
{
    private class InputConfigJson
    {
        public Dictionary<string, InputActionJson> Actions { get; set; }
    }

    private class InputActionJson
    {
        public List<string> Keys { get; set; }
        public string Axis { get; set; }
        public float Value { get; set; }
    }

    public static InputConfig LoadInputConfig(string jsonContent)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var data = JsonSerializer.Deserialize<InputConfigJson>(jsonContent, options);
        
        if (data?.Actions == null)
        {
            throw new InvalidOperationException("Invalid input config JSON: missing or null Actions");
        }

        var inputConfig = new InputConfig
        {
            Actions = new Dictionary<string, InputAction>()
        };

        foreach (var (actionName, actionData) in data.Actions)
        {
            if (actionData?.Keys == null || actionData.Keys.Count == 0)
            {
                throw new InvalidOperationException($"Invalid action data for action: {actionName}");
            }

            var keys = actionData.Keys.Select(k => Enum.Parse<Keys>(k)).ToArray();
            
            inputConfig.Actions[actionName] = new InputAction
            {
                Keys = keys,
                Axis = actionData.Axis,
                Value = actionData.Value
            };
        }

        return inputConfig;
    }

    public static InputConfig LoadInputConfigFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Input config not found at: {filePath}");
        }

        string jsonContent = File.ReadAllText(filePath);
        return LoadInputConfig(jsonContent);
    }
}