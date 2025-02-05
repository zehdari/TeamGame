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

        // Mapping actions to ints
        private Dictionary<int, string> actions = new();

        private void MappingSetter()
        {
            int i = 0;
            actions.Add(i++, "jump");
            actions.Add(i++, "walk_left");
            actions.Add(i++, "walk_right");
        }

        public override void Initialize(World world)
        {
            base.Initialize(world);
            World.EventBus.Subscribe<TimerEvent>(HandleTimerUp);
            MappingSetter();
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

            // This is here to reset walk system, should figure out a better way
            World.EventBus.Publish(new ActionEvent
            {
                ActionName = action.Value,
                Entity = timerEvent.Entity,
                IsStarted = false,
                IsEnded = false,
                IsHeld = false,
            });

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
                ref var action = ref GetComponent<CurrentAction>(entity);

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
