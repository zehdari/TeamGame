namespace ECS.Core;

public class WindowManager
{
    private readonly GraphicsDeviceManager graphics;
    private readonly Point defaultResolution = new(800, 600);

    public WindowManager(Game game, GraphicsDeviceManager graphics)
    {
        this.graphics = graphics;
        
        graphics.PreferredBackBufferWidth = defaultResolution.X;
        graphics.PreferredBackBufferHeight = defaultResolution.Y;
        graphics.ApplyChanges();
        
        game.Content.RootDirectory = "Content";
        game.IsMouseVisible = true;
    }
    
    public Point GetWindowSize() => defaultResolution;
}