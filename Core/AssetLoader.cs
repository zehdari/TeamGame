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
            "hitbox",
            null,
            null,
            "HitboxConfig"
        );
    }
}
