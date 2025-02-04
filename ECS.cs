


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
        world.AddSystem(new InputEventSystem(this), SystemExecutionPhase.Input, 1);

        // PreUpdate Phase - Handle input events and generate forces
        world.AddSystem(new RandomSystem(), SystemExecutionPhase.PreUpdate, 1);
        world.AddSystem(new TimerSystem(), SystemExecutionPhase.PreUpdate, 2);
        world.AddSystem(new AISystem(), SystemExecutionPhase.PreUpdate, 3);
        world.AddSystem(new ProjectileSystem(), SystemExecutionPhase.PreUpdate, 4); // This needs to move and change
        world.AddSystem(new PlayerMovementSystem(), SystemExecutionPhase.PreUpdate, 5);
        world.AddSystem(new FacingSystem(), SystemExecutionPhase.PreUpdate, 6);

        // Update Phase - Core physics simulation
        world.AddSystem(new JumpSystem(), SystemExecutionPhase.Update, 1);
        world.AddSystem(new GravitySystem(), SystemExecutionPhase.Update, 1);
        world.AddSystem(new FrictionSystem(), SystemExecutionPhase.Update, 2);
        world.AddSystem(new AirResistanceSystem(), SystemExecutionPhase.Update, 3);
        world.AddSystem(new ForceSystem(), SystemExecutionPhase.Update, 4);
        world.AddSystem(new VelocitySystem(), SystemExecutionPhase.Update, 5);
        world.AddSystem(new PositionSystem(), SystemExecutionPhase.Update, 6);

        // PostUpdate Phase - Collision resolution and state updates
        world.AddSystem(new CollisionDetectionSystem(), SystemExecutionPhase.PostUpdate, 1);
        world.AddSystem(new CollisionResponseSystem(), SystemExecutionPhase.PostUpdate, 2);
        world.AddSystem(new AnimationSystem(), SystemExecutionPhase.PostUpdate, 3);

        //world.AddSystem(new DebugGroundedSystem(), SystemExecutionPhase.PostUpdate, 4);
        world.AddSystem(new RawInputDebugSystem(), SystemExecutionPhase.PostUpdate, 4);

        base.Initialize();
    }

    protected override void LoadContent()
    {
        spriteBatch = new SpriteBatch(GraphicsDevice);
        
        // Add render system now that SpriteBatch is created
        world.AddSystem(new RenderSystem(spriteBatch), SystemExecutionPhase.Render, 0);
        world.AddSystem(new DebugRenderSystem(spriteBatch, GraphicsDevice), SystemExecutionPhase.Render, 1);
        

        // Load configurations
        var spriteSheet = Content.Load<Texture2D>("Sprites/blob_spritesheet");
        var animConfig = SpriteSheetLoader.LoadSpriteSheet(
            File.ReadAllText("Config/player_spritesheet.json")
        );
        var animConfig2 = SpriteSheetLoader.LoadSpriteSheet(
            File.ReadAllText("Config/blue_slime_spritesheet.json")
        );
        var animConfig3 = SpriteSheetLoader.LoadSpriteSheet(
            File.ReadAllText("Config/projectile_spritesheet.json")
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

        //for(int i = 0; i < 10; i++)
        entityFactory.CreateEnemy(spriteSheet, animConfig2);

        entityFactory.CreateProjectile(spriteSheet, animConfig3);

        entityFactory.CreateFloor(
            new Vector2(400, 500),  // Position in middle-bottom of screen
            new Vector2(800, 40)    // Wide rectangle for floor
        );

        entityFactory.CreatePlatform(
            new Vector2(400, 300),  // Position in middle of screen
            new Vector2(200, 20)
        );

        entityFactory.CreateLine(
            new Vector2(400, 100),
            new Vector2(600, 300)
        );

        // Right wall
        entityFactory.CreateLine(
            new Vector2(800, 0),
            new Vector2(800, 480)
        );

        // Left wall
        entityFactory.CreateLine(
            new Vector2(0, 0),
            new Vector2(0, 480)
        );

        // Ceiling
        entityFactory.CreateLine(
            new Vector2(0, 0),
            new Vector2(800, 0)
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