using Microsoft.Xna.Framework.Audio;

namespace ECS.Core;

public static class AssetLoader
{
    public static GameAssets LoadAssets(ContentManager content)
    {
        var assets = new GameAssets();

        // Load all assets
        LoadSprites(content, assets);
        LoadSounds(content, assets);
        LoadConfigs(assets);
        RegisterEntities();

        return assets;
    }

    private static void LoadSprites(ContentManager content, GameAssets assets)
    {
        AssetManager.LoadTexture(assets, content, "BonkChoySprite", "Sprites/bonk_choy_sprites");
        AssetManager.LoadTexture(assets, content, "PeashooterSprite", "Sprites/peashooter_sprites");
        AssetManager.LoadTexture(assets, content, "ItemSprites", "Sprites/item_sprites");
        AssetManager.LoadFont(assets, content, "DebugFont", "Fonts/DebugFont");

        AssetManager.LoadTexture(assets, content, "MapObjectSprite", "Sprites/object_sprites");

        AssetManager.LoadTexture(assets, content, "HUDSprite", "Sprites/pvz_hud");
        AssetManager.LoadTexture(assets, content, "PauseButton", "Sprites/menu_option");

    }

    private static void LoadConfigs(GameAssets assets)
    {
        AssetManager.LoadSpriteSheet(assets, "BonkChoyAnimation", "Config/SpriteConfig/bonk_choy_spritesheet.json");
        AssetManager.LoadSpriteSheet(assets, "PeashooterAnimation", "Config/SpriteConfig/peashooter_spritesheet.json");
        AssetManager.LoadSpriteSheet(assets, "ItemAnimation", "Config/SpriteConfig/item_spritesheet.json");
        AssetManager.LoadSpriteSheet(assets, "ObjectAnimation", "Config/SpriteConfig/map_tiles_spritesheet.json");
        AssetManager.LoadSpriteSheet(assets, "HUDAnimation", "Config/SpriteConfig/hud_spritesheet.json");
        AssetManager.LoadSpriteSheet(assets, "PauseAnimation", "Config/SpriteConfig/pause_spritesheet.json");

        AssetManager.LoadInputConfig(assets, "Player1Input", "Config/InputConfig/player_input.json");
        AssetManager.LoadInputConfig(assets, "Player2Input", "Config/InputConfig/player2_input.json");
        AssetManager.LoadInputConfig(assets, "UI_Input", "Config/InputConfig/ui_input.json");

        AssetManager.LoadEntityConfig(assets, "PeaConfig", "Config/EntityConfig/pea.json");
        AssetManager.LoadEntityConfig(assets, "Sun", "Config/EntityConfig/sun.json");
        AssetManager.LoadEntityConfig(assets, "Fertilizer", "Config/EntityConfig/fertilizer.json");
        AssetManager.LoadEntityConfig(assets, "BonkChoyConfig", "Config/EntityConfig/bonk_choy.json");
        AssetManager.LoadEntityConfig(assets, "PeashooterConfig", "Config/EntityConfig/peashooter.json");
        AssetManager.LoadEntityConfig(assets, "Platform", "Config/EntityConfig/wall.json");

        AssetManager.LoadEntityConfig(assets, "UITextConfig", "Config/UIConfig/ui_text.json");
        AssetManager.LoadEntityConfig(assets, "UIPauseConfig", "Config/UIConfig/ui_pause.json");
        AssetManager.LoadEntityConfig(assets, "UIHUDConfig", "Config/UIConfig/ui_hud.json");
        AssetManager.LoadEntityConfig(assets, "HitboxConfig", "Config/EntityConfig/hitbox.json");
    }

    private static void RegisterEntities()
    {
        EntityRegistry.RegisterEntity(
            "bonk_choy", 
            "BonkChoySprite", 
            "BonkChoyAnimation",
            "BonkChoyConfig"
        );
        
        EntityRegistry.RegisterEntity(
            "peashooter", 
            "PeashooterSprite", 
            "PeashooterAnimation",
            "PeashooterConfig"
        );

        EntityRegistry.RegisterEntity(
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

    private static void LoadSounds(ContentManager content, GameAssets assets)
    {
        AssetManager.LoadSound(assets, content, "BackgroundMusic", "Sounds/trap-future-bass");
    }
}
