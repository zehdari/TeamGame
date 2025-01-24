namespace ECS.Resources;

public static class JsonLoader
{
    private static readonly JsonSerializerOptions DefaultOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public static T LoadJson<T>(string filePath, JsonSerializerOptions options = null) where T : struct
    {
        string jsonContent = File.ReadAllText(filePath);
        return ParseJson<T>(jsonContent, options);
    }

    public static T ParseJson<T>(string jsonContent, JsonSerializerOptions options = null) where T : struct
    {
        options ??= DefaultOptions;
        
        try
        {
            var result = JsonSerializer.Deserialize<T>(jsonContent, options);
            return result;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Error parsing JSON for {typeof(T)}: {ex.Message}", ex);
        }
    }
}