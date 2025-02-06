using ECS.Resources;

namespace ECS.Core;
public static class ContentLoader
{
    public static GameAssets LoadContent(
        ContentManager content, 
        EntityFactory entityFactory, 
        World world, 
        int screenWidth, 
        int screenHeight)
    {
        var assets = new GameAssets();

        // Load all assets
        LoadSprites(content, assets);
        LoadConfigs(assets);

        // Create initial game entities
        CreateInitialEntities(entityFactory, world, screenWidth, screenHeight, assets);

        return assets;
    }


    private static void LoadSprites(ContentManager content, GameAssets assets)
    {
        AssetManager.LoadTexture(assets, content, "BonkChoySprite", "Sprites/bonk_choy_sprites");
        AssetManager.LoadTexture(assets, content, "PeashooterSprite", "Sprites/peashooter_sprites");
        AssetManager.LoadTexture(assets, content, "ItemSprites", "Sprites/item_sprites");
        AssetManager.LoadFont(assets, content, "DebugFont", "Fonts/DebugFont");
    }

    private static void LoadConfigs(GameAssets assets)
    {
        AssetManager.LoadSpriteSheet(assets, "BonkChoyAnimation", "Config/SpriteConfig/bonk_choy_spritesheet.json");
        AssetManager.LoadSpriteSheet(assets, "PeashooterAnimation", "Config/SpriteConfig/peashooter_spritesheet.json");
        AssetManager.LoadSpriteSheet(assets, "MapConfig", "Config/SpriteConfig/item_spritesheet.json");

        AssetManager.LoadInputConfig(assets, "Player1Input", "Config/InputConfig/player_input.json");
        AssetManager.LoadInputConfig(assets, "Player2Input", "Config/InputConfig/player2_input.json");
    }


    private static void CreateInitialEntities(
        EntityFactory entityFactory,
        World world,
        int screenWidth,
        int screenHeight,
        GameAssets assets)
    {

        entityFactory.CreateGameStateEntity();

        var bonkChoy = assets.GetTexture("BonkChoySprite");
        var peashooter = assets.GetTexture("PeashooterSprite");
        var itemSprites = assets.GetTexture("ItemSprites");

        var bonkChoyAnim = assets.GetAnimation("BonkChoyAnimation");
        var peashooterAnim = assets.GetAnimation("PeashooterAnimation");
        var mapConfig = assets.GetAnimation("MapConfig");

        var player1Input = assets.GetInputConfig("Player1Input");
        var player2Input = assets.GetInputConfig("Player2Input");

        // Create players
        entityFactory.CreatePlayer(bonkChoy, bonkChoyAnim, player1Input);
        entityFactory.CreatePlayer(peashooter, peashooterAnim, player2Input);

        // Create enemies
        entityFactory.CreateEnemy(peashooter, peashooterAnim);
        entityFactory.CreateEnemy(bonkChoy, bonkChoyAnim);

        // Create map objects
        entityFactory.CreateMapObject("sun", new Vector2(100, 100), itemSprites, mapConfig);

        // Create world boundaries
        CreateWorldBoundaries(entityFactory, screenWidth, screenHeight);
    }


    private static void CreateWorldBoundaries(EntityFactory entityFactory, int screenWidth, int screenHeight)
    {
        // Floor
        entityFactory.CreateLine(
            new Vector2(0, screenHeight), 
            new Vector2(screenWidth, screenHeight)
        ); 

        // Left wall
        entityFactory.CreateLine(
            new Vector2(0, 0), 
            new Vector2(0, screenHeight)
        ); 

        // Right wall
        entityFactory.CreateLine(
            new Vector2(screenWidth, 0), 
            new Vector2(screenWidth, screenHeight)
        );

        // Ceiling
        entityFactory.CreateLine(
            new Vector2(0, 0), 
            new Vector2(screenWidth, 0)
        ); 
    }
}

