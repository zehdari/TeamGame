namespace ECS.Systems.Debug;

public class ActionDebugSystem : SystemBase
{

    public override void Initialize(World world)
    {
        base.Initialize(world);
        Subscribe<ActionEvent>(HandleAction);
    }

    private void HandleAction(IEvent evt)
    {   
        var action = (ActionEvent)evt;
        Console.WriteLine($"[Action] Entity: {action.Entity.Id} | " +
                         $"Action: {action.ActionName} | " +
                         $"IsStarted: {action.IsStarted} | " +
                         $"IsEnded: {action.IsEnded} | " +
                         $"IsHeld: {action.IsHeld}");
    }

    public override void Update(World world, GameTime gameTime) {}
}