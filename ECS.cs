using ECS.Core;

namespace ECS;

public class Game1 : Game
{
    private World world = new();
    private GameStateManager gameStateManager;
    private GameAssets assets;
    private GraphicsManager graphicsManager;
    private SoundManager soundManager;

    //private SoundEffect soundEffect;

    public Game1()
    {
        graphicsManager = new GraphicsManager(this);
        soundManager = new SoundManager(this, assets);
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

        //soundManager.Initialize();
        //soundEffect.Play();
        var backgroundMusic = assets.GetSound("BackgroundMusic");
        backgroundMusic.Play();

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