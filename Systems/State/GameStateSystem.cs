namespace ECS.Systems.State;

public class GameStateSystem : SystemBase
{
    private readonly Game game;
    private readonly Dictionary<string, Action<GameStateComponent>> stateHandlers;

    public GameStateSystem(Game game)
    {
        this.game = game;
        stateHandlers = new Dictionary<string, Action<GameStateComponent>>
        {
            ["pause"] = component => {
                component.CurrentState = component.CurrentState == GameState.Paused ? 
                    GameState.Running : GameState.Paused;
                HandlePause(component.CurrentState);
            },
            ["reset"] = component => {
                component.CurrentState = GameState.Reset;
                HandleReset();
            },
            ["exit"] = component => {
                component.CurrentState = GameState.Exit;
                game.Exit();
            }
        };
    }

    public override void Initialize(World world)
    {
        base.Initialize(world);
        World.EventBus.Subscribe<ActionEvent>(HandleActionEvent);
    }

    private void HandleActionEvent(IEvent evt)
    {
        var actionEvent = (ActionEvent)evt;
        if (!actionEvent.IsStarted) return;

        var gameStateEntity = World.GetEntities()
            .First(e => HasComponents<GameStateComponent>(e) && HasComponents<SingletonTag>(e));

        ref var state = ref GetComponent<GameStateComponent>(gameStateEntity);
        if (stateHandlers.TryGetValue(actionEvent.ActionName, out var handler))
        {
            handler(state);
        }
    }

    private void HandlePause(GameState state)
    {
        // Pause will go here
    }

    private void HandleReset()
    {
        // Reset will go here
    }

    public override void Update(World world, GameTime gameTime) { }
}