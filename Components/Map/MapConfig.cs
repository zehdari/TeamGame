namespace ECS.Components.Input;

public struct MapConfig
{
    public Dictionary<string, MapAction> Actions;
}

public struct MapAction
{
    public List<string> levelEntities;
}