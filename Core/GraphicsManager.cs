using ECS.Core.Utilities;
using ECS.Components.Animation;

namespace ECS.Core;

public class GraphicsManager
{
    // Original window size for reference
    private readonly Point defaultWindowSize = new(800, 600);
    
    // Current window size that we'll update when resized
    private Point currentWindowSize;
    
    private readonly GraphicsDeviceManager graphics;
    public GraphicsDevice graphicsDevice { get; private set; }
    public SpriteBatch spriteBatch { get; private set; }
    public CameraManager cameraManager { get; private set; }
    
    // Add a delegate/event to notify systems when window size changes
    public delegate void WindowResizedHandler(Point newSize);
    public event WindowResizedHandler OnWindowResized;

    public GraphicsManager(Game game)
    {
        graphics = new GraphicsDeviceManager(game);
        
        graphics.PreferredBackBufferWidth = defaultWindowSize.X;
        graphics.PreferredBackBufferHeight = defaultWindowSize.Y;
        currentWindowSize = defaultWindowSize;
        
        // Allow window resizing
        game.Window.AllowUserResizing = true;
        
        // Add event handlers for window client size changed
        game.Window.ClientSizeChanged += Window_ClientSizeChanged;
        
        graphics.ApplyChanges();
        
        game.Content.RootDirectory = MAGIC.CONFIG.CONTENT;
        game.IsMouseVisible = true;
    }

    // Handle window resizing event
    private void Window_ClientSizeChanged(object sender, EventArgs e)
    {
        // Get new window size
        currentWindowSize = new Point(
            graphicsDevice.Viewport.Width,
            graphicsDevice.Viewport.Height
        );
        
        // Notify camera manager and other systems about the resize
        OnWindowResized?.Invoke(currentWindowSize);
    }

    public void Initialize()
    {
        graphicsDevice = graphics.GraphicsDevice;
        spriteBatch = new SpriteBatch(graphicsDevice);
        cameraManager = new CameraManager(graphicsDevice);
        // Configure camera to maintain world scale during resizing
        cameraManager.SetMaintainWorldScale(true);
        
        // Set initial window size
        currentWindowSize = new Point(
            graphicsDevice.Viewport.Width,
            graphicsDevice.Viewport.Height
        );
    }
    
    public Point GetWindowSize() => currentWindowSize;

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