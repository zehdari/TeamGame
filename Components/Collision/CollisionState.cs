namespace ECS.Components.Collision;

[Flags]
public enum ContactFlags
{
    None = 0,
    Top = 1,
    Bottom = 2,
    Left = 4,
    Right = 8
}

public struct ContactState
{
    public ContactFlags Flags;
    public HashSet<Entity> Contacts;
}