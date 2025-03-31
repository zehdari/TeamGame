namespace ECS.Components.Collision;

public enum CollisionLayer
{
    None = 0,
    World = 1,
    Physics = 2,
    Hitbox = 4,
    Hurtbox = 8,
    Trigger = 16
}

public struct Polygon
{
    public Vector2[] Vertices;
    public bool IsTrigger;
    public CollisionLayer Layer;
    public CollisionLayer CollidesWith;
}

public struct CollisionBody 
{
    public List<Polygon> Polygons;
}