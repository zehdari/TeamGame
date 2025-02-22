using ECS.Components.Tags;
using ECS.Components.State;

namespace ECS.Core;

public class GameStateManager
{
    private readonly World world;
    private readonly GameAssets assets;
    private readonly EntityFactory entityFactory;
    private readonly GameInitializer gameInitializer;
    private readonly WindowManager windowManager;
    private readonly Game game;
    private bool pendingReset = false;

    public GameStateManager(
        Game game,
        World world,
        GameAssets assets,
        EntityFactory entityFactory,
        WindowManager windowManager)
    {
        this.world = world;
        this.assets = assets;
        this.entityFactory = entityFactory;
        this.game = game;
        this.windowManager = windowManager;

        this.gameInitializer = new GameInitializer(world, entityFactory);

        // Initialize game on construction
        Initialize();
    }

    public void Initialize()
    {
        TearDown();
        var windowSize = windowManager.GetWindowSize();
        gameInitializer.InitializeGame(assets, windowSize.X, windowSize.Y);
    }

    public void TearDown()
    {
        // Destroy all non-singleton entities
        var entities = world.GetEntities().ToList();
        foreach (var entity in entities)
        {
            if (!world.GetPool<SingletonTag>().Has(entity))
            {
                world.DestroyEntity(entity);
            }
        }
    }

    public void Update()
    {
        if (pendingReset)
        {
            var windowSize = windowManager.GetWindowSize();
            gameInitializer.InitializeGame(assets, windowSize.X, windowSize.Y);
            pendingReset = false;
        }
    }

    public void Reset()
    {
        TearDown();
        pendingReset = true;
    }

    public void TogglePause()
    {
        var gameStateEntity = world.GetEntities()
            .First(e => world.GetPool<GameStateComponent>().Has(e) && 
                       world.GetPool<SingletonTag>().Has(e));
        
        ref var state = ref world.GetPool<GameStateComponent>().Get(gameStateEntity);
        state.CurrentState = state.CurrentState == GameState.Paused 
            ? GameState.Running 
            : GameState.Paused;
    }

    public void Exit()
    {
        var gameStateEntity = world.GetEntities()
            .First(e => world.GetPool<GameStateComponent>().Has(e) && 
                       world.GetPool<SingletonTag>().Has(e));
        
        ref var state = ref world.GetPool<GameStateComponent>().Get(gameStateEntity);
        state.CurrentState = GameState.Exit;
        game.Exit();
    }
}