using ECS.Resources;

namespace ECS.Core;

public static class ContentLoader
{
    public static class Sprites
    {
        // Character sprites
        public static Texture2D BonkChoy { get; private set; }
        public static Texture2D Peashooter { get; private set; }
        
        // Map/Item sprites
        public static Texture2D ItemSprites { get; private set; }
        
        // UI elements
        public static SpriteFont DebugFont { get; private set; }

        public static void LoadAll(ContentManager content)
        {
            // Load character sprites
            BonkChoy = content.Load<Texture2D>("Sprites/bonk_choy_sprites");
            Peashooter = content.Load<Texture2D>("Sprites/peashooter_sprites");
            
            // Load map/item sprites
            ItemSprites = content.Load<Texture2D>("Sprites/item_sprites");
            
            // Load fonts
            DebugFont = content.Load<SpriteFont>("Fonts/DebugFont");
        }
    }

    public static class Configs
    {
        // Character animations
        public static AnimationConfig BonkChoyAnimation { get; private set; }
        public static AnimationConfig PeashooterAnimation { get; private set; }
        
        // Item/Map configs
        public static AnimationConfig MapConfig { get; private set; }
        
        // Input configs
        public static InputConfig Player1Input { get; private set; }
        public static InputConfig Player2Input { get; private set; }

        public static void LoadAll()
        {
            // Load character animations
            BonkChoyAnimation = SpriteSheetLoader.LoadSpriteSheet(
                File.ReadAllText("Config/SpriteConfig/bonk_choy_spritesheet.json")
            );
            PeashooterAnimation = SpriteSheetLoader.LoadSpriteSheet(
                File.ReadAllText("Config/SpriteConfig/peashooter_spritesheet.json")
            );

            // Load map/item configs
            MapConfig = SpriteSheetLoader.LoadSpriteSheet(
                File.ReadAllText("Config/SpriteConfig/item_spritesheet.json")
            );

            // Load input configs
            Player1Input = InputConfigLoader.LoadInputConfig(
                File.ReadAllText("Config/InputConfig/player_input.json")
            );
            Player2Input = InputConfigLoader.LoadInputConfig(
                File.ReadAllText("Config/InputConfig/player2_input.json")
            );
        }
    }

    public static void LoadContent(
        ContentManager content, 
        EntityFactory entityFactory, 
        World world, 
        int screenWidth, 
        int screenHeight)
    {
        // Load all sprites and configs
        Sprites.LoadAll(content);
        Configs.LoadAll();

        // Create initial game entities
        CreateInitialEntities(entityFactory, world, screenWidth, screenHeight);
    }

    private static void CreateInitialEntities(
        EntityFactory entityFactory,
        World world,
        int screenWidth,
        int screenHeight)
    {
        // Create players with different sprites
        entityFactory.CreatePlayer(
            Sprites.BonkChoy, 
            Configs.BonkChoyAnimation, 
            Configs.Player1Input
        );
        entityFactory.CreatePlayer(
            Sprites.Peashooter, 
            Configs.PeashooterAnimation, 
            Configs.Player2Input
        );

        // Create all enemies
        entityFactory.CreateEnemy(
            Sprites.Peashooter,
            Configs.PeashooterAnimation
        );

        entityFactory.CreateEnemy(
            Sprites.BonkChoy,
            Configs.BonkChoyAnimation
        );

        // Create map objects
        entityFactory.CreateMapObject(
            tileName: "sun",
            position: new Vector2(100, 100),
            spriteSheet: Sprites.ItemSprites,
            tileConfig: Configs.MapConfig
        );

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