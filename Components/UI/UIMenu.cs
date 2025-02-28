namespace ECS.Components.UI;

public struct Button
{
    public string Text;
    public string Action;
    public Color Color;
    public bool Active;
}
public struct UIMenu
{
    public List<Button> Buttons;
    public int Separation; //TODO change separation to an x/y vector if we want menu to be non-linear
    public int Selected;
    public bool Active;
}
