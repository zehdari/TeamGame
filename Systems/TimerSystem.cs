
namespace ECS.Systems
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
                 * If the timer is up, sent a Timerevent and 'reset' the timer
                 */
                if(timer.Elapsed > timer.Time)
                {
                    World.EventBus.Publish(new TimerEvent
                    {
                        Entity = entity
                    });

                    var difference = timer.Elapsed - timer.Time;
                    timer.Elapsed = 0;
                    timer.Elapsed += difference;
                }
            }

        }
    }
}
