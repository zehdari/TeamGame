using ECS.Core.Utilities;
using ECS.Components.Animation;

namespace ECS.Core;

public class GraphicsManager
{
    private readonly GraphicsDeviceManager graphics;
    private readonly Point windowSize = new(800, 600);
    public GraphicsDevice graphicsDevice { get; private set; }
    public SpriteBatch spriteBatch { get; private set; }
    public CameraManager cameraManager { get; private set; }

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
        cameraManager = new CameraManager(graphicsDevice);
    }
    
    public Point GetWindowSize() => windowSize;

    public GraphicsDevice GetGraphicsDevice() => graphicsDevice;

    public Matrix GetTransformMatrix() => cameraManager.GetTransformMatrix();

    public float GetLayerDepth(DrawLayer layer)
    {
        // Get total number of layers in the enum
        int totalLayers = Enum.GetValues(typeof(DrawLayer)).Length;
        
        // Convert layer to int and normalize to 0.0f-1.0f range
        // Divide by total to get a value between 0 and 1
        return (float)((int)layer) / (totalLayers - 1);
    }
}