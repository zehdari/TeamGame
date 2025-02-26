namespace ECS.Core;

public class GameInitializer
{
    private readonly World world;
    private readonly EntityFactory entityFactory;

    public GameInitializer(World world, EntityFactory entityFactory)
    {
        this.world = world;
        this.entityFactory = entityFactory;
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
        var HUDAnim = assets.GetAnimation("HUDAnimation");

        entityFactory.CreateEntityFromConfig(UITextConfig, inputConfig: UIInputConfig);
        entityFactory.CreateEntityFromConfig(UIPausedConfig, inputConfig: UIInputConfig);
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

        entityFactory.CreatePlayerFromConfig(bonkChoyConfig, bonkChoySprite, bonkChoyAnim, player1Input);
        entityFactory.CreatePlayerFromConfig(peashooterConfig, peashooterSprite, peashooterAnim, player2Input);
    }

    private void CreateAI(GameAssets assets)
    {
        var bonkChoySprite = assets.GetTexture("BonkChoySprite");
        var peashooterSprite = assets.GetTexture("PeashooterSprite");
        var bonkChoyAnim = assets.GetAnimation("BonkChoyAnimation");
        var peashooterAnim = assets.GetAnimation("PeashooterAnimation");
        var peashooterConfig = assets.GetEntityConfig("PeashooterConfig");
        var bonkChoyConfig = assets.GetEntityConfig("BonkChoyConfig");

        entityFactory.CreateAIFromConfig(bonkChoyConfig, bonkChoySprite, bonkChoyAnim);
        entityFactory.CreateAIFromConfig(peashooterConfig, peashooterSprite, peashooterAnim);
    }

    private void CreateWorldBoundaries(int screenWidth, int screenHeight)
    {
        entityFactory.CreateWorldBoundaries(entityFactory, screenWidth, screenHeight);
    }
}