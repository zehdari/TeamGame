namespace ECS.Core;

public class GameStateManager
{
    private readonly World world;
    private readonly GameAssets assets;
    private readonly EntityFactory entityFactory;
    private readonly GameInitializer gameInitializer;
    private readonly int screenWidth;
    private readonly int screenHeight;
    private readonly Game game;
    private bool pendingReset = false;

    public GameStateManager(
        World world,
        GameAssets assets,
        EntityFactory entityFactory,
        Game game,
        int screenWidth,
        int screenHeight)
    {
        this.world = world;
        this.assets = assets;
        this.entityFactory = entityFactory;
        this.game = game;
        this.screenWidth = screenWidth;
        this.screenHeight = screenHeight;

        this.gameInitializer = new GameInitializer(world, entityFactory);
    }

    public void Initialize()
    {
        TearDown();
        gameInitializer.InitializeGame(assets, screenWidth, screenHeight);
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
            gameInitializer.InitializeGame(assets, screenWidth, screenHeight);
            pendingReset = false;
        }
    }

    public void Reset()
    {
        TearDown();
        pendingReset = true;
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