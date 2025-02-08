namespace ECS;

public class Game1 : Game
{
    private GraphicsDeviceManager graphics;
    private SpriteBatch spriteBatch;
    private World world;
    private EntityFactory entityFactory;
    private GameStateManager gameStateManager;
    private GameAssets assets;

    public Game1()
    {
        graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        graphics.PreferredBackBufferWidth = 800;
        graphics.PreferredBackBufferHeight = 600;
        graphics.ApplyChanges();
    }

    protected override void Initialize()
    {
        world = new World();
        entityFactory = new EntityFactory(world);
        base.Initialize();
    }

    protected override void LoadContent()
    {
        spriteBatch = new SpriteBatch(GraphicsDevice);
        
        // Load assets first
        assets = AssetLoader.LoadAssets(Content);

        // Create game state manager
        gameStateManager = new GameStateManager(
            world,
            assets,
            entityFactory,
            this,
            graphics.PreferredBackBufferWidth,
            graphics.PreferredBackBufferHeight
        );

        // Initialize game state
        gameStateManager.Initialize();

        // Build systems
        SystemBuilder.BuildSystems(world, entityFactory, gameStateManager, assets, spriteBatch, GraphicsDevice);
    }

    protected override void Update(GameTime gameTime)
    {
        world.Update(gameTime);
        gameStateManager.Update();
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);
        world.Draw(gameTime, spriteBatch);
        base.Draw(gameTime);
    }
}