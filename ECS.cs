namespace ECS;

public class Game1 : Game
{
    private World world = new();
    private GameStateManager gameStateManager;
    private GameAssets assets;
    private GraphicsManager graphicsManager;

    private SoundEffect soundEffect;

    public Game1()
    {
        graphicsManager = new GraphicsManager(this);
    }

    protected override void Initialize()
    {
        graphicsManager.Initialize();
        base.Initialize();
    }

    protected override void LoadContent()
    {
        assets = AssetLoader.LoadAssets(Content);

        gameStateManager = new GameStateManager(
            this,
            world,
            assets,
            graphicsManager
        );

        SystemBuilder.BuildSystems(world, gameStateManager, assets, graphicsManager);

        soundEffect = Content.Load<SoundEffect>("Sounds/trap-future-bass");
        soundEffect.Play();

    }

    protected override void Update(GameTime gameTime)
    {
        world.Update(gameTime);
        gameStateManager.Update();
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        world.Draw(gameTime, graphicsManager);
        base.Draw(gameTime);
    }
}   