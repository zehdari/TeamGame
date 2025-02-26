using ECS.Components.AI;
using ECS.Components.Tags;
using ECS.Components.Timer;
using ECS.Core;

namespace ECS.Systems.Projectile;

public class HitboxDespawnSystem : SystemBase
{
    public override void Initialize(World world)
    {
        base.Initialize(world);
        Subscribe<TimerEvent>(HandleTimerUp);
    }

    private void HandleTimerUp(IEvent evt)
    {
        var timerEvent = (TimerEvent)evt;
        if (timerEvent.TimerType != TimerType.HitboxTimer)
            return;

        Publish<DespawnEvent>(new DespawnEvent
        {
            Entity = timerEvent.Entity,
        });
    }

    public override void Update(World world, GameTime gameTime) { }
}
