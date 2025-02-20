namespace ECS.Components.Collision;

public enum ShapeType
{
    Rectangle,
    Line
}

public struct CollisionShape
{
    public ShapeType Type;
    public Vector2 Size;       // W/H for Rectangle, End point for Line
    public Vector2 Offset;     // Offset from position
    public bool IsPhysical;    // Whether it should respond to collisions physically
    public bool IsOneWay;      // For platforms you can jump through
}