
using ECS.Events;

namespace ECS.Systems
{
    public class AISystem : SystemBase
    {
        public override void Initialize(World world)
        {
            base.Initialize(world);
            World.EventBus.Subscribe<TimerEvent>(HandleTimerUp);
        }

        private void HandleTimerUp(IEvent evt)
        {
            var timerEvent = (TimerEvent)evt;
            if (!HasComponents<AITag>(timerEvent.Entity) ||
                !HasComponents<Direction>(timerEvent.Entity) ||
                !HasComponents<RandomlyGeneratedFloat>(timerEvent.Entity))
                return;

            ref var aiTag = ref GetComponent<AITag>(timerEvent.Entity);
            ref var direction = ref GetComponent<Direction>(timerEvent.Entity);
            ref var randomFloat = ref GetComponent<RandomlyGeneratedFloat>(timerEvent.Entity);

            // Turn in a random direction within 360deg. Math here is in radians.
            direction.Value.Rotate(MathF.PI * 2f * randomFloat.Value);

        }

        public override void Update(World world, GameTime gameTime)
        {
            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            foreach (Entity entity in World.GetEntities())
            {
                if (!HasComponents<AITag>(entity))
                    continue;

                ref var aiTag = ref GetComponent<AITag>(entity);
                ref var direction = ref GetComponent<Direction>(entity);

                // Publish that 'I moved!' every update, as if a player was holding a key down.
                World.EventBus.Publish(new InputEvent
                {
                    MovementDirection = direction.Value,
                    Entity = entity,
                });
               
            }
        }
    }
}
