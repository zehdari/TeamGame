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
        public static string SHOOT { get; } = "shoot";
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
        public static string RUN { get; } = "run";
        public static string WALKLEFT { get; } = "walk_left";
        public static string WALKRIGHT { get; } = "walk_right";
        public static string SHOOT { get; } = "shoot";
        public static string ATTACK { get; } = "attack";
        public static string BLOCK { get; } = "block";
        public static string RESET { get; } = "reset";
        public static string EXIT { get; } = "exit";
        public static string MAIN_MENU { get; } = "main_menu";
        public static string PAUSE { get; } = "pause";
        public static string SWITCH_OBJECT_FORWARD { get; } = "switch_object_forward";
        public static string SWITCH_OBJECT_BACKWARD { get; } = "switch_object_backward";
        public static string SWITCH_CHARACTER_FORWARD { get; } = "switch_character_forward";
        public static string SWITCH_CHARACTER_BACKWARD { get; } = "switch_character_backward";
        public static string SWITCH_ITEM_FORWARD { get; } = "switch_item_forward";
        public static string SWITCH_ITEM_BACKWARD { get; } = "switch_item_backward";
        public static string SWITCH_LEVEL_FORWARD { get; } = "switch_level_forward";
        public static string DROP_THROUGH { get; } = "drop_through";
        public static string PICK_UP{ get; } = "pickup";
        public static string MENU_UP { get; } = "menu_up";
        public static string MENU_LEFT { get; } = "menu_left";
        public static string MENU_RIGHT { get; } = "menu_right";
        public static string MENU_DOWN { get; } = "menu_down";
        public static string MENU_ENTER { get; } = "menu_enter";
        public static string START_GAME { get; } = "start_game";
        public static string SETTINGS { get; } = "settings";

    }
    public static class GAMEPAD
    {
        public static string PLAYER_ONE { get; } = "PlayerOne";
        public static string PLAYER_TWO { get; } = "PlayerTwo";
        public static string PLAYER_THREE { get; } = "PlayerThree";
        public static string PLAYER_FOUR{ get; } = "PlayerFour";
        public static string ACCEPTS_ALL { get; } = "AcceptsAll";
    }
    public static class SPAWNED
    {
        public static string PLAYER { get; } = "player";
        public static string PROJECTILE { get; } = "projectile";
        public static string PEA { get; } = "pea";
        public static string SPLAT_PEA { get; } = "splat_pea";
    }

    public static class JSON_PARSING
    {
        public static string ACTIONS { get; } = "actions";
        public static string LEVEL_ELEMENTS { get; } = "level_elements";
        public static string STATES { get; } = "states";
        public static string X { get; } = "X";
        public static string Y { get; } = "Y";

    }

    public static class SOUND
    {
        public static string PUNCH { get; } = "Punch"; 
        public static string ERROR { get; } = "Error";
        public static string JUMP { get; } = "Jump";
        public static string DEATH { get; } = "Death";
        public static string POP { get; } = "Pop";
        public static string MENU { get; } = "BackgroundMusic";
    }
}

