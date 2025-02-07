


namespace ECS;

public class Game1 : Game
{
    private GraphicsDeviceManager graphics;
    private SpriteBatch spriteBatch;
    private World world;
    private EntityFactory entityFactory;
    private GameInitializer gameInitializer;

    public Game1()
    {
        graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        graphics.PreferredBackBufferWidth = 800;   // Game window width
        graphics.PreferredBackBufferHeight = 600;  // Game window height
        graphics.ApplyChanges();
    }

    protected override void Initialize()
    {
        world = new World();
        
        entityFactory = new EntityFactory(world);

        gameInitializer = new GameInitializer(world, entityFactory);

        SystemBuilder.BuildCoreSystems(world, entityFactory, this);

        base.Initialize();
    }

    protected override void LoadContent()
    {
        spriteBatch = new SpriteBatch(GraphicsDevice);
        
        // Load assets first
        GameAssets assets = AssetLoader.LoadAssets(Content);

        // Initialize game with loaded assets
        gameInitializer.InitializeGame(
            assets,
            graphics.PreferredBackBufferWidth,
            graphics.PreferredBackBufferHeight
        );

        SystemBuilder.BuildRenderSystems(world, spriteBatch, GraphicsDevice, assets);
    }


    protected override void Update(GameTime gameTime)
    {
        world.Update(gameTime);
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        world.Draw(gameTime);
        base.Draw(gameTime);
    }
}