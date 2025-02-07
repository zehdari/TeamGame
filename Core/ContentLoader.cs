public static class AssetLoader
{
    public static GameAssets LoadAssets(ContentManager content)
    {
        var assets = new GameAssets();

        // Load all assets
        LoadSprites(content, assets);
        LoadConfigs(assets);

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
        AssetManager.LoadSpriteSheet(assets, "ItemAnimation", "Config/SpriteConfig/item_spritesheet.json");

        AssetManager.LoadInputConfig(assets, "Player1Input", "Config/InputConfig/player_input.json");
        AssetManager.LoadInputConfig(assets, "Player2Input", "Config/InputConfig/player2_input.json");

        AssetManager.LoadEntityConfig(assets, "Sun", "Config/EntityConfig/sun.json");
        AssetManager.LoadEntityConfig(assets, "Player1", "Config/EntityConfig/player.json");
        AssetManager.LoadEntityConfig(assets, "AI", "Config/EntityConfig/enemy.json");
    }
}
