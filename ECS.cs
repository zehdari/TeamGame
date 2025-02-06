


namespace ECS;

public class Game1 : Game
{
    private GraphicsDeviceManager graphics;
    private SpriteBatch spriteBatch;
    private World world;
    private EntityFactory entityFactory;

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

        SystemBuilder.BuildCoreSystems(world: world, entityFactory: entityFactory, game: this);

        base.Initialize();
    }

    protected override void LoadContent()
    {
        spriteBatch = new SpriteBatch(GraphicsDevice);
        
        GameAssets assets = ContentLoader.LoadContent(
            Content, 
            entityFactory, 
            world,
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