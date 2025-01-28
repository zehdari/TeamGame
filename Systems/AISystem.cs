
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
            if (!HasComponents<AITag>(timerEvent.Entity))
                return;

            ref var aiTag = ref GetComponent<AITag>(timerEvent.Entity);

            // TODO: This RNG should probably be moved to a singleton for the whole game or some other place. Also, is casting here fine?
            Random rnd = new Random();
            aiTag.CurrentDirection.Rotate(MathF.PI * (float)rnd.NextDouble());

        }

        public override void Update(World world, GameTime gameTime)
        {
            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            foreach (Entity entity in World.GetEntities())
            {
                if (!HasComponents<AITag>(entity))
                    continue;

                ref var aiTag = ref GetComponent<AITag>(entity);

                // Should probably be some 'Should I be moving?' check here, but fine for this super dumb simple 'AI'.
                World.EventBus.Publish(new InputEvent
                {
                    MovementDirection = aiTag.CurrentDirection,
                    Entity = entity,
                });
               
            }
        }
    }
}
