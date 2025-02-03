using ECS.Core;
using ECS.Events;

namespace ECS.Systems;

public class RunSystem : SystemBase
{

    public override void Initialize(World world)
    {
        base.Initialize(world);
        World.EventBus.Subscribe<ActionEvent>(HandleRun);
    }


    private void HandleRun(IEvent evt)
    {
        //checks to see if action event is run
        var runEvent = (ActionEvent)evt;
        if (!runEvent.ActionName.Equals("RUN"))
        {
            return;
        }
        //checks to see if it has these components so we can make sure we can get them
        if (!HasComponents<Force>(runEvent.Entity) ||
            !HasComponents<WalkForce>(runEvent.Entity) ||
            !HasComponents<RunSpeed>(runEvent.Entity))
            return;
        ref var force = ref GetComponent<Force>(runEvent.Entity);
        ref var walk = ref GetComponent<WalkForce>(runEvent.Entity);
        ref var run = ref GetComponent<RunSpeed>(runEvent.Entity);
        if (runEvent.IsHeld)
        {
            force.Value += new Vector2(walk.Value * run.Scalar, 0);
        }
    }
    public override void Update(World world, GameTime gameTime)
    {
    }
}