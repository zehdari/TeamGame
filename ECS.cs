namespace ECS;

public class Game1 : Game
{
    private readonly GraphicsDeviceManager graphics;
    private SpriteBatch spriteBatch;
    private World world;
    private EntityFactory entityFactory;
    private GameStateManager gameStateManager;
    private GameAssets assets;
    private WindowManager windowManager;

    public Game1()
    {
        graphics = new GraphicsDeviceManager(this);
        windowManager = new WindowManager(this, graphics);
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
        assets = AssetLoader.LoadAssets(Content);

        gameStateManager = new GameStateManager(
            this,
            world,
            assets,
            entityFactory,
            windowManager
        );

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