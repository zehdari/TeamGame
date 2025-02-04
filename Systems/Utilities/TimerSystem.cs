using ECS.Components.Timer;

namespace ECS.Systems.Utilities
{
    public class TimerSystem : SystemBase
    {
        public override void Update(World world, GameTime gameTime)
        {
            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            foreach (Entity entity in World.GetEntities())
            {
                if (!HasComponents<Timer>(entity))
                    continue;

                ref var timer = ref GetComponent<Timer>(entity);

                timer.Elapsed += deltaTime;

                /*
                 * If the timer is up, send a Timerevent and 'reset' the timer
                 */
                if (timer.Elapsed > timer.Duration)
                {
                    World.EventBus.Publish(new TimerEvent
                    {
                        Entity = entity
                    });

                    var difference = timer.Elapsed - timer.Duration;
                    timer.Elapsed = 0;
                    timer.Elapsed += difference;
                }
            }

        }
    }
}
