using ECS.Core;
using ECS.Events;

namespace ECS.Systems;

public class WalkSystem : SystemBase
{

    public override void Initialize(World world)
    {
        base.Initialize(world);
        World.EventBus.Subscribe<ActionEvent>(HandleWalk);
    }


    private void HandleWalk(IEvent evt)
    {
        //TODO: Add other components to check for
        //checks to see if action event is walk
        var walkEvent = (ActionEvent)evt;
        if (!walkEvent.ActionName.equals("WALK"))
        {
            return;
        }
        //checks to see if it has these components so we can make sure we can get them
        if (!HasComponents<Force>(walkEvent.Entity) ||
            !HasComponents<WalkSpeed>(walkEvent.Entity))
            return;
        ref var force = ref GetComponent<Force>(walkEvent.Entity);
        ref var walk = ref GetComponent<WalkSpeed>(walkEvent.Entity);
        if (walk.IsHeld)
        {
            force.Value += walk.Value;
        }
    }
    public override void Update(World world, GameTime gameTime)
    {
    }
}