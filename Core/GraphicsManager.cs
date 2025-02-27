namespace ECS.Core;

public class GraphicsManager
{
    private readonly GraphicsDeviceManager graphics;
    private readonly Point windowSize = new(800, 600);
    public GraphicsDevice graphicsDevice { get; private set; }
    public SpriteBatch spriteBatch { get; private set; }

    public SpatialGrid spatialGrid { get; private set; }

    public GraphicsManager(Game game)
    {
        graphics = new GraphicsDeviceManager(game);
        
        graphics.PreferredBackBufferWidth = windowSize.X;
        graphics.PreferredBackBufferHeight = windowSize.Y;
        graphics.ApplyChanges();
        
        game.Content.RootDirectory = "Content";
        game.IsMouseVisible = true;
    }

    public void Initialize()
    {
        graphicsDevice = graphics.GraphicsDevice;
        spriteBatch = new SpriteBatch(graphicsDevice);
        spatialGrid = new SpatialGrid(windowSize, 75);
    }
    
    public Point GetWindowSize() => windowSize;

    public GraphicsDevice GetGraphicsDevice() => graphicsDevice;
}