using ECS.Components.Input;
using ECS.Systems.Input;
using ECS.Events;
using System.Diagnostics;

namespace ECS.Systems.Debug;

public class GamePadDebugSystem : SystemBase
{

    public override void Initialize(World world)
    {
        base.Initialize(world);
        Subscribe<RawInputEvent>(HandleInput);
    }

    private void HandleInput(IEvent evt)
    {
        var rawInput = (RawInputEvent)evt;


        if (rawInput.IsGamepadInput)
        {
            if(rawInput.RawButton == Buttons.A) System.Diagnostics.Trace.WriteLine("A");
            if (rawInput.RawButton == Buttons.B) System.Diagnostics.Trace.WriteLine("B");
            if (rawInput.RawButton == Buttons.X) System.Diagnostics.Trace.WriteLine("X");
            if (rawInput.RawButton == Buttons.Y) System.Diagnostics.Trace.WriteLine("Y");
        }

        if (rawInput.IsTriggerInput)
        {
            if (rawInput.TriggerValue > 0) System.Diagnostics.Trace.WriteLine("TRIGGER!");
        }


    }

    public override void Update(World world, GameTime gameTime) {}
}