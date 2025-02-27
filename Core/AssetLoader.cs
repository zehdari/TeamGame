namespace ECS.Core;

public static class AssetLoader
{
    public static GameAssets LoadAssets(ContentManager content)
    {
        var assets = new GameAssets();

        // Load all assets
        LoadSprites(content, assets);
        LoadConfigs(assets);
        RegisterCharacters();

        return assets;
    }

    private static void LoadSprites(ContentManager content, GameAssets assets)
    {
        AssetManager.LoadTexture(assets, content, "BonkChoySprite", "Sprites/bonk_choy_sprites");
        AssetManager.LoadTexture(assets, content, "PeashooterSprite", "Sprites/peashooter_sprites");
        AssetManager.LoadTexture(assets, content, "ItemSprites", "Sprites/item_sprites");
        AssetManager.LoadFont(assets, content, "DebugFont", "Fonts/DebugFont");

        AssetManager.LoadTexture(assets, content, "MapObjectSprite", "Sprites/object_sprites");

    }

    private static void LoadConfigs(GameAssets assets)
    {
        AssetManager.LoadSpriteSheet(assets, "BonkChoyAnimation", "Config/SpriteConfig/bonk_choy_spritesheet.json");
        AssetManager.LoadSpriteSheet(assets, "PeashooterAnimation", "Config/SpriteConfig/peashooter_spritesheet.json");
        AssetManager.LoadSpriteSheet(assets, "ItemAnimation", "Config/SpriteConfig/item_spritesheet.json");
        AssetManager.LoadSpriteSheet(assets, "ObjectAnimation", "Config/SpriteConfig/map_tiles_spritesheet.json");

        AssetManager.LoadInputConfig(assets, "Player1Input", "Config/InputConfig/player_input.json");
        AssetManager.LoadInputConfig(assets, "Player2Input", "Config/InputConfig/player2_input.json");
        AssetManager.LoadInputConfig(assets, "UI_Input", "Config/InputConfig/ui_input.json");

        AssetManager.LoadEntityConfig(assets, "PeaConfig", "Config/EntityConfig/pea.json");
        AssetManager.LoadEntityConfig(assets, "Sun", "Config/EntityConfig/sun.json");
        AssetManager.LoadEntityConfig(assets, "Fertilizer", "Config/EntityConfig/fertilizer.json");
        AssetManager.LoadEntityConfig(assets, "BonkChoyConfig", "Config/EntityConfig/bonk_choy.json");
        AssetManager.LoadEntityConfig(assets, "PeashooterConfig", "Config/EntityConfig/peashooter.json");
        AssetManager.LoadEntityConfig(assets, "UITextConfig", "Config/EntityConfig/ui_text.json");
        AssetManager.LoadEntityConfig(assets, "Platform", "Config/EntityConfig/wall.json");
        AssetManager.LoadEntityConfig(assets, "LittleLeftPlatformDay", "Config/MapConfig/little_left_platform_day.json");
        AssetManager.LoadEntityConfig(assets, "LittleRightPlatformDay", "Config/MapConfig/little_right_platform_day.json");
        AssetManager.LoadEntityConfig(assets, "BigPlatformDay", "Config/MapConfig/big_platform_day.json");
        AssetManager.LoadEntityConfig(assets, "LittleLeftPlatformNight", "Config/MapConfig/little_left_platform_night.json");
        AssetManager.LoadEntityConfig(assets, "LittleRightPlatformNight", "Config/MapConfig/little_right_platform_night.json");
        AssetManager.LoadEntityConfig(assets, "BigPlatformNight", "Config/MapConfig/big_platform_night.json");

        AssetManager.LoadLevelConfig(assets, "DayLevel", "Config/MapConfig/day_level.json");
        AssetManager.LoadLevelConfig(assets, "NightLevel", "Config/MapConfig/night_level.json");

    }

    private static void RegisterCharacters()
    {
        CharacterRegistry.RegisterCharacter(
            "bonk_choy", 
            "BonkChoySprite", 
            "BonkChoyAnimation",
            "BonkChoyConfig"
        );
        
        CharacterRegistry.RegisterCharacter(
            "peashooter", 
            "PeashooterSprite", 
            "PeashooterAnimation",
            "PeashooterConfig"
        );

        CharacterRegistry.RegisterCharacter(
            "pea",
            "ItemSprites",
            "ItemAnimation",
            "PeaConfig"
        );
        CharacterRegistry.RegisterCharacter(
            "little_left_platform_day",
            "MapObjectSprite",
            "ObjectAnimation",
            "LittleLeftPlatformDay"
         );
        CharacterRegistry.RegisterCharacter(
            "little_right_platform_day",
            "MapObjectSprite",
            "ObjectAnimation",
            "LittleRightPlatformDay"
         );
        CharacterRegistry.RegisterCharacter(
            "big_platform_day",
            "MapObjectSprite",
            "ObjectAnimation",
            "BigPlatformDay"
         );
        CharacterRegistry.RegisterCharacter(
            "little_left_platform_night",
            "MapObjectSprite",
            "ObjectAnimation",
            "LittleLeftPlatformNight"
         );
        CharacterRegistry.RegisterCharacter(
            "little_right_platform_night",
            "MapObjectSprite",
            "ObjectAnimation",
            "LittleRightPlatformNight"
         );
        CharacterRegistry.RegisterCharacter(
            "big_platform_night",
            "MapObjectSprite",
            "ObjectAnimation",
            "BigPlatformNight"
         );
    }
}
