namespace ECS.Core.Utilities;

public static class MAGIC
{
    public static class LEVEL
    {   
        public static string DAY_LEVEL { get; } = "DayLevel";
        public static string NIGHT_LEVEL { get; } = "NightLevel";
        public static string TEST_LEVEL { get; } = "TestLevel";
        public static string ROOF_LEVEL { get; } = "Roof";
        public static string DAY_LEVEL_ARENA { get; } = "DayLevelArena";
        public static string NIGHT_LEVEL_ARENA { get; } = "NightLevelArena";
        public static string PLAYERS { get; } = "players";
        public static string PLATFORMS { get; } = "platforms";
        public static string ITEMS { get; } = "items";
        public static string UI { get; } = "ui";
        public static string AI { get; } = "ai";
        public static string BACKGROUND { get; } = "background";
    }
    public static class UTILS
    {
        public static string WORLD { get; } = "world";
        public static string ENTITY { get; } = "entity";
        public static string VALUE { get; } = "value";
        public static string GETPOOL { get; } = "GetPool";
        public static string SET { get; } = "Set";
    }
    public static class ANIMATIONSTATE
    {
        public static string IDLE{ get; } = "idle";
        public static string ATTACK { get; } = "attack";
    }
    public static class CONFIG
    {
        public static string CONTENT { get; } = "Content";
    }
    public static class ASSETKEY
    {
        public static string GAMESTATE { get; } = "game_state";
        public static string LEVELMENU { get; } = "level_menu";
        public static string MAINMENU { get; } = "main_menu";
        public static string PAUSEMENU { get; } = "pause_menu";
    }
    public static class METHODTYPES
    {
        public static string GET { get; } = "Get";
    }
    public static class ACTIONS
    {
        public static string JUMP { get; } = "jump";
        public static string WALKLEFT { get; } = "walk_left";
        public static string WALKRIGHT { get; } = "walk_right";
        public static string SHOOT { get; } = "shoot";
        public static string ATTACK { get; } = "attack";
    }

}

