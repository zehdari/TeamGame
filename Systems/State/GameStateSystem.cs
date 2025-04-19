using ECS.Components.Characters;
using ECS.Components.Physics;
using ECS.Components.PVZ;
using ECS.Components.State;
using ECS.Components.Timer;
using ECS.Components.UI;
using System.Security.Cryptography;

namespace ECS.Systems.State;

public class GameStateSystem : SystemBase
{
    private readonly GameStateManager gameStateManager;
    private readonly Dictionary<string, Action> stateHandlers;
    public override bool Pausible => false;

    public GameStateSystem(GameStateManager gameStateManager)
    {
        this.gameStateManager = gameStateManager;

        stateHandlers = new Dictionary<string, Action>
        {
            [MAGIC.ACTIONS.RESET] = () => gameStateManager.Reset(),
            [MAGIC.ACTIONS.EXIT] = () => gameStateManager.Exit(),
            [MAGIC.ACTIONS.PAUSE] = () => gameStateManager.TogglePause()
        };
    }

    public override void Initialize(World world)
    {
        base.Initialize(world);
        Subscribe<ActionEvent>(HandleActionEvent);
        Subscribe<TimerEvent>(HandleWinTimer);
    }

    private void HandleActionEvent(IEvent evt)
    {
        var actionEvent = (ActionEvent)evt;
        if (!actionEvent.IsStarted) return;

        if (stateHandlers.TryGetValue(actionEvent.ActionName, out var handler))
        {
            handler();
        }
    }

    // Win Timer Management
    private void HandleWinTimer(IEvent evt)
    {
        var timerEvent = (TimerEvent)evt;

        if (timerEvent.TimerType != TimerType.WinTimer)
            return;

        var entity = timerEvent.Entity;
        if (HasComponents<UIText>(entity))
        {
            ref var text = ref GetComponent<UIText>(entity);
            text.Text = "";
        }
        
        gameStateManager.ReturnToMainMenu();
    }

    public override void Update(World world, GameTime gameTime)
    {
        if (gameStateManager.IsWin())
            return;
        int count = 0;
        foreach (var entity in world.GetEntities())
        {
            if (!HasComponents<Percent>(entity) || HasComponents<PvZTag>(entity))
                continue;

            count++;
            
            ref var name = ref GetComponent<CharacterConfig>(entity);
            gameStateManager.UpdateWinner(name.Value);
        }

        if (count == 1)
        {
            gameStateManager.Win();
        }
    }
}