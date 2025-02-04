
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
                !HasComponents<CurrentAction>(timerEvent.Entity) ||
                !HasComponents<RandomlyGeneratedInteger>(timerEvent.Entity))
                return;

            ref var aiTag = ref GetComponent<AITag>(timerEvent.Entity);
            ref var action = ref GetComponent<CurrentAction>(timerEvent.Entity);
            ref var randomInt = ref GetComponent<RandomlyGeneratedFloat>(timerEvent.Entity);

            // Switch case incoming
            if (randomInt.Value == 0)
            {
                action.Value = "jump";
            }
            else if (randomInt.Value == 1)
            {
                action.Value = "walk_left";
            }
            else if (randomInt.Value == 2)
            {
                action.Value = "walk_right";
            }

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
                ref var action = ref GetComponent<CurrentAction>(entity);

                // Publish that 'I moved!' every update, as if a player was holding a key down.
                World.EventBus.Publish(new ActionEvent
                {
                    ActionName = action.Value,
                    Entity = entity,
                    IsStarted = true,
                    IsEnded = false,
                    IsHeld = true,
                });

            }
        }
    }
}
