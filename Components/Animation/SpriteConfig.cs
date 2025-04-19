namespace ECS.Components.Animation;

public enum DrawLayer
{
    Background,
    Terrain,
    Platform,
    Player,
    Projectile,
    Debug,
    DebugText,
    UI,
    UIOverlay1,
    UIOverlay2,
    UIOverlay3,
    UIText
}

public struct SpriteConfig
{
    public Texture2D Texture;
    public Rectangle SourceRect;
    public Vector2 Origin;
    public Color Color;
    public DrawLayer Layer;
}