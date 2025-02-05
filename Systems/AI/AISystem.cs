using ECS.Components.AI;
using ECS.Components.Physics;
using ECS.Components.Random;
using ECS.Components.Tags;
using ECS.Core;
using ECS.Events;

namespace ECS.Systems.AI
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
            ref var randomInt = ref GetComponent<RandomlyGeneratedInteger>(timerEvent.Entity);

            // This is here to reset walk system
            World.EventBus.Publish(new ActionEvent
            {
                ActionName = action.Value,
                Entity = timerEvent.Entity,
                IsStarted = false,
                IsEnded = false,
                IsHeld = false,
            });

            /* Assigns a random action to the AI */
            if(actions.TryGetValue(randomInt.Value, out string value))
                action.Value = value;

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
