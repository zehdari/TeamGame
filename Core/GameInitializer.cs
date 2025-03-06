namespace ECS.Core;

public class GameInitializer
{
    private readonly World world;
    private readonly EntityFactory entityFactory;

    public GameInitializer(World world)
    {
        this.world = world;
        this.entityFactory = world.entityFactory;
    }

    public void InitializeGame(GameAssets assets, int screenWidth, int screenHeight)
    {
        CreateGameState();
        CreateObjects(assets);
        CreatePlayers(assets);
        CreateAI(assets);
        CreateUI(assets);
        CreateWorldBoundaries(screenWidth, screenHeight);
    }

    private void CreateGameState()
    {
        entityFactory.CreateGameStateEntity();
    }

    private void CreateUI(GameAssets assets)
    {
        var UIInputConfig = assets.GetInputConfig("UI_Input");
        var UITextConfig = assets.GetEntityConfig("UITextConfig");
        var UIPausedConfig = assets.GetEntityConfig("UIPauseConfig");
        var UIHUDConfig = assets.GetEntityConfig("UIHUDConfig");
        var HUDSprite = assets.GetTexture("HUDSprite");
        var ButtonSprite = assets.GetTexture("PauseButton");
        var HUDAnim = assets.GetAnimation("HUDAnimation");
        var ButtonAnim = assets.GetAnimation("PauseAnimation");

        entityFactory.CreateEntityFromConfig(UITextConfig, inputConfig: UIInputConfig);
        entityFactory.CreateEntityFromConfig(UIPausedConfig, ButtonSprite, ButtonAnim, inputConfig: UIInputConfig);
        entityFactory.CreateEntityFromConfig(UIHUDConfig, HUDSprite, HUDAnim, inputConfig: UIInputConfig);
    }

    private void CreateObjects(GameAssets assets)
    {
        var items = assets.GetTexture("ItemSprites");
        var itemsAnim = assets.GetAnimation("ItemAnimation");
        var sunConfig = assets.GetEntityConfig("Sun");

        var objects = assets.GetTexture("MapObjectSprite");
        var objectsAnim = assets.GetAnimation("ObjectAnimation");
        var platformConfig = assets.GetEntityConfig("Platform");

        entityFactory.CreateEntityFromConfig(sunConfig, items, itemsAnim);
        entityFactory.CreateEntityFromConfig(platformConfig, objects, objectsAnim);
    }
    private void CreatePlayers(GameAssets assets)
    {
        var bonkChoySprite = assets.GetTexture("BonkChoySprite");
        var peashooterSprite = assets.GetTexture("PeashooterSprite");
        var bonkChoyAnim = assets.GetAnimation("BonkChoyAnimation");
        var peashooterAnim = assets.GetAnimation("PeashooterAnimation");
        var player1Input = assets.GetInputConfig("Player1Input");
        var player2Input = assets.GetInputConfig("Player2Input");
        var peashooterConfig = assets.GetEntityConfig("PeashooterConfig");
        var bonkChoyConfig = assets.GetEntityConfig("BonkChoyConfig");

        entityFactory.CreatePlayerFromConfig(bonkChoyConfig, bonkChoySprite, bonkChoyAnim, player1Input, new Vector2(500,100));
        entityFactory.CreatePlayerFromConfig(peashooterConfig, peashooterSprite, peashooterAnim, player2Input, new Vector2(700,100));
    }

    private void CreateAI(GameAssets assets)
    {
        var bonkChoySprite = assets.GetTexture("BonkChoySprite");
        var peashooterSprite = assets.GetTexture("PeashooterSprite");
        var bonkChoyAnim = assets.GetAnimation("BonkChoyAnimation");
        var peashooterAnim = assets.GetAnimation("PeashooterAnimation");
        var peashooterConfig = assets.GetEntityConfig("PeashooterConfig");
        var bonkChoyConfig = assets.GetEntityConfig("BonkChoyConfig");

        entityFactory.CreateAIFromConfig(bonkChoyConfig, bonkChoySprite, bonkChoyAnim, new Vector2(300,100));
        entityFactory.CreateAIFromConfig(peashooterConfig, peashooterSprite, peashooterAnim, new Vector2(100,100));
    }

    private void CreateWorldBoundaries(int screenWidth, int screenHeight)
    {
        entityFactory.CreateWorldBoundaries(screenWidth, screenHeight);
    }
}