namespace ECS.Resources;

public abstract class JsonLoaderBase<T> : IJsonLoader<T>
{
    protected static readonly JsonSerializerOptions DefaultOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public T LoadFromJson(string jsonContent)
    {
        try
        {
            return ParseJson(jsonContent);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Error parsing JSON: {ex.Message}", ex);
        }
    }

    public T LoadFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Config file not found at: {filePath}");
        }

        string jsonContent = File.ReadAllText(filePath);
        return LoadFromJson(jsonContent);
    }

    protected abstract T ParseJson(string jsonContent);

    protected TValue GetRequiredValue<TValue>(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            throw new ArgumentException($"Missing required property: {propertyName}");
        }

        try
        {
            return JsonSerializer.Deserialize<TValue>(property.GetRawText(), DefaultOptions);
        }
        catch (JsonException ex)
        {
            throw new ArgumentException($"Invalid value for {propertyName}: {ex.Message}");
        }
    }

    protected TValue GetOptionalValue<TValue>(JsonElement element, string propertyName, TValue defaultValue)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return defaultValue;
        }

        try
        {
            return JsonSerializer.Deserialize<TValue>(property.GetRawText(), DefaultOptions);
        }
        catch (JsonException)
        {
            return defaultValue;
        }
    }
}