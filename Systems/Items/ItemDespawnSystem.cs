using ECS.Components.Items;
using ECS.Components.Timer;
using ECS.Components.Tags;
using ECS.Core;

namespace ECS.Systems.Items;

public class ItemDespawnSystem : SystemBase
{
    public override void Initialize(World world)
    {
        base.Initialize(world);
        Subscribe<TimerEvent>(HandleTimerUp);
    }

    private void HandleTimerUp(IEvent evt)
    {
        var timerEvent = (TimerEvent)evt;
        if (timerEvent.TimerType != TimerType.ItemTimer)
            return;

        // Get the entity and check if it has an ItemTag
        if (HasComponents<ItemTag>(timerEvent.Entity))
        {
            // Despawn the item
            World.DestroyEntity(timerEvent.Entity);
        }
    }

    public override void Update(World world, GameTime gameTime) 
    {
        // Nothing to do in the update loop
    }
}