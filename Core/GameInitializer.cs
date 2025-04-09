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

    public void InitializeGame(GameAssets assets)
    {
        CreateGameState(assets);
        CreateUI(assets);
    }

    private void CreateGameState(GameAssets assets)
    {
        entityFactory.CreateGameStateEntity(assets);
    }

    private void CreateUI(GameAssets assets)
    {
        entityFactory.CreateEntityFromKey(MAGIC.ASSETKEY.MAINMENU, assets);
        entityFactory.CreateEntityFromKey(MAGIC.ASSETKEY.LEVELMENU, assets);
        entityFactory.CreateEntityFromKey(MAGIC.ASSETKEY.CHARACTERMENU, assets);
        entityFactory.CreateEntityFromKey(MAGIC.ASSETKEY.CHARACTERMENU, assets);
        entityFactory.CreateEntityFromKey(MAGIC.ASSETKEY.CHARACTERMENU, assets);
        entityFactory.CreateEntityFromKey(MAGIC.ASSETKEY.CHARACTERMENU, assets);
        entityFactory.CreateEntityFromKey(MAGIC.ASSETKEY.PAUSEMENU, assets);
    }
}