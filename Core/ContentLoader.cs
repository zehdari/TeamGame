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

        AssetManager.LoadEntityConfig(assets, "SunEntity", "Config/EntityConfig/sun.json");
        AssetManager.LoadEntityConfig(assets, "Player1", "Config/EntityConfig/player.json");
        AssetManager.LoadEntityConfig(assets, "AI", "Config/EntityConfig/enemy.json");
    }

    // Needs to be its own class TODO
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

        var sunConfig = assets.GetEntityConfig("SunEntity");
        var playerConfig = assets.GetEntityConfig("Player1");
        var AIConfig = assets.GetEntityConfig("AI");

        // Create players
        entityFactory.CreateEntityFromConfig(playerConfig, bonkChoy, bonkChoyAnim, player1Input);
        entityFactory.CreateEntityFromConfig(playerConfig, peashooter, peashooterAnim, player2Input);
        entityFactory.CreateEntityFromConfig(AIConfig, bonkChoy, bonkChoyAnim);
        entityFactory.CreateEntityFromConfig(AIConfig, peashooter, peashooterAnim);
        entityFactory.CreateEntityFromConfig(sunConfig, itemSprites, mapConfig);

        // Create world boundaries
        entityFactory.CreateWorldBoundaries(entityFactory, screenWidth, screenHeight);
    }
}

