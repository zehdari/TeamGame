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
    }

    protected override void Initialize()
    {
        world = new World();
        entityFactory = new EntityFactory(world);

        // Add systems in proper phases with priorities
        world.AddSystem(new InputEventSystem(this), SystemExecutionPhase.Input, 1);
        world.AddSystem(new MovementSystem(), SystemExecutionPhase.Update, 2);
        world.AddSystem(new FacingSystem(), SystemExecutionPhase.Update, 3);
        world.AddSystem(new AnimationSystem(), SystemExecutionPhase.Update, 4);

        base.Initialize();
    }

    protected override void LoadContent()
    {
        spriteBatch = new SpriteBatch(GraphicsDevice);
        
        // Add render system now that SpriteBatch is created
        world.AddSystem(new RenderSystem(spriteBatch), SystemExecutionPhase.Render, 0);

        // Load configurations
        var spriteSheet = Content.Load<Texture2D>("Sprites/blob_spritesheet");
        var animConfig = SpriteSheetLoader.LoadSpriteSheet(
            File.ReadAllText("Config/player_spritesheet.json")
        );
        var inputConfig = InputConfigLoader.LoadInputConfig(
            File.ReadAllText("Config/player_input.json")
        );

        var inputConfig2 = InputConfigLoader.LoadInputConfig(
            File.ReadAllText("Config/player2_input.json")
        );

        // Create player with configurations
        entityFactory.CreatePlayer(spriteSheet, animConfig, inputConfig);
        entityFactory.CreatePlayer(spriteSheet, animConfig, inputConfig2);
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