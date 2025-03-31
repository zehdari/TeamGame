namespace ECS.Resources;

public class EntityConfig
{
    public Dictionary<Type, object> Components { get; } = new();
    
    public Dictionary<string, string> Assets { get; set; } = new();
}

