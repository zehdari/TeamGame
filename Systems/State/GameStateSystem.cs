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
        if (!actionEvent.IsStarted) return;

        if (stateHandlers.TryGetValue(actionEvent.ActionName, out var handler))
        {
            handler();
        }
    }

    public override void Update(World world, GameTime gameTime) { }
}