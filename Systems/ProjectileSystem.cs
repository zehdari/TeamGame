
using ECS.Components;
using ECS.Events;

namespace ECS.Systems
{
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
                    !HasComponents<Direction>(entity) ||
                    !HasComponents<ExistedTooLong>(entity) ||
                    !HasComponents<Timer>(entity))
                    continue;

                ref var existedTooLong = ref GetComponent<ExistedTooLong>(entity);
                ref var direction = ref GetComponent<Direction>(entity);

                if (existedTooLong.Value)
                {
                    world.DestroyEntity(entity);
                }
                else
                {
                    World.EventBus.Publish(new InputEvent
                    {
                        MovementDirection = direction.Value,
                        Entity = entity,
                    });

                }

            }

        }
    }
}
