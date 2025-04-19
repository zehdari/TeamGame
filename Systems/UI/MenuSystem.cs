using ECS.Components.State;
using ECS.Components.Tags;
using ECS.Components.UI;
using ECS.Core;
using ECS.Core.Utilities;

namespace ECS.Systems.UI;

public class MenuSystem : SystemBase
{
    private readonly GameStateManager gameStateManager;
    private readonly Dictionary<string, Action<Entity>> keyActions;
    private readonly Dictionary<string, Action> buttonActions;
    private readonly Dictionary<string, Action<Entity>> characterButtonActions;
    private GameState previousGameState = GameState.Running;
    private float stateChangeTimer = 0f;
    private const float STATE_CHANGE_COOLDOWN = 0.2f; // 200ms cooldown
    private double lastHandledInputTime = 0f;
    
    public override bool Pausible => false;

    public MenuSystem(GameStateManager gameStateManager)
    {
        this.gameStateManager = gameStateManager;

        keyActions = new Dictionary<string, Action<Entity>>
        {
            [MAGIC.ACTIONS.MENU_UP] = (entity) => DecrementMenu(entity),
            [MAGIC.ACTIONS.MENU_DOWN] = (entity) => IncrementMenu(entity),
            [MAGIC.ACTIONS.MENU_LEFT] = (entity) => DecrementMenuColumn(entity),
            [MAGIC.ACTIONS.MENU_RIGHT] = (entity) => IncrementMenuColumn(entity),
            [MAGIC.ACTIONS.MENU_ENTER] = (entity) => ExecuteMenuOption(entity)
        };

        buttonActions = new Dictionary<string, Action>
        {
            // Main menu actions
            [MAGIC.ACTIONS.START_LOBBY] = () => gameStateManager.StartLevelSelect(),
            [MAGIC.ACTIONS.SETTINGS] = () => gameStateManager.ShowSettings(),

            // Pause menu actions
            [MAGIC.ACTIONS.PAUSE] = () => gameStateManager.TogglePause(),
            [MAGIC.ACTIONS.RESET] = () => gameStateManager.Reset(),
            [MAGIC.ACTIONS.MAIN_MENU] = () => gameStateManager.ReturnToMainMenu(),

            // Level menu actions
            [MAGIC.LEVEL.DAY_LEVEL] = () => gameStateManager.StartCharacterSelect(),
            [MAGIC.LEVEL.NIGHT_LEVEL] = () => gameStateManager.StartCharacterSelect(),
            [MAGIC.LEVEL.TEST_LEVEL] = () => gameStateManager.StartCharacterSelect(),
            [MAGIC.LEVEL.ROOF_LEVEL] = () => gameStateManager.StartCharacterSelect(),
            [MAGIC.LEVEL.DAY_LEVEL_ARENA] = () => gameStateManager.StartCharacterSelect(),
            [MAGIC.LEVEL.NIGHT_LEVEL_ARENA] = () => gameStateManager.StartCharacterSelect(),
            [MAGIC.LEVEL.NIGHT_ROOF] = () => gameStateManager.StartCharacterSelect(),

            // Common actions
            [MAGIC.ACTIONS.EXIT] = () => gameStateManager.Exit()
        };

        characterButtonActions = new Dictionary<string, Action<Entity>>
        {
            // Character menu actions
            [MAGIC.CHARACTERS.PEASHOOTER] = (entity) => NextCharacterMenu(entity),
            [MAGIC.CHARACTERS.BONK_CHOY] = (entity) => NextCharacterMenu(entity),
            [MAGIC.CHARACTERS.CHOMPER] = (entity) => NextCharacterMenu(entity),
            [MAGIC.CHARACTERS.ZOMBIE] = (entity) => NextCharacterMenu(entity),
            [MAGIC.LEVEL.AI] = (entity) => ToggleAI(entity),
            [MAGIC.ACTIONS.START_GAME] = (entity) => gameStateManager.StartGame()
        };
    }

    public override void Initialize(World world)
    {
        base.Initialize(world);
        Subscribe<ActionEvent>(HandleActionEvent);
    }

    private void HandleActionEvent(IEvent evt)
    {
        var actionEvent = (ActionEvent)evt;

        if (!actionEvent.IsStarted || !HasComponents<UIMenu>(actionEvent.Entity)) return;

        // Get current game state - only for cooldown check
        GameState currentState = GetCurrentGameState();
        if (currentState != previousGameState && stateChangeTimer < STATE_CHANGE_COOLDOWN)
            return;
            
        ref var menu = ref GetComponent<UIMenu>(actionEvent.Entity);
        if (!menu.Active) return;

        double seconds = (DateTime.Now - DateTime.Today).TotalSeconds;
        if (seconds < lastHandledInputTime + 0.15) return;
        lastHandledInputTime = seconds;

        if (keyActions.TryGetValue(actionEvent.ActionName, out var handler))
        {
            handler(actionEvent.Entity);
        }
    }

    private void DecrementMenu(Entity entity)
    {
        ref var currentMenu = ref GetComponent<UIMenu>(entity);

        SetButtonActive(currentMenu, false);

        currentMenu.Selected--;
        if (currentMenu.Selected < 0)
        {
            currentMenu.Selected = currentMenu.Buttons.Count - 1;
        }

        SetButtonActive(currentMenu, true);
    }

    private void IncrementMenu(Entity entity)
    {
        ref var currentMenu = ref GetComponent<UIMenu>(entity);

        SetButtonActive(currentMenu, false);

        currentMenu.Selected++;
        if (currentMenu.Selected >= currentMenu.Buttons.Count)
        {
            currentMenu.Selected = 0;
        }

        SetButtonActive(currentMenu, true);
    }

    private void DecrementMenuColumn(Entity entity)
    {
        if (!HasComponents<UIMenu2D>(entity)) return;
        ref var currentMenu2D = ref GetComponent<UIMenu2D>(entity);
        ref var currentMenu = ref GetComponent<UIMenu>(entity);

        currentMenu2D.Selected--;

        if (currentMenu2D.Selected < 0)
        {
            currentMenu2D.Selected = currentMenu2D.Menus.Count - 1;
        }

        ChangeCurrentMenu(currentMenu2D.Menus[currentMenu2D.Selected], ref currentMenu);
    }

    private void IncrementMenuColumn(Entity entity)
    {
        if (!HasComponents<UIMenu2D>(entity)) return;
        ref var currentMenu2D = ref GetComponent<UIMenu2D>(entity);
        ref var currentMenu = ref GetComponent<UIMenu>(entity);

        currentMenu2D.Selected++;
        
        if (currentMenu2D.Selected >= currentMenu2D.Menus.Count)
        {
            currentMenu2D.Selected = 0;
        }

        ChangeCurrentMenu(currentMenu2D.Menus[currentMenu2D.Selected], ref currentMenu);
    }

    private void ExecuteMenuOption(Entity entity)
    {
        ref var currentMenu = ref GetComponent<UIMenu>(entity);

        //only reset the current button if not on ai button or number of players is less than 4
        var button = currentMenu.Buttons[currentMenu.Selected];
        var resetSelection = button.Action != MAGIC.LEVEL.AI;
        if (HasComponents<PlayerCount>(entity))
        {
            ref var playerCount = ref GetComponent<PlayerCount>(entity);
            resetSelection = resetSelection && playerCount.Value < playerCount.MaxValue;
        }
        
        //Handle level select and character select specialties
        if (HasComponents<LevelSelectTag>(entity) && GameStateHelper.IsLevelSelect(World))
        {
            gameStateManager.UpdateLevel(button.Action);
        }
        if (HasComponents<CharacterSelectTag>(entity) && GameStateHelper.IsCharacterSelect(World))
        {
            if (characterButtonActions.TryGetValue(button.Action, out var handler))
            {
                handler(entity);
            }
            if (HasComponents<UIMenu2D>(entity) && resetSelection)
            {
                ref var menu2D = ref GetComponent<UIMenu2D>(entity);
                menu2D.Selected = 0;
                ResetColumnSelection(menu2D);
                ChangeCurrentMenu(menu2D.Menus[menu2D.Selected], ref currentMenu);
                //toggle ai if applicable
                if (HasComponents<AddAI>(entity))
                {
                    ref var addAI = ref GetComponent<AddAI>(entity);
                    addAI.Value = false;
                }
            }
        } 
        else if (buttonActions.TryGetValue(button.Action, out var handler))
        {
            handler();
        }
        if (resetSelection)
        {
            SetButtonActive(currentMenu, false);
            currentMenu.Selected = 0;
        }
        if (resetSelection)
        {
            SetButtonActive(currentMenu, true);
        }
    }

    private void NextCharacterMenu(Entity entity)
    {
        if (HasComponents<PlayerCount>(entity) && HasComponents<AddAI>(entity))
        {
            ref var menu = ref GetComponent<UIMenu>(entity);
            ref var playerCount = ref GetComponent<PlayerCount>(entity);
            ref var addAI = ref GetComponent<AddAI>(entity);

            var button = menu.Buttons[menu.Selected];
            
            if (playerCount.Value < playerCount.MaxValue)
            {
                playerCount.Value++;
                gameStateManager.UpdateCharacter(button.Action, addAI.Value);
            }
        }
    }

    private void ToggleAI(Entity entity)
    {
        if (HasComponents<AddAI>(entity))
        {
            ref var addAI = ref GetComponent<AddAI>(entity);
            addAI.Value = !addAI.Value;
            if (HasComponents<PlayerCount>(entity) && HasComponents<PlayerIndicators>(entity))
            {
                ref var indicators = ref GetComponent<PlayerIndicators>(entity);
                ref var playerCount = ref GetComponent<PlayerCount>(entity);
                //Update position of currently selecting player before drawing
                if (playerCount.Value < playerCount.MaxValue)
                {
                    ref var indicator = ref indicators.Values[playerCount.Value];
                    indicator.Value = addAI.Value ? 1 : 0;
                }
            }
        }
    }

    private void SetButtonActive(UIMenu menu, bool active)
    {
        var button = menu.Buttons[menu.Selected];
        button.Active = active;
        menu.Buttons[menu.Selected] = button;
    }

    private void ChangeCurrentMenu(UIMenu menuIn, ref UIMenu menuOut)
    {
        SetButtonActive(menuOut, false);
        menuOut.Separation = menuIn.Separation;
        menuOut.Buttons = menuIn.Buttons;
        if (menuOut.Selected >= menuOut.Buttons.Count)
        {
            menuOut.Selected = 0;
        }
        SetButtonActive(menuOut, true);
    }

    // Get the current game state using the helper
    private GameState GetCurrentGameState()
    {
        return GameStateHelper.GetGameState(World);
    }

    private void UpdateMenuActive(Entity entity, GameState currentState)
    {
        ref var menu = ref GetComponent<UIMenu>(entity);
        
        // Determine if this menu should be active based on tags and state
        bool shouldBeActive = false;
        
        if (HasComponents<MainMenuTag>(entity))
        {
            // Main menu is active only in MainMenu state
            shouldBeActive = GameStateHelper.IsMenu(World);
        }
        else if (HasComponents<LevelSelectTag>(entity))
        {
            //Level Select only active in LevelSelect state
            shouldBeActive = GameStateHelper.IsLevelSelect(World);
        }
        else if (HasComponents<CharacterSelectTag>(entity))
        {
            //Level Select only active in LevelSelect state
            shouldBeActive = GameStateHelper.IsCharacterSelect(World);
        }
        else if (HasComponents<UIPaused>(entity))
        {
            // Pause menu entities active based on UIPaused value and game state
            ref var pausedFlag = ref GetComponent<UIPaused>(entity);
            shouldBeActive = GameStateHelper.IsPaused(World) == pausedFlag.Value;
        }
        
        // No change needed
        if (menu.Active == shouldBeActive)
            return;
            
        // Update active state
        menu.Active = shouldBeActive;
        if (HasComponents<UIMenu2D>(entity))
        {
            ref var menu2D = ref GetComponent<UIMenu2D>(entity);
            menu2D.Selected = 0;
            ChangeCurrentMenu(menu2D.Menus[menu2D.Selected], ref menu);
        }
        
        // Only reset selection when activating a menu
        if (!shouldBeActive)
            return;
            
        // Reset selection to first item
        menu.Selected = 0;
        
        // Nothing to activate if no buttons
        if (menu.Buttons.Count <= 0)
            return;
            
        // Set first button active
        ResetButtonSelection(menu);
    }
    
    private void ResetButtonSelection(UIMenu menu)
    {
        for (int i = 0; i < menu.Buttons.Count; i++)
        {
            var button = menu.Buttons[i];
            button.Active = i == 0;
            menu.Buttons[i] = button;
        }
    }

    private void ResetColumnSelection(UIMenu2D menu)
    {
        for (int i = 0; i < menu.Menus.Count; i++)
        {
            var selected = menu.Menus[i];
            selected.Active = i == 0;
            menu.Menus[i] = selected;
        }
    }

    public override void Update(World world, GameTime gameTime)
    {
        // Get current game state
        GameState currentState = GetCurrentGameState();
        
        // Check for state change
        if (currentState != previousGameState)
        {
            // Reset timer when state changes
            stateChangeTimer = 0f;
            previousGameState = currentState;
        }
        else
        {
            // Increment timer when in same state
            stateChangeTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        // Update all menu entities based on the current game state
        foreach (var entity in World.GetEntities())
        {
            if (!HasComponents<UIMenu>(entity))
                continue;

            UpdateMenuActive(entity, currentState);
        }
    }
}