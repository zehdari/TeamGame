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
        CreateUI(assets);
        //CreateWorldBoundaries(screenWidth, screenHeight);
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

        entityFactory.CreateEntityFromConfig(UITextConfig);
        entityFactory.CreateEntityFromConfig(UIPausedConfig, ButtonSprite, ButtonAnim, inputConfig: UIInputConfig);
        entityFactory.CreateEntityFromConfig(UIHUDConfig, HUDSprite, HUDAnim);
    }
}