namespace ECS.Core;

public class GraphicsManager
{
    public static GraphicsManager Instance { get; private set; }  // Static instance

    private readonly GraphicsDeviceManager graphics;
    private readonly Point windowSize = new(800, 600);
    public GraphicsDevice graphicsDevice { get; private set; }
    public SpriteBatch spriteBatch { get; private set; }

    public GraphicsManager(Game game)
    {
        Instance = this;  // Set the static instance

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
    }
    
    public Point GetWindowSize() => windowSize;

    public GraphicsDevice GetGraphicsDevice() => graphicsDevice;
}