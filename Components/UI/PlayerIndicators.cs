namespace ECS.Components.UI;

public struct UIIndicator
{
    public List<String> PotentialValues;
    public int Value;
    public Vector2 Position;
    public Vector2 Offset;
}
public struct PlayerIndicators
{
    public UIIndicator[] Values;
}