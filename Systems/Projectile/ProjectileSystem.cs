namespace ECS.Systems.Projectile;

public class ProjectileSystem : SystemBase
{

    public override void Initialize(World world)
    {
        base.Initialize(world);
        World.EventBus.Subscribe<TimerEvent>(HandleTimerUp);
    }

    private void HandleTimerUp(IEvent evt)
    {
        var timerEvent = (TimerEvent)evt;
        if (!HasComponents<ProjectileTag>(timerEvent.Entity) ||
            !HasComponents<ExistedTooLong>(timerEvent.Entity) ||
            !HasComponents<Timer>(timerEvent.Entity))
            return;

        ref var existedTooLong = ref GetComponent<ExistedTooLong>(timerEvent.Entity);

        existedTooLong.Value = true;

    }

    public override void Update(World world, GameTime gameTime)
    {
        var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        foreach (Entity entity in World.GetEntities())
        {
            if (!HasComponents<ProjectileTag>(entity) ||
                !HasComponents<ExistedTooLong>(entity))
                continue;

            ref var existedTooLong = ref GetComponent<ExistedTooLong>(entity);

            if (existedTooLong.Value)
            {
                world.DestroyEntity(entity);
            }

        }
    }
}

