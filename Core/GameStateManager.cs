using ECS.Components.State;
using ECS.Components.Tags;
using ECS.Components.UI;
using ECS.Core.Utilities;

namespace ECS.Core;

public class GameStateManager
{
    private readonly World world;
    private readonly GameAssets assets;
    private readonly GameInitializer gameInitializer;
    private readonly LevelLoader levelLoader;
    private readonly GraphicsManager graphicsManager;
    private readonly Game game;
    private bool pendingReset = false;
    private bool pendingGameStart = false;
    private string currentLevel = "DayLevel";
    public GameStateManager(
        Game game,
        World world,
        GameAssets assets,
        GraphicsManager graphicsManager,
        LevelLoader levelLoader)
    {
        this.world = world;
        this.assets = assets;
        this.game = game;
        this.graphicsManager = graphicsManager;

        this.gameInitializer = new GameInitializer(world);
        this.levelLoader = levelLoader;

        // Initialize with main menu on construction
        Initialize();
    }

    public void Initialize(string level = null)
    {
        if (level != null)
        {
            currentLevel = level;
        }

        graphicsManager.SetGlobalShader(assets.GetEffect("BasicEffect"));

        TearDown();
        gameInitializer.InitializeGame(assets);
        
        // Set initial state to MainMenu
        GameStateHelper.SetGameState(world, GameState.MainMenu);
    }

    public void StartGame()
    {
        // Only set pendingGameStart if we're in MainMenu state
        if (GameStateHelper.GetGameState(world) == GameState.MainMenu)
        {
            pendingGameStart = true;
        }
    }

    public void ShowSettings()
    {
        // TBD
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
            levelLoader.MakeEntities(currentLevel);
            pendingReset = false;
        }
        
        if (pendingGameStart)
        {
            GameStateHelper.SetGameState(world, GameState.Running);
            levelLoader.MakeEntities(currentLevel);
            pendingGameStart = false;
        }
    }

    public void Reset()
    {
        TearDown();
        pendingReset = true;
    }

    public void TogglePause()
    {
        GameState currentState = GameStateHelper.GetGameState(world);
        
        // Only toggle between Running and Paused states
        if (currentState == GameState.Running || currentState == GameState.Paused)
        {
            GameStateHelper.SetGameState(
                world, 
                currentState == GameState.Paused ? GameState.Running : GameState.Paused
            );
        }
    }

    public void Exit()
    {
        GameStateHelper.SetGameState(world, GameState.Exit);
        game.Exit();
    }

    public void ReturnToMainMenu()
    {
        TearDown();
        GameStateHelper.SetGameState(world, GameState.MainMenu);
    }
}