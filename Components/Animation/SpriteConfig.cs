namespace ECS.Components.Animation;

public enum DrawLayer
{
    Background,
    Terrain,
    Platform,
    Player,
    Projectile,
    UI
}

public struct SpriteConfig
{
    public Texture2D Texture;
    public Rectangle SourceRect;
    public Vector2 Origin;
    public Color Color;
    public DrawLayer Layer;
}