using ECS.Components.UI;
using ECS.Core;
using ECS.Events;
using System;
namespace ECS.Systems.UI;

public class UIUpdateSystem : SystemBase
{
    private readonly GameStateManager gameStateManager;
    private readonly Dictionary<string, Action<Entity>> keyActions;
    private readonly Dictionary<string, Action> buttonActions;
    public override bool Pausible => false;

    public UIUpdateSystem(GameStateManager gameStateManager)
    {
        this.gameStateManager = gameStateManager;

        keyActions = new Dictionary<string, Action<Entity>>
        {
            ["menu_up"] = (entity) => DecrementMenu(entity),
            ["menu_down"] = (entity) => IncrementMenu(entity),
            ["menu_enter"] = (entity) => ExecuteMenuOption(entity)
        };

        buttonActions = new Dictionary<string, Action>
        {
            ["reset"] = () => gameStateManager.Reset(),
            ["exit"] = () => gameStateManager.Exit(),
            ["pause"] = () => gameStateManager.TogglePause()
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

        ref var menu = ref GetComponent<UIMenu>(actionEvent.Entity);
        if (!menu.Active) return;

        if (keyActions.TryGetValue(actionEvent.ActionName, out var handler))
        {
            handler(actionEvent.Entity);
        }
    }
    private void DecrementMenu(Entity entity)
    {
        ref var currentMenu = ref GetComponent<UIMenu>(entity);

        var button = currentMenu.Buttons[currentMenu.Selected];
        button.Active = false;
        currentMenu.Buttons[currentMenu.Selected] = button;

        currentMenu.Selected--;
        if (currentMenu.Selected < 0)
        {
            currentMenu.Selected = currentMenu.Buttons.Count - 1;
        }

        button = currentMenu.Buttons[currentMenu.Selected];
        button.Active = true;
        currentMenu.Buttons[currentMenu.Selected] = button;
    }

    private void IncrementMenu(Entity entity)
    {
        ref var currentMenu = ref GetComponent<UIMenu>(entity);

        var button = currentMenu.Buttons[currentMenu.Selected];
        button.Active = false;
        currentMenu.Buttons[currentMenu.Selected] = button;

        currentMenu.Selected++;
        if (currentMenu.Selected >= currentMenu.Buttons.Count)
        {
            currentMenu.Selected = 0;
        }

        button = currentMenu.Buttons[currentMenu.Selected];
        button.Active = true;
        currentMenu.Buttons[currentMenu.Selected] = button;
    }

    private void ExecuteMenuOption(Entity entity)
    {
        ref var currentMenu = ref GetComponent<UIMenu>(entity);
        if (buttonActions.TryGetValue(currentMenu.Buttons[currentMenu.Selected].Action, out var handler))
        {
            handler();
        }
    }

    public override void Update(World world, GameTime gameTime)
    {
        foreach (var entity in World.GetEntities())
        {
            if (!HasComponents<UIMenu>(entity) || !HasComponents<UIPaused>(entity))
                continue;

            ref var menu = ref GetComponent<UIMenu>(entity);
            ref var paused = ref GetComponent<UIPaused>(entity);
            menu.Active = GameStateHelper.IsPaused(World) == paused.Value;

        }
    }
}