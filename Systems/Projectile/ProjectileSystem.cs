using ECS.Components.AI;
using ECS.Components.Tags;
using ECS.Components.Timer;

namespace ECS.Systems.Projectile;

public class ProjectileSystem : SystemBase
{
    public override void Initialize(World world)
    {
        base.Initialize(world);
        Subscribe<TimerEvent>(HandleTimerUp);
    }

    private void HandleTimerUp(IEvent evt)
    {
        var timerEvent = (TimerEvent)evt;
        if (timerEvent.TimerType != TimerType.ProjectileTimer)
            return;

        if (!HasComponents<ProjectileTag>(timerEvent.Entity) ||
            !HasComponents<ExistedTooLong>(timerEvent.Entity))
            return;

        ref var existedTooLong = ref GetComponent<ExistedTooLong>(timerEvent.Entity);
        existedTooLong.Value = true;
    }

    public override void Update(World world, GameTime gameTime)
    {
        foreach (Entity entity in world.GetEntities())
        {
            if (!HasComponents<ProjectileTag>(entity) ||
                !HasComponents<ExistedTooLong>(entity))
                continue;

            ref var existedTooLong = ref GetComponent<ExistedTooLong>(entity);
            if (existedTooLong.Value)
            {
                Publish<DespawnEvent>(new DespawnEvent
                {
                    Entity = entity,
                });
            }
        }
    }
}
