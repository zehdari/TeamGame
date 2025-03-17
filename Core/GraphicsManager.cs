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

    public Effect GlobalEffect { get; private set; }
    public bool ShaderEnabled { get; set; } = true;

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

    public void Begin(GameTime gameTime)
    {
        graphicsDevice.Clear(Color.CornflowerBlue);

        Effect currentEffect = GetCurrentEffect();
    
        // If using a shader with time parameter, update it
        if (currentEffect != null)
        {
            if (currentEffect.Parameters["TotalTime"] != null)
            {
                currentEffect.Parameters["TotalTime"].SetValue((float)gameTime.TotalGameTime.TotalSeconds);
            }
            
            if (currentEffect.Parameters["Resolution"] != null)
            {
                currentEffect.Parameters["Resolution"].SetValue(new Vector2(
                    graphicsDevice.Viewport.Width,
                    graphicsDevice.Viewport.Height
                ));
            }
        }
        
        spriteBatch.Begin(
            sortMode: SpriteSortMode.FrontToBack,
            samplerState: SamplerState.PointClamp,
            effect: currentEffect,
            blendState: BlendState.AlphaBlend,
            transformMatrix: GetTransformMatrix()
        );
    }
    
    public void End()
    {
        spriteBatch.End();
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

    public void SetGlobalShader(Effect effect)
    {
        GlobalEffect = effect;
    }
    
    // Add method to toggle shader on/off
    public void ToggleShader()
    {
        ShaderEnabled = !ShaderEnabled;
    }
    
    // Method to get the current effect (null if disabled)
    public Effect GetCurrentEffect()
    {
        return ShaderEnabled ? GlobalEffect : null;
    }
}