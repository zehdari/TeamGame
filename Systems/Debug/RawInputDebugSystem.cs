namespace ECS.Systems.Debug;

public class RawInputDebugSystem : SystemBase
{

    public override void Initialize(World world)
    {
        base.Initialize(world);
        World.EventBus.Subscribe<RawInputEvent>(HandleRawInput);
    }

    private void HandleRawInput(IEvent evt)
    {   
        var rawInput = (RawInputEvent)evt;
        Console.WriteLine($"[RawInput] Entity: {rawInput.Entity.Id} | " +
                         $"Key: {rawInput.RawKey} | " +
                         $"IsPressed: {rawInput.IsPressed}");
    }

    public override void Update(World world, GameTime gameTime) {}
}