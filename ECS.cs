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

        // Input Phase - Handle raw input and generate events
        //world.AddSystem(new InputEventSystem(this), SystemExecutionPhase.Input, 1);
        world.AddSystem(new RawInputSystem(), SystemExecutionPhase.Input, 1);
        world.AddSystem(new InputMappingSystem(), SystemExecutionPhase.Input, 2);
        

        // PreUpdate Phase - Handle input events and generate forces
        world.AddSystem(new JumpSystem(), SystemExecutionPhase.PreUpdate, 3);
        world.AddSystem(new WalkSystem(), SystemExecutionPhase.PreUpdate, 4);
        world.AddSystem(new RunSystem(), SystemExecutionPhase.PreUpdate, 4);
        world.AddSystem(new AirControlSystem(), SystemExecutionPhase.PreUpdate, 4);
        world.AddSystem(new ActionEventDebugSystem(), SystemExecutionPhase.PreUpdate, 1);

        // Update Phase - Core physics simulation
        world.AddSystem(new GravitySystem(), SystemExecutionPhase.Update, 1);
        world.AddSystem(new FrictionSystem(), SystemExecutionPhase.Update, 2);
        world.AddSystem(new AirResistanceSystem(), SystemExecutionPhase.Update, 3);
        world.AddSystem(new ForceSystem(), SystemExecutionPhase.Update, 4);
        world.AddSystem(new VelocitySystem(), SystemExecutionPhase.Update, 5);
        world.AddSystem(new PositionSystem(), SystemExecutionPhase.Update, 6);

        // PostUpdate Phase - Collision resolution and state updates
        world.AddSystem(new CollisionDetectionSystem(), SystemExecutionPhase.PostUpdate, 1);
        world.AddSystem(new CollisionResponseSystem(), SystemExecutionPhase.PostUpdate, 2);
        world.AddSystem(new FacingSystem(), SystemExecutionPhase.PostUpdate, 3);
        world.AddSystem(new AnimationSystem(), SystemExecutionPhase.PostUpdate, 4);

        // world.AddSystem(new DebugGroundedSystem(), SystemExecutionPhase.PostUpdate, 6);
        // world.AddSystem(new RawInputDebugSystem(), SystemExecutionPhase.PostUpdate, 4);
        // world.AddSystem(new ActionDebugSystem(), SystemExecutionPhase.PostUpdate, 5);
        base.Initialize();
    }

    protected override void LoadContent()
    {
        spriteBatch = new SpriteBatch(GraphicsDevice);
        
        // Add render system now that SpriteBatch is created
        world.AddSystem(new RenderSystem(spriteBatch), SystemExecutionPhase.Render, 0);

        // Add debug render system
        var debugFont = Content.Load<SpriteFont>("Fonts/DebugFont");
        //world.AddSystem(new DebugRenderSystem(spriteBatch, GraphicsDevice, debugFont), SystemExecutionPhase.Render, 1);
        

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

        entityFactory.CreatePlatform(
            new Vector2(400, 300),  // Position in middle of screen
            new Vector2(200, 20)
        );

        // Floor
        entityFactory.CreateBlock(
            new Vector2(400, 500),  
            new Vector2(800, 40)    
        );

        // Left Wall
        entityFactory.CreateBlock(
            new Vector2(0, 250),
            new Vector2(40, 500)
        );

        // Right Wall
        entityFactory.CreateBlock(
            new Vector2(800, 250),
            new Vector2(40, 500)
        );

        // Cieling
        entityFactory.CreateBlock(
            new Vector2(400, 0),
            new Vector2(840, 40) 
        );
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