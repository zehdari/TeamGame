using ECS.Components.Input;
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
    private string currentLevel = MAGIC.LEVEL.DAY_LEVEL;
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

        TearDown();
        gameInitializer.InitializeGame(assets);
        
        // Set initial state to MainMenu
        GameStateHelper.SetGameState(world, GameState.MainMenu);
    }

    public void StartGame()
    {
        // Only set pendingGameStart if we're in CharacterSelect state
        if (GameStateHelper.GetGameState(world) == GameState.CharacterSelect)
        {
            pendingGameStart = true;
        }
    }

    public void StartLevelSelect()
    {
        GameStateHelper.SetGameState(world, GameState.LevelSelect);
    }
    public void StartCharacterSelect()
    {

        GameStateHelper.SetGameState(world, GameState.CharacterSelect);
        //reset player count
        var entities = world.GetEntities().ToList();
        var playerCounts = world.GetPool<PlayerCount>();
        var playerIndicators = world.GetPool<PlayerIndicators>();
        foreach (var entity in entities)
        {
            if (!playerCounts.Has(entity) || !playerIndicators.Has(entity))
                continue;

            ref var playerCount = ref playerCounts.Get(entity);
            ref var playerIndicator = ref playerIndicators.Get(entity);

            playerCount.Value = 0;
            for (var i = 0; i < playerIndicator.Values.Length; i++) {
                ref var indicator = ref playerIndicator.Values[i];
                indicator.Value = -1;
            }
        }
    }

    public void UpdateLevel(String level)
    {
        currentLevel = level;
    }

    public void UpdateCharacter(String character, bool ai)
    {
        if (ai)
        {
            levelLoader.SetAICharacter(character);
        }
        else
        {
            levelLoader.SetPlayerCharacter(character);
        }
    }

    public void ResetLobby()
    {
        currentLevel = MAGIC.LEVEL.DAY_LEVEL;
        levelLoader.ResetCharacters();
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
        if (GameStateHelper.IsMenu(world)) return;
        
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
        ResetLobby();
        GameStateHelper.SetGameState(world, GameState.MainMenu);
    }
}