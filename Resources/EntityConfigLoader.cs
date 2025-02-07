using ECS.Core;

namespace ECS.Resources;

public class EntityConfigLoader : JsonLoaderBase<EntityConfig>
{
    private static readonly Dictionary<string, Type> ComponentTypes;

    static EntityConfigLoader()
    {
        ComponentTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes()) // Flatten all types from all assemblies
            .Where(t => 
                t.Namespace?.StartsWith("ECS.Components.") == true && // Only ECS component types
                t.IsValueType // Ensures only structs are selected (Components)
            )
            .ToDictionary(
                t => t.Name, // Use the type name as the key
                t => t, // Store the type itself as the value
                StringComparer.OrdinalIgnoreCase // Ensure case-insensitive lookup
            );
    }

    protected override EntityConfig ParseJson(string jsonContent)
    {
        var document = JsonDocument.Parse(jsonContent);
        var root = document.RootElement;

        var entityConfig = new EntityConfig();

        if (root.TryGetProperty("Assets", out var assetsElement))
        {
            foreach (var asset in assetsElement.EnumerateObject())
            {
                entityConfig.Assets[asset.Name] = asset.Value.GetString();
            }
        }

        if (root.TryGetProperty("components", out var componentsElement))
        {
            foreach (var componentProperty in componentsElement.EnumerateObject())
            {
                string componentName = componentProperty.Name;

                if (!ComponentTypes.TryGetValue(componentName, out Type componentType))
                {
                    throw new InvalidOperationException($"Unknown component type: {componentName}");
                }

                var componentValue = JsonSerializer.Deserialize(
                    componentProperty.Value.GetRawText(),
                    componentType,
                    new JsonSerializerOptions
                    {
                        IncludeFields = true,
                        PropertyNameCaseInsensitive = true,
                        Converters = { new JsonStringEnumConverter(), new Vector2JsonConverter() }
                    });

                entityConfig.Components[componentType] = componentValue;
            }
        }

        return entityConfig;
    }

}